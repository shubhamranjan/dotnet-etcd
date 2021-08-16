// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

using Etcdserverpb;

using Grpc.Core;
using Grpc.Net.Client;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {
        private readonly HashSet<Connection> _healthyNode;

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


        internal Balancer(List<Uri> nodes, HttpClientHandler handler = null, bool ssl = false)
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
                        HttpHandler = handler
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
                        HttpHandler = handler
                    };

                    channel = GrpcChannel.ForAddress(node, options);
                }

                Connection connection = new Connection
                {
                    _kvClient = new KV.KVClient(channel),
                    _watchClient = new Watch.WatchClient(channel),
                    _leaseClient = new Lease.LeaseClient(channel),
                    _lockClient = new V3Lockpb.Lock.LockClient(channel),
                    _clusterClient = new Cluster.ClusterClient(channel),
                    _maintenanceClient = new Maintenance.MaintenanceClient(channel),
                    _authClient = new Auth.AuthClient(channel)
                };

                _healthyNode.Add(connection);
            }
        }

        internal Connection GetConnection()
        {
            return _healthyNode.ElementAt(GetNextNodeIndex());
        }

        internal Connection GetConnection(int index)
        {
            return _healthyNode.ElementAt(index);
        }

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
