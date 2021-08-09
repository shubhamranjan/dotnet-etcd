// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Etcdserverpb;

using Grpc.Core;
using Grpc.Net.Client;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {
        private readonly HashSet<Connection> _healthyNode;

        private readonly HashSet<Connection> _unHealthyNode;

        /// <summary>
        /// CA Certificate contents to be used to connect to etcd.
        /// </summary>
        private readonly string _caCert;

        /// <summary>
        /// Client Certificate contents to be used to connect to etcd.
        /// </summary>
        private readonly string _clientCert;

        /// <summary>
        /// Client key contents to be used to connect to etcd.
        /// </summary>
        private readonly string _clientKey;

        /// <summary>
        /// Depicts whether ssl is enabled or not
        /// </summary>
        private readonly bool _ssl;

        /// <summary>
        /// Depicts whether ssl auth is enabled or not
        /// </summary>
        private readonly bool _clientSSL;

        /// <summary>
        /// Depicts whether to connect using publicly trusted roots.
        /// </summary>
        private readonly bool _publicRootCa;

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


        private const string InsecurePrefix = "http://";
        private const string SecurePrefix = "https://";

        internal Balancer(List<Uri> nodes, string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _numNodes = nodes.Count;
            _caCert = caCert;
            _clientCert = clientCert;
            _clientKey = clientKey;
            _publicRootCa = publicRootCa;
            _lastNodeIndex = s_random.Next(-1, _numNodes);

            _ssl = !_publicRootCa && !string.IsNullOrWhiteSpace(_caCert);
            _clientSSL = _ssl && (!string.IsNullOrWhiteSpace(_clientCert) && !(string.IsNullOrWhiteSpace(_clientKey)));

            _healthyNode = new HashSet<Connection>();
            _unHealthyNode = new HashSet<Connection>();


            foreach (Uri node in nodes)
            {
                GrpcChannel channel;

                Uri uri = GetCleanUri(node.Host, node.Port, _publicRootCa || _clientSSL || _ssl);

                if (_publicRootCa)
                {
                    channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
                    {
                        Credentials = new SslCredentials()
                    });
                }
                else if (_clientSSL)
                {
                    channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
                    {
                        Credentials = new SslCredentials(
                            _caCert,
                            new KeyCertificatePair(_clientCert, _clientKey)
                        )
                    });
                }
                else if (_ssl)
                {
                    channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
                    {
                        Credentials = new SslCredentials(_caCert)
                    });
                }
                else
                {
                    channel = GrpcChannel.ForAddress(uri, new GrpcChannelOptions
                    {
                        Credentials = ChannelCredentials.Insecure
                    });
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

        private static Uri GetCleanUri(string host, int port, bool needsSsl)
        {
            if (!(host.StartsWith(InsecurePrefix) || host.StartsWith(SecurePrefix)))
            {
                if (needsSsl)
                {
                    host = $"{SecurePrefix}{host}";
                }
                else
                {
                    host = $"{InsecurePrefix}{host}";
                }
            }
            host = $"{host}:{port}";

            return new Uri(host);
        }

        internal Connection GetConnection()
        {
            return _healthyNode.ElementAt(GetNextNodeIndex());
        }

        internal Connection GetConnection(int index)
        {
            return _healthyNode.ElementAt(index);
        }

        internal void MarkUnHealthy(Connection connection)
        {
            _healthyNode.Remove(connection);
            _unHealthyNode.Add(connection);
        }

        internal void MarkHealthy(Connection connection)
        {
            _unHealthyNode.Remove(connection);
            _healthyNode.Add(connection);
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
