using System;
using System.Collections.Generic;
using System.Linq;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.multiplexer
{

    internal class Balancer
    {
        private SortedSet<Connection> _HealthyCluster;

        private SortedSet<Connection> _UnHealthyCluster;

        private Random _random;

        /// <summary>
        /// The username for etcd server for basic auth
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// The password for etcd server for basic auth
        /// </summary>
        private readonly string _password;

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
        /// Depicts whether basic auth is enabled or not
        /// </summary>
        private readonly bool _basicAuth;

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



        internal Balancer(List<Uri> nodes, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _caCert = caCert;
            _clientCert = clientCert;
            _clientKey = clientKey;
            _username = username;
            _password = password;
            _publicRootCa = publicRootCa;

            _basicAuth = (!string.IsNullOrWhiteSpace(_username) && !(string.IsNullOrWhiteSpace(_password)));
            _ssl = !_publicRootCa && !string.IsNullOrWhiteSpace(_caCert);
            _clientSSL = _ssl && (!string.IsNullOrWhiteSpace(_clientCert) && !(string.IsNullOrWhiteSpace(_clientKey)));

            _HealthyCluster = new SortedSet<Connection>();
            _UnHealthyCluster = new SortedSet<Connection>();
            _random = new Random(0);

            foreach (Uri node in nodes)
            {
                Channel channel = null;
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
            return _HealthyCluster.ElementAt(_random.Next(_HealthyCluster.Count));
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



    }
}
