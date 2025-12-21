using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using dotnet_etcd.interfaces;
using dotnet_etcd.multiplexer;
using Etcdserverpb;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet_etcd;

/// <summary>
///     Etcd client is the entrypoint for this library.
///     It contains all the functions required to perform operations on etcd.
/// </summary>
public partial class EtcdClient : IDisposable, IEtcdClient
{
    /// <summary>
    ///     Gets the connection object
    /// </summary>
    /// <returns>The connection object</returns>
    public IConnection GetConnection() => _connection;

    /// <summary>
    ///     Gets the watch manager
    /// </summary>
    /// <returns>The watch manager</returns>
    public IWatchManager GetWatchManager() => _watchManager;

    /// <summary>
    ///     Cancels a watch request
    /// </summary>
    /// <param name="watchId">The ID of the watch to cancel</param>
    public void CancelWatch(long watchId) => _watchManager.CancelWatch(watchId);

    /// <summary>
    ///     Cancels multiple watch requests
    /// </summary>
    /// <param name="watchIds">The IDs of the watches to cancel</param>
    public void CancelWatch(long[] watchIds)
    {
        foreach (long watchId in watchIds)
        {
            _watchManager.CancelWatch(watchId);
        }
    }

    #region Variables

    private const string InsecurePrefix = "http://";
    private const string SecurePrefix = "https://";

    private const string StaticHostsPrefix = "static://";
    private const string DnsPrefix = "dns://";
    private const string AlternateDnsPrefix = "discovery-srv://";

    private const string DefaultServerName = "my-etcd-server";

    internal readonly IConnection _connection;
    private readonly GrpcChannel _channel;
    private readonly IWatchManager _watchManager;
    private readonly AsyncStreamCallFactory<WatchRequest, WatchResponse> _watchCallFactory;

    private string _username;
    private string _password;
    private string _authToken;
    private readonly object _authLock = new object();

