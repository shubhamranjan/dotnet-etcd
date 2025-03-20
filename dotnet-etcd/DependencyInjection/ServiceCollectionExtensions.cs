// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using dotnet_etcd.interfaces;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet_etcd.DependencyInjection;

/// <summary>
/// Extension methods for setting up etcd related services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds etcd client services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">The connection string for etcd.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="serverName">The server name.</param>
    /// <param name="configureChannel">Optional action to configure channel options.</param>
    /// <param name="interceptors">Optional interceptors to apply to calls.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEtcdClient(
        this IServiceCollection services,
        string connectionString,
        int port = 2379,
        string serverName = "my-etcd-server",
        Action<GrpcChannelOptions> configureChannel = null,
        Interceptor[] interceptors = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var options = new EtcdClientOptions
        {
            ConnectionString = connectionString,
            Port = port,
            ServerName = serverName,
            ConfigureChannel = configureChannel,
            Interceptors = interceptors
        };

        // Validate the options
        EtcdClientOptionsValidator.ValidateOptions(options);

        return AddEtcdClient(services, options);
    }

    /// <summary>
    /// Adds etcd client services to the specified <see cref="IServiceCollection" /> with the specified configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configureClient">An action to configure the etcd client options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEtcdClient(
        this IServiceCollection services,
        Action<EtcdClientOptions> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureClient);
        
        var options = new EtcdClientOptions();
        configureClient(options);
        
        // Validate the options
        EtcdClientOptionsValidator.ValidateOptions(options);
        
        return AddEtcdClient(services, options);
    }

    /// <summary>
    /// Adds etcd client services to the specified <see cref="IServiceCollection" /> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="options">The options to configure the etcd client.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEtcdClient(
        this IServiceCollection services,
        EtcdClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Validate the options
        EtcdClientOptionsValidator.ValidateOptions(options);

        // Create a wrapper action that applies both the options and any custom channel configuration
        Action<GrpcChannelOptions> channelConfiguration = grpcOptions => 
        {
            options.ApplyTo(grpcOptions);
        };

        // Register EtcdClient as a singleton
        services.TryAddSingleton<IEtcdClient>(serviceProvider => 
            (IEtcdClient)new EtcdClient(
                options.ConnectionString, 
                options.Port, 
                options.ServerName,
                channelConfiguration,
                options.Interceptors));

        // Register IWatchManager for direct access if needed
        services.TryAddTransient(serviceProvider => 
        {
            var etcdClient = serviceProvider.GetRequiredService<IEtcdClient>();
            return (etcdClient as EtcdClient)?.GetWatchManager();
        });

        return services;
    }

    /// <summary>
    /// Adds etcd client services to the specified <see cref="IServiceCollection" /> using custom factory functions.
    /// This allows for more advanced configuration and testing scenarios.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionFactory">Factory function to create a connection.</param>
    /// <param name="watchManagerFactory">Optional factory function to create a watch manager.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEtcdClient(
        this IServiceCollection services,
        Func<IServiceProvider, IConnection> connectionFactory,
        Func<IServiceProvider, IWatchManager> watchManagerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionFactory);

        // Register EtcdClient as a singleton
        services.TryAddSingleton<IEtcdClient>(serviceProvider =>
        {
            var connection = connectionFactory(serviceProvider);
            
            if (watchManagerFactory != null)
            {
                var watchManager = watchManagerFactory(serviceProvider);
                return (IEtcdClient)new EtcdClient(connection, watchManager);
            }
            
            return (IEtcdClient)new EtcdClient(connection);
        });

        // If a watch manager factory is provided, also register IWatchManager directly
        if (watchManagerFactory != null)
        {
            services.TryAddSingleton(watchManagerFactory);
        }
        else
        {
            // Register IWatchManager for direct access if needed
            services.TryAddTransient(serviceProvider => 
            {
                var etcdClient = serviceProvider.GetRequiredService<IEtcdClient>();
                return (etcdClient as EtcdClient)?.GetWatchManager();
            });
        }

        return services;
    }
}