﻿// Licensed to the .NET Foundation under one or more agreements.
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

namespace dotnet_etcd
{
    /// <summary>
    /// Etcd client is the entrypoint for this library.
    /// It contains all the functions required to perform operations on etcd.
    /// </summary>
    public partial class EtcdClient : IDisposable, IEtcdClient
    {
        #region Variables

        private const string InsecurePrefix = "http://";
        private const string SecurePrefix = "https://";

        private const string StaticHostsPrefix = "static://";
        private const string DnsPrefix = "dns://";
        private const string AlternateDnsPrefix = "discovery-srv://";

        private const string DefaultServerName = "my-etcd-server";

        private readonly Connection _connection;

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
            MaxTokens = 10,
            TokenRatio = 0.1
        };
        #endregion

        #region Initializers

        public EtcdClient(string connectionString, int port = 2379, string serverName = DefaultServerName, Action<GrpcChannelOptions> configureChannelOptions = null, Interceptor[] interceptors = null)
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

            // Connection Configuration
            var options = new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig
                {
                    MethodConfigs = { _defaultGrpcMethodConfig },
                    RetryThrottling = _defaultRetryThrottlingPolicy,
                    LoadBalancingConfigs = { new RoundRobinConfig() },
                }
            };

            configureChannelOptions?.Invoke(options);

            // Channel Configuration
            GrpcChannel channel = null;
            if (connectionString.StartsWith(DnsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                channel = GrpcChannel.ForAddress(connectionString, options);
            }
            else
            {
                string[] hosts = Array.Empty<string>();
                hosts = connectionString.Split(',');
                List<Uri> nodes = new();
                for (int i = 0; i < hosts.Length; i++)
                {
                    string host = hosts[i];
                    if (host.Split(':').Length < 3)
                    {
                        host += $":{Convert.ToString(port, CultureInfo.InvariantCulture)}";
                    }

                    if (!(host.StartsWith(InsecurePrefix, StringComparison.InvariantCultureIgnoreCase) || host.StartsWith(SecurePrefix, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        host = options.Credentials == ChannelCredentials.Insecure ? $"{InsecurePrefix}{host}" : $"{SecurePrefix}{host}";
                    }

                    nodes.Add(new Uri(host));
                }

                var factory = new StaticResolverFactory(addr => nodes.Select(i => new BalancerAddress(i.Host, i.Port)).ToArray());
                var services = new ServiceCollection();
                services.AddSingleton<ResolverFactory>(factory);
                options.ServiceProvider = services.BuildServiceProvider();

                channel = GrpcChannel.ForAddress($"{StaticHostsPrefix}{serverName}", options);
            }

            CallInvoker callInvoker = interceptors != null && interceptors.Length > 0 ? channel.Intercept(interceptors) : channel.CreateCallInvoker();


            // Setup Connection
            _connection = new Connection
            {
                _kvClient = new KV.KVClient(callInvoker),
                _watchClient = new Watch.WatchClient(callInvoker),
                _leaseClient = new Lease.LeaseClient(callInvoker),
                _lockClient = new V3Lockpb.Lock.LockClient(callInvoker),
                _clusterClient = new Cluster.ClusterClient(callInvoker),
                _maintenanceClient = new Maintenance.MaintenanceClient(callInvoker),
                _authClient = new Auth.AuthClient(callInvoker),
                _electionClient = new V3Electionpb.Election.ElectionClient(callInvoker)
            };
        }

        #endregion

        #region IDisposable Support

        private bool _disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