    // https://learn.microsoft.com/en-us/aspnet/core/grpc/retries?view=aspnetcore-6.0#configure-a-grpc-retry-policy
    private static readonly MethodConfig _defaultGrpcMethodConfig = new()
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = 5,
            InitialBackoff = TimeSpan.FromSeconds(1),
            MaxBackoff = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    };

    // https://github.com/grpc/proposal/blob/master/A6-client-retries.md#throttling-retry-attempts-and-hedged-rpcs
    private static readonly RetryThrottlingPolicy _defaultRetryThrottlingPolicy = new()
    {
        MaxTokens = 10, TokenRatio = 0.1
    };

    #endregion

    #region Initializers

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class with a CallInvoker.
    /// </summary>
    /// <param name="callInvoker">The call invoker to use for gRPC calls</param>
    /// <exception cref="ArgumentNullException">Thrown if callInvoker is null</exception>
    public EtcdClient(CallInvoker callInvoker)
    {
        ArgumentNullException.ThrowIfNull(callInvoker);

        // Add authentication interceptor - it passively reads the cached token
        var authInterceptor = new AuthenticationInterceptor(() => _authToken);
        var interceptedCallInvoker = callInvoker.Intercept(authInterceptor);

        _connection = new Connection(interceptedCallInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken));

        // Initialize the watch manager
        _watchManager = new WatchManager((headers, deadline, cancellationToken) =>
            _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class with a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for etcd</param>
    /// <param name="port">The port to connect to</param>
    /// <param name="serverName">The server name</param>
    /// <param name="configureChannelOptions">Function to configure channel options</param>
    /// <param name="interceptors">Interceptors to apply to calls</param>
    /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty</exception>
    public EtcdClient(string connectionString, int port = 2379, string serverName = DefaultServerName,
        Action<GrpcChannelOptions> configureChannelOptions = null, Interceptor[] interceptors = null)
    {
        // Param check
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        // Param sanitization
        interceptors ??= Array.Empty<Interceptor>();

        var channelFactory = new GrpcChannelFactory();
        _channel = channelFactory.CreateChannel(
            connectionString,
            port,
            serverName,
            ChannelCredentials.Insecure, // Default to insecure for backward compatibility
            configureChannelOptions,
            configureSslOptions: null); // No custom SSL by default

        // Always add authentication interceptor first (it will be a no-op if credentials aren't set)
        var authInterceptor = new AuthenticationInterceptor(() => _authToken);
        var allInterceptors = new[] { authInterceptor }.Concat(interceptors).ToArray();

        CallInvoker callInvoker = allInterceptors.Length > 0
            ? _channel.Intercept(allInterceptors)
            : _channel.CreateCallInvoker();

        // Setup Connection
        _connection = new Connection(callInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken));

        // Initialize the watch manager
        _watchManager = new WatchManager((headers, deadline, cancellationToken) =>
            _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class with a connection string and SSL options configuration.
    ///     Use this constructor to configure custom SSL certificates for self-signed certificates.
    /// </summary>
    /// <param name="connectionString">The connection string for etcd</param>
    /// <param name="configureSslOptions">Action to configure SSL options for custom SSL certificates</param>
    /// <param name="port">The port to connect to</param>
    /// <param name="serverName">The server name</param>
    /// <param name="configureChannelOptions">Function to configure channel options</param>
    /// <param name="interceptors">Interceptors to apply to calls</param>
    /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty</exception>
    public EtcdClient(string connectionString, Action<SslClientAuthenticationOptions> configureSslOptions, int port = 2379, 
       string serverName = DefaultServerName, Action<GrpcChannelOptions> configureChannelOptions = null, 
        Interceptor[] interceptors = null)
    {
        // Param check
       if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        // Param sanitization
        interceptors ??= Array.Empty<Interceptor>();

        var channelFactory = new GrpcChannelFactory();
        
        // Use SSL credentials when SSL options configuration is provided
        var credentials = configureSslOptions != null 
            ? new SslCredentials() 
            : ChannelCredentials.Insecure;
        
        _channel = channelFactory.CreateChannel(
            connectionString,
            port,
            serverName,
            credentials,
            configureChannelOptions,
            configureSslOptions);

        // Always add authentication interceptor first (it will be a no-op if credentials aren't set)
        var authInterceptor = new AuthenticationInterceptor(() => _authToken);
        var allInterceptors = new[] { authInterceptor }.Concat(interceptors).ToArray();

        CallInvoker callInvoker = allInterceptors.Length > 0
            ? _channel.Intercept(allInterceptors)
            : _channel.CreateCallInvoker();

        // Setup Connection
        _connection = new Connection(callInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken));

        // Initialize the watch manager
        _watchManager = new WatchManager((headers, deadline, cancellationToken) =>
            _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class with a connection string and credentials.
    /// </summary>
    /// <param name="connectionString">The connection string for etcd</param>
    /// <param name="username">The username for authentication</param>
    /// <param name="password">The password for authentication</param>
    /// <param name="port">The port to connect to</param>
    /// <param name="serverName">The server name</param>
    /// <param name="configureChannelOptions">Function to configure channel options</param>
    /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty</exception>
    public EtcdClient(string connectionString, string username, string password, int port = 2379,
        string serverName = DefaultServerName, Action<GrpcChannelOptions> configureChannelOptions = null)
        : this(connectionString, port, serverName, configureChannelOptions)
    {
        // Validate and set credentials
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        _username = username;
        _password = password;
        
        // Authenticate immediately with the provided credentials
        AuthenticateAndCacheToken();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class for testing.
    /// </summary>
    /// <param name="connection">The connection to use</param>
    /// <param name="watchManager">The watch manager to use</param>
    /// <exception cref="ArgumentNullException">Thrown if connection or watchManager is null</exception>
    public EtcdClient(IConnection connection, IWatchManager watchManager = null)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;

        if (watchManager != null)
        {
            _watchManager = watchManager;
        }
        else
        {
            // Create the watch call factory
            _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
                (headers, deadline, cancellationToken) =>
                    _connection.WatchClient.Watch(headers, deadline, cancellationToken));

            // Initialize the watch manager with default implementation
            _watchManager = new WatchManager((headers, deadline, cancellationToken) =>
                _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken));
        }
    }

    #endregion

    #region Authentication

    /// <summary>
    ///     Sets the credentials for automatic authentication with etcd.
    ///     When credentials are set, all subsequent requests will automatically include the authentication token.
    /// </summary>
    /// <param name="username">The username for authentication</param>
    /// <param name="password">The password for authentication</param>
    /// <exception cref="ArgumentNullException">Thrown if username or password is null or empty</exception>
    public void SetCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        lock (_authLock)
        {
            _username = username;
            _password = password;
            _authToken = null; // Reset token to force re-authentication on next request
        }
        
        // Authenticate immediately with the new credentials
        AuthenticateAndCacheToken();
    }

    /// <summary>
    ///     Authenticates with etcd and caches the token.
    ///     This method is called immediately when credentials are set (constructor or SetCredentials).
    /// </summary>
    private void AuthenticateAndCacheToken()
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            return;
        }

        lock (_authLock)
        {
            try
            {
                var authRequest = new AuthenticateRequest
                {
                    Name = _username,
                    Password = _password
                };

                // Call authenticate directly on the connection's auth client
                // The interceptor won't interfere because it only reads the cached token
                var authResponse = _connection.AuthClient.Authenticate(authRequest, null, null, default);
                _authToken = authResponse.Token;
            }
            catch
            {
                // Clear token on failure
                _authToken = null;
                throw;
            }
        }
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    /// <summary>
    ///     Disposes the resources used by this instance
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _channel?.Dispose();
                _watchManager?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    ///     Disposes the resources used by this instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
