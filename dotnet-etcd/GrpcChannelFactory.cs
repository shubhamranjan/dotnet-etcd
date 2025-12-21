using System;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet_etcd;

/// <summary>
///     Factory for creating gRPC channels
/// </summary>
public class GrpcChannelFactory
{
    private static readonly MethodConfig DefaultGrpcMethodConfig = new()
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

    private static readonly RetryThrottlingPolicy DefaultRetryThrottlingPolicy = new()
    {
        MaxTokens = 10,
        TokenRatio = 0.1
    };

    private const string StaticHostsPrefix = "static://";

    /// <summary>
    ///     Creates a gRPC channel with the specified configuration
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="port">The port to use</param>
    /// <param name="serverName">The server name</param>
    /// <param name="credentials">The credentials to use</param>
    /// <param name="configureChannelOptions">Optional configuration for channel options</param>
    /// <param name="configureSslOptions">Optional configuration for SSL options (for custom SSL certificates)</param>
    /// <returns>A gRPC channel</returns>
    public GrpcChannel CreateChannel(
        string connectionString, 
        int port, 
        string serverName, 
        ChannelCredentials credentials,
        Action<GrpcChannelOptions> configureChannelOptions = null,
        Action<SslClientAuthenticationOptions> configureSslOptions = null)
    {
        var parser = new ConnectionStringParser();
        var (uris, isDnsConnection) = parser.ParseConnectionString(connectionString, port, credentials);

        var httpHandler = new SocketsHttpHandler
        {
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
        };

        // Configure SSL options if provided - this is the correct way for gRPC .NET
        if (configureSslOptions != null)
        {
            var sslOptions = new SslClientAuthenticationOptions();
            configureSslOptions(sslOptions);
            httpHandler.SslOptions = sslOptions;
        }

        var options = new GrpcChannelOptions
        {
            ServiceConfig = new ServiceConfig
            {
                MethodConfigs = { DefaultGrpcMethodConfig },
                RetryThrottling = DefaultRetryThrottlingPolicy,
                LoadBalancingConfigs = { new RoundRobinConfig() }
            },
            HttpHandler = httpHandler,
            DisposeHttpClient = true,
            ThrowOperationCanceledOnCancellation = true,
            Credentials = credentials
        };

        configureChannelOptions?.Invoke(options);

        if (isDnsConnection)
        {
            return GrpcChannel.ForAddress(uris[0].ToString(), options);
        }

        var factory = new StaticResolverFactory(addr => uris.Select(i => new BalancerAddress(i.Host, i.Port)).ToArray());
        var services = new ServiceCollection();
        services.AddSingleton<ResolverFactory>(factory);
        options.ServiceProvider = services.BuildServiceProvider();

        return GrpcChannel.ForAddress($"{StaticHostsPrefix}{serverName}", options);
    }
}
