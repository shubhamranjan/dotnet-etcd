using System;
using System.Collections.Generic;
using dotnet_etcd.multiplexer;


namespace dotnet_etcd
{
    /// <summary>
    /// Etcd client is the entrypoint for this library.
    /// It contains all the functions required to perform operations on etcd.
    /// </summary>
    public partial class EtcdClient : IDisposable
    {
        #region Variables
        private readonly Balancer _balancer;

        #endregion

        #region Initializers



        public EtcdClient(string connectionString, int port = 2379, string username = "", string password = "", string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("etcd connection string is empty.");
            }

            string[] hosts = connectionString.Split(',');

            List<Uri> nodes = new List<Uri>();

            for (int i = 0; i < hosts.Length; i++)
            {
                string host = hosts[i];
                if (host.Split(':').Length < 2)
                {
                    host += $":{Convert.ToString(port)}";
                }

                nodes.Add(new Uri(host));

            }

            _balancer = new Balancer(nodes, username, password, caCert, clientCert, clientKey, publicRootCa);
        }
        #endregion

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

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
