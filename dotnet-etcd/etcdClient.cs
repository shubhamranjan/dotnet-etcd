using Etcdserverpb;
using Grpc.Core;
using System;


namespace dotnet_etcd
{
    /// <summary>
    /// Etcd client is the entrypoint for this library.
    /// It contains all the functions required to perform operations on etcd.
    /// </summary>
    public partial class EtcdClient : IDisposable
    {
        #region Variables

        /// <summary>
        /// Grpc channel through which etcd client will communicate
        /// </summary>
        private Channel _channel;

        /// <summary>
        /// Hostname of etcd server
        /// </summary>
        private readonly string _host;

        /// <summary>
        /// Port on which etcd server is listening
        /// </summary>
        private readonly int _port;

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
        /// The token generated and recieved from etcd server post basic auth call
        /// </summary>
        private string _authToken;

        /// <summary>
        /// The headers for each etcd request
        /// </summary>
        private Metadata _headers;

        /// <summary>
        /// Client for authentication requests
        /// </summary>
        private Auth.AuthClient _authClient;

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

        private Watch.WatchClient _watchClient;

        private Lease.LeaseClient _leaseClient;

        /// <summary>
        /// Key-Value client through which operations like range/put on etcd can be performed
        /// </summary>
        private KV.KVClient _kvClient;

        /// <summary>
        /// Client throug which operations like Add, Remove, Update and List
        /// on etcd members could be performed.
        /// </summary>
        private Cluster.ClusterClient _clusterClient;

        /// <summary>
        /// Client through which maintenance operations can be performed.
        /// </summary>
        private Maintenance.MaintenanceClient _maintenanceClient;
        #endregion

        #region Initializers


        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">etcd server hostname</param>
        /// <param name="port">etcd server port</param>
        /// <param name="username">Username for basic auth on etcd</param>
        /// <param name="password">Password for basic auth on etcd</param>
        /// <param name="caCert">Certificate contents to connect to etcd server</param>
        /// <param name="clientCert"></param>
        /// <param name="clientKey"></param>
        public EtcdClient(string host, int port, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            _host = host;
            _port = port;
            _caCert = caCert;
            _clientCert = clientCert;
            _clientKey = clientKey;
            _username = username;
            _password = password;
            _publicRootCa = publicRootCa;

            _basicAuth = (!string.IsNullOrWhiteSpace(username) && !(string.IsNullOrWhiteSpace(password)));
            _ssl = !_publicRootCa && !string.IsNullOrWhiteSpace(caCert);
            _clientSSL = _ssl && (!string.IsNullOrWhiteSpace(clientCert) && !(string.IsNullOrWhiteSpace(clientKey)));


            Init();
        }

        private void Init()
        {
            try
            {
                if (_publicRootCa)
                {
                    _channel = new Channel(_host, _port, new SslCredentials());
                }
                else if (_clientSSL)
                {
                    _channel = new Channel(
                        _host,
                        _port,
                        new SslCredentials(
                            _caCert,
                            new KeyCertificatePair(_clientCert, _clientKey)
                        )
                    );
                }
                else if (_ssl)
                {
                    _channel = new Channel(_host, _port, new SslCredentials(_caCert));
                }
                else
                {
                    _channel = new Channel(_host, _port, ChannelCredentials.Insecure);
                }

                if (_basicAuth)
                {
                    Authenticate();
                }

                _kvClient = new KV.KVClient(_channel);
                _watchClient = new Watch.WatchClient(_channel);
                _leaseClient = new Lease.LeaseClient(_channel);
                _clusterClient = new Cluster.ClusterClient(_channel);
                _maintenanceClient = new Maintenance.MaintenanceClient(_channel);
            }
            catch
            {
                throw;
            }
            _disposed = false;
        }

        ~EtcdClient()
        {
            Dispose(true);
        }


        #endregion

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _channel.State != ChannelState.Shutdown)
                {
                    // TODO: dispose managed state (managed objects).
                    _channel.ShutdownAsync();
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
