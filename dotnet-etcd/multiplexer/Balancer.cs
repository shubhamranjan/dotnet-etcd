// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

using Etcdserverpb;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {
        internal readonly HashSet<Connection> _healthyNode;

        /// <summary>
        /// No of etcd nodes
        /// </summary>
        internal readonly int _numNodes;

        /// <summary>
        /// Last used node index
        /// </summary>
        private int _lastNodeIndex;

        /// <summary>
        /// Random object for randomizing selected node
        /// </summary>
        private static readonly Random s_random = new Random();


        internal Balancer(List<Uri> nodes, HttpClientHandler handler = null, bool ssl = false,
            bool useLegacyRpcExceptionForCancellation = false, params Interceptor[] interceptors)
        {
            _numNodes = nodes.Count;
            _lastNodeIndex = s_random.Next(-1, _numNodes);

            _healthyNode = new HashSet<Connection>();

            foreach (Uri node in nodes)
            {
                GrpcChannel channel;

                if (ssl)
                {
                    channel = GrpcChannel.ForAddress(node, new GrpcChannelOptions
                    {
                        Credentials = new SslCredentials(),
                        HttpHandler = handler,
                        ThrowOperationCanceledOnCancellation = !useLegacyRpcExceptionForCancellation
                    });
                }
                else
                {
#if NETCOREAPP3_1 || NETCOREAPP3_0
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
                    var options = new GrpcChannelOptions
                    {
                        Credentials = ChannelCredentials.Insecure,
                        HttpHandler = handler,
                        ThrowOperationCanceledOnCancellation = !useLegacyRpcExceptionForCancellation
                    };

                    channel = GrpcChannel.ForAddress(node, options);
                }

                CallInvoker callInvoker;

                if (interceptors !=null && interceptors.Length > 0)
                {
                    callInvoker = channel.Intercept(interceptors);
                }
                else
                {
                    callInvoker = channel.CreateCallInvoker();
                }


                Connection connection = new Connection
                {
                    _kvClient = new KV.KVClient(callInvoker),
                    _watchClient = new Watch.WatchClient(callInvoker),
                    _leaseClient = new Lease.LeaseClient(callInvoker),
                    _lockClient = new V3Lockpb.Lock.LockClient(callInvoker),
                    _clusterClient = new Cluster.ClusterClient(callInvoker),
                    _maintenanceClient = new Maintenance.MaintenanceClient(callInvoker),
                    _authClient = new Auth.AuthClient(callInvoker)
                };

                _healthyNode.Add(connection);
            }
        }

        internal Connection GetConnection() => _healthyNode.ElementAt(GetNextNodeIndex());

        internal Connection GetConnection(int index) => _healthyNode.ElementAt(index);

        internal int GetNextNodeIndex()
        {
            int initial, computed;
            do
            {
                initial = _lastNodeIndex;
                computed = initial + 1;
                computed = computed >= _numNodes ? computed = 0 : computed;
            }
            while (Interlocked.CompareExchange(ref _lastNodeIndex, computed, initial) != initial);
            return computed;
        }


    }
}
