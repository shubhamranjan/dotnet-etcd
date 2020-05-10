using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {
        private readonly HashSet<Connection> _HealthyCluster;

        private readonly HashSet<Connection> _UnHealthyCluster;

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
        private static Random random;

        internal Balancer(List<Uri> nodes, string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _numNodes = nodes.Count;
            _caCert = caCert;
            _clientCert = clientCert;
            _clientKey = clientKey;
            _publicRootCa = publicRootCa;
            _lastNodeIndex = random.Next(-1, _numNodes);

            _ssl = !_publicRootCa && !string.IsNullOrWhiteSpace(_caCert);
            _clientSSL = _ssl && (!string.IsNullOrWhiteSpace(_clientCert) && !(string.IsNullOrWhiteSpace(_clientKey)));

            _HealthyCluster = new HashSet<Connection>();
            _UnHealthyCluster = new HashSet<Connection>();

            random = new Random();

            foreach (Uri node in nodes)
            {
                Channel channel;
                if (_publicRootCa)
                {
                    channel = new Channel(node.Host, node.Port, new SslCredentials());
                }
                else if (_clientSSL)
                {
                    channel = new Channel(
                        node.Host,
                        node.Port,
                        new SslCredentials(
                            _caCert,
                            new KeyCertificatePair(_clientCert, _clientKey)
                        )
                    );
                }
                else if (_ssl)
                {
                    channel = new Channel(node.Host, node.Port, new SslCredentials(_caCert));
                }
                else
                {
                    channel = new Channel(node.Host, node.Port, ChannelCredentials.Insecure);
                }

                Connection connection = new Connection
                {
                    kvClient = new KV.KVClient(channel),
                    watchClient = new Watch.WatchClient(channel),
                    leaseClient = new Lease.LeaseClient(channel),
                    lockClient = new V3Lockpb.Lock.LockClient(channel),
                    clusterClient = new Cluster.ClusterClient(channel),
                    maintenanceClient = new Maintenance.MaintenanceClient(channel),
                    authClient = new Auth.AuthClient(channel)
                };

                _HealthyCluster.Add(connection);
            }

        }

        internal Connection GetConnection()
        {
            return _HealthyCluster.ElementAt(GetNextNodeIndex());
        }

        internal Connection GetConnection(int index)
        {
            return _HealthyCluster.ElementAt(index);
        }

        internal void MarkUnHealthy(Connection connection)
        {
            _HealthyCluster.Remove(connection);
            _UnHealthyCluster.Add(connection);
        }

        internal void MarkHealthy(Connection connection)
        {
            _UnHealthyCluster.Remove(connection);
            _HealthyCluster.Add(connection);
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
