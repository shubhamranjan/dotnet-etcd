﻿using System;
using System.Collections.Generic;
using System.Linq;
using dotnet_etcd.multiplexer;
using DnsClient;
using DnsClient.Protocol;

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

        public EtcdClient(string connectionString, int port = 2379, string username = "", string password = "",
            string caCert = "", string clientCert = "", string clientKey = "", bool publicRootCa = false)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("etcd connection string is empty.");
            }

            string[] hosts;

            if (connectionString.ToLowerInvariant().StartsWith("discovery-srv://"))
            {
                // Expecting it to be discovery-srv://{domain}/{name}
                // Examples:
                // discovery-srv://my-domain.local/ would expect entries for either _etcd-client-ssl._tcp.my-domain.local or _etcd-client._tcp.my-domain.local
                // discovery-srv://my-domain.local/project1 would expect entries for either _etcd-client-ssl-project1._tcp.my-domain.local or _etcd-client-project1._tcp.my-domain.local
                Uri discoverySrv = new Uri(connectionString);
                var client = new LookupClient {UseCache = true};
                // SSL first ...
                var serviceName = "/".Equals(discoverySrv.AbsolutePath)
                    ? ""
                    : $"-{discoverySrv.AbsolutePath.Substring(1, discoverySrv.AbsolutePath.Length - 1)}";
                var result = client.Query($"_etcd-client-ssl{serviceName}._tcp.{discoverySrv.Host}", QueryType.SRV);
                var scheme = "https";
                if (result.HasError)
                {
                    scheme = "http";
                    // No SSL ...
                    result = client.Query($"_etcd-client{serviceName}._tcp.{discoverySrv.Host}", QueryType.SRV);
                    if (result.HasError)
                    {
                        throw new InvalidOperationException(result.ErrorMessage);
                    }
                }

                var results = result.Answers.OfType<SrvRecord>().OrderBy(a => a.Priority)
                    .ThenByDescending(a => a.Weight).ToList();
                hosts = new string[results.Count];
                for (int index = 0; index < results.Count; index++)
                {
                    var srvRecord = results[index];
                    var additionalRecord =
                        result.Additionals.FirstOrDefault(p => p.DomainName.Equals(srvRecord.Target));
                    var host = srvRecord.Target.Value;

                    if (additionalRecord is ARecord aRecord)
                    {
                        host = aRecord.Address.ToString();
                    }
                    else if (additionalRecord is CNameRecord cname)
                    {
                        host = cname.CanonicalName;
                    }

                    if (host.EndsWith("."))
                        host = host.Substring(0, host.Length - 1);
                    hosts[index] = $"{scheme}://{host}:{srvRecord.Port}";
                }
            }
            else
            {
                hosts = connectionString.Split(',');
            }

            List<Uri> nodes = new List<Uri>();

            for (int i = 0; i < hosts.Length; i++)
            {
                string host = hosts[i];
                if (host.Split(':').Length < 3)
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