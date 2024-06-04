// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using dotnet_etcd.interfaces;
using dotnet_etcd.options;
using Etcdserverpb;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using V3Lockpb;

namespace dotnet_etcd.extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions class.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds etcd client.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="etcdOptions"><see cref="EtcdOptions"/>.</param>
        /// <returns><see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddEtcdGrpcClient(this IServiceCollection services, EtcdOptions etcdOptions,
            Action<GrpcChannelOptions> configureChannelOptionsAction = default, bool useSsl = false)
        {
            services
                .AddGrpcClient<KV.KVClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Watch.WatchClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Lease.LeaseClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Lock.LockClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Cluster.ClusterClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Maintenance.MaintenanceClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .AddGrpcClient<Auth.AuthClient>(ConfigureGrpcClient(etcdOptions, configureChannelOptionsAction, useSsl));

            services
                .TryAddTransient<IEtcdClient, EtcdClient>();

            return services;
        }

        private static Action<GrpcClientFactoryOptions> ConfigureGrpcClient(
            EtcdOptions etcdOptions,
            Action<GrpcChannelOptions> configureChannelOptionsAction,
            bool useSsl) => o =>
        {
            string address = ResolveAddress(etcdOptions);

            o.Address = new Uri(address);

            o.ChannelOptionsActions.Add(options =>
            {
                options.ServiceConfig = new ServiceConfig
                {
                    MethodConfigs = { DefaultGrpcMethodConfig },
                    RetryThrottling = DefaultRetryThrottlingPolicy,
                    LoadBalancingConfigs = { new RoundRobinConfig() },
                };

                options.Credentials = useSsl ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure;

                options.ServiceProvider = ResolveGrpcChannelServiceProvider(etcdOptions, useSsl);
            });

            if (configureChannelOptionsAction != null)
            {
                o.ChannelOptionsActions.Add(configureChannelOptionsAction);
            }
        };

        private static IServiceProvider ResolveGrpcChannelServiceProvider(EtcdOptions etcdOptions, bool useSsl)
        {
            string connectionString = etcdOptions.ConnectionString;

            if (connectionString.StartsWith(AlternateDnsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                connectionString = connectionString.Substring(AlternateDnsPrefix.Length);
                connectionString = DnsPrefix + connectionString;
            }

            if (connectionString.StartsWith(DnsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            IEnumerable<Uri> nodes = connectionString
               .Split(',')
               .Select(host =>
               {
                   if (host.Split(':').Length < 3)
                   {
                       host += $":{Convert.ToString(etcdOptions.Port, CultureInfo.InvariantCulture)}";
                   }

                   if (!(host.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) || host.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase)))
                   {
                       host = useSsl ? $"{SecurePrefix}{host}" : $"{InsecurePrefix}{host}";
                   }

                   return new Uri(host);
               });

            IEnumerable<BalancerAddress> balancerAddresses = nodes
                .Select(i => new BalancerAddress(i.Host, i.Port))
                .ToArray();

            StaticResolverFactory factory = new StaticResolverFactory(addr => balancerAddresses);

            return GetGrpcChannelServiceProvider(factory);
        }

        private static IServiceProvider GetGrpcChannelServiceProvider(StaticResolverFactory factory)
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ResolverFactory>(factory);

            return services.BuildServiceProvider();
        }

        private const string InsecurePrefix = "http://";
        private const string SecurePrefix = "https://";

        private const string StaticHostsPrefix = "static://";
        private const string DnsPrefix = "dns://";
        private const string AlternateDnsPrefix = "discovery-srv://";

        private static string ResolveAddress(EtcdOptions etcdOptions)
        {
            string connectionString = etcdOptions.ConnectionString;

            // Param check
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(EtcdOptions.ConnectionString));
            }

            // Param sanitization
            if (connectionString.StartsWith(AlternateDnsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                connectionString = connectionString.Substring(AlternateDnsPrefix.Length);
                connectionString = DnsPrefix + connectionString;
            }

            // Channel Configuration
            if (connectionString.StartsWith(DnsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return connectionString;
            }

            return $"{StaticHostsPrefix}{etcdOptions.ServerName}";
        }

        // https://learn.microsoft.com/en-us/aspnet/core/grpc/retries?view=aspnetcore-6.0#configure-a-grpc-retry-policy
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

        // https://github.com/grpc/proposal/blob/master/A6-client-retries.md#throttling-retry-attempts-and-hedged-rpcs
        private static readonly RetryThrottlingPolicy DefaultRetryThrottlingPolicy = new()
        {
            MaxTokens = 10,
            TokenRatio = 0.1
        };
    }
}
