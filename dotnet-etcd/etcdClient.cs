// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
        if (callInvoker == null)
        {
            throw new ArgumentNullException(nameof(callInvoker));
        }

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

        if (connectionString.StartsWith(AlternateDnsPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            connectionString = connectionString.Substring(AlternateDnsPrefix.Length);
            connectionString = DnsPrefix + connectionString;
        }

        SocketsHttpHandler httpHandler = new()
        {
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
        };

        // Connection Configuration
        GrpcChannelOptions options = new()
        {
            ServiceConfig = new ServiceConfig
            {
                MethodConfigs = { _defaultGrpcMethodConfig },
                RetryThrottling = _defaultRetryThrottlingPolicy,
                LoadBalancingConfigs = { new RoundRobinConfig() }
            },
            HttpHandler = httpHandler,
            DisposeHttpClient = true,
            ThrowOperationCanceledOnCancellation = true,
            Credentials = ChannelCredentials.Insecure // Default to insecure for backward compatibility
        };

        configureChannelOptions?.Invoke(options);

        // Channel Configuration
        if (connectionString.StartsWith(DnsPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            _channel = GrpcChannel.ForAddress(connectionString, options);
        }
        else
        {
            string[] hosts = connectionString.Split(',');
            List<Uri> nodes = new();

            foreach (string host in hosts)
            {
                string processedHost = host.Trim();

                // Only append port if no port is specified and it's not a full URL
                if (!processedHost.Contains(':') &&
                    !processedHost.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) &&
                    !processedHost.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    processedHost += $":{Convert.ToString(port, CultureInfo.InvariantCulture)}";
                }

                if (!(processedHost.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) ||
                      processedHost.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase)))
                {
                    processedHost = options.Credentials == ChannelCredentials.Insecure
                        ? $"{InsecurePrefix}{processedHost}"
                        : $"{SecurePrefix}{processedHost}";
                }

                nodes.Add(new Uri(processedHost));
            }

            StaticResolverFactory factory =
                new(addr => nodes.Select(i => new BalancerAddress(i.Host, i.Port)).ToArray());
            ServiceCollection services = new();
            services.AddSingleton<ResolverFactory>(factory);
            options.ServiceProvider = services.BuildServiceProvider();

            _channel = GrpcChannel.ForAddress($"{StaticHostsPrefix}{serverName}", options);
        }

        CallInvoker callInvoker = interceptors != null && interceptors.Length > 0
            ? _channel.Intercept(interceptors)
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
    ///     Initializes a new instance of the <see cref="EtcdClient" /> class for testing.
    /// </summary>
    /// <param name="connection">The connection to use</param>
    /// <param name="watchManager">The watch manager to use</param>
    /// <exception cref="ArgumentNullException">Thrown if connection or watchManager is null</exception>
    public EtcdClient(IConnection connection, IWatchManager watchManager = null)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));

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

    #region IDisposable Support

    private bool _disposed; // To detect redundant calls

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
