using System;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.multiplexer;
using Etcdserverpb;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

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

    private (string username, string password)? _credentials;
    private AuthenticationHttpHandler _authHttpHandler;

    // ETCD Tokens are valid for 5 min per default, so we cache them for the 5 min - 1 min safety margin.
    private TimeSpan _tokenCacheDuration = TimeSpan.FromMinutes(4);

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
            RetryableStatusCodes = { StatusCode.Unavailable },
        },
    };

    // https://github.com/grpc/proposal/blob/master/A6-client-retries.md#throttling-retry-attempts-and-hedged-rpcs
    private static readonly RetryThrottlingPolicy _defaultRetryThrottlingPolicy = new()
    {
        MaxTokens = 10,
        TokenRatio = 0.1,
    };

    #endregion

    #region Initializers

    /// <summary>
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class with a CallInvoker.
    ///     Automatic authentication is not configured for this overload — the caller is responsible
    ///     for attaching any auth credentials to the supplied CallInvoker.
    /// </summary>
    /// <param name="callInvoker">The call invoker to use for gRPC calls</param>
    /// <exception cref="ArgumentNullException">Thrown if callInvoker is null</exception>
    public EtcdClient(CallInvoker callInvoker)
    {
        ArgumentNullException.ThrowIfNull(callInvoker);

        _connection = new Connection(callInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken)
        );

        // Initialize the watch manager
        _watchManager = new WatchManager(
            (headers, deadline, cancellationToken) =>
                _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken)
        );
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
    public EtcdClient(
        string connectionString,
        int port = 2379,
        string serverName = DefaultServerName,
        Action<GrpcChannelOptions> configureChannelOptions = null,
        Interceptor[] interceptors = null
    )
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
            options =>
            {
                _authHttpHandler = new AuthenticationHttpHandler(
                    RequestTokenAsync,
                    options.HttpHandler!
                );
                options.HttpHandler = _authHttpHandler;
                configureChannelOptions?.Invoke(options);
            },
            configureSslOptions: null // No custom SSL by default
        );

        CallInvoker callInvoker =
            interceptors.Length > 0
                ? _channel.Intercept(interceptors)
                : _channel.CreateCallInvoker();

        // Setup Connection
        _connection = new Connection(callInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken)
        );

        // Initialize the watch manager
        _watchManager = new WatchManager(
            (headers, deadline, cancellationToken) =>
                _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken)
        );
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
    public EtcdClient(
        string connectionString,
        Action<SslClientAuthenticationOptions> configureSslOptions,
        int port = 2379,
        string serverName = DefaultServerName,
        Action<GrpcChannelOptions> configureChannelOptions = null,
        Interceptor[] interceptors = null
    )
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
        var credentials =
            configureSslOptions != null ? new SslCredentials() : ChannelCredentials.Insecure;

        _channel = channelFactory.CreateChannel(
            connectionString,
            port,
            serverName,
            credentials,
            options =>
            {
                _authHttpHandler = new AuthenticationHttpHandler(
                    RequestTokenAsync,
                    options.HttpHandler!
                );
                options.HttpHandler = _authHttpHandler;
                configureChannelOptions?.Invoke(options);
            },
            configureSslOptions
        );
        CallInvoker callInvoker =
            interceptors.Length > 0
                ? _channel.Intercept(interceptors)
                : _channel.CreateCallInvoker();

        // Setup Connection
        _connection = new Connection(callInvoker);

        // Create the watch call factory
        _watchCallFactory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(
            (headers, deadline, cancellationToken) =>
                _connection.WatchClient.Watch(headers, deadline, cancellationToken)
        );

        // Initialize the watch manager
        _watchManager = new WatchManager(
            (headers, deadline, cancellationToken) =>
                _watchCallFactory.CreateDuplexStreamingCall(headers, deadline, cancellationToken)
        );
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
    /// <param name="tokenCacheDuration">The duration to cache requested tokens from etcd</param>
    /// <exception cref="ArgumentNullException">Thrown if connectionString is null or empty</exception>
    public EtcdClient(
        string connectionString,
        string username,
        string password,
        int port = 2379,
        string serverName = DefaultServerName,
        Action<GrpcChannelOptions> configureChannelOptions = null,
        TimeSpan? tokenCacheDuration = null
    )
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

        if (tokenCacheDuration.HasValue)
            _tokenCacheDuration = tokenCacheDuration.Value;

        _credentials = (username, password);
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
                    _connection.WatchClient.Watch(headers, deadline, cancellationToken)
            );

            // Initialize the watch manager with default implementation
            _watchManager = new WatchManager(
                (headers, deadline, cancellationToken) =>
                    _watchCallFactory.CreateDuplexStreamingCall(
                        headers,
                        deadline,
                        cancellationToken
                    )
            );
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
    /// <param name="tokenCacheDuration">The duration to cache requested tokens from etcd</param>
    /// <exception cref="ArgumentNullException">Thrown if username or password is null or empty</exception>
    public void SetCredentials(
        string username,
        string password,
        TimeSpan? tokenCacheDuration = null
    )
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (tokenCacheDuration.HasValue)
            _tokenCacheDuration = tokenCacheDuration.Value;

        _credentials = (username, password);

        // Purge any token cached under the previous credentials so the next request re-auths.
        _authHttpHandler?.InvalidateToken();
    }

    /// <summary>
    ///     Clears the current credentials if set and invalidates any cached token.
    /// </summary>
    public void ClearCredentials()
    {
        _credentials = null;
        _authHttpHandler?.InvalidateToken();
    }

    private async Task<(string token, TimeSpan cacheDuration)?> RequestTokenAsync(
        CancellationToken cancellationToken
    )
    {
        (string username, string password)? cred = _credentials;
        if (!cred.HasValue)
            return null;

        AuthenticateResponse response = await _connection
            .AuthClient.AuthenticateAsync(
                new AuthenticateRequest
                {
                    Name = cred.Value.username,
                    Password = cred.Value.password,
                },
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        return (response.Token, _tokenCacheDuration);
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
