using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_etcd
{

    #region Public Classes
    public partial class EtcdClient : IDisposable
    {
        #region Variables

        /// <summary>
        /// Grpc channel through which etcd client will communicate
        /// </summary>
        private Channel _channel;

        /// <summary>
        /// Key-Value client through which operations like range/put on etcd can be performed
        /// </summary>
        private KV.KVClient _kvClient;

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

            _basicAuth = (!String.IsNullOrWhiteSpace(username) && !(String.IsNullOrWhiteSpace(password)));
            _ssl = !_publicRootCa && !String.IsNullOrWhiteSpace(caCert);
            _clientSSL = _ssl && (!String.IsNullOrWhiteSpace(clientCert) && !(String.IsNullOrWhiteSpace(clientKey)));


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
                    Authenticate();


                _kvClient = new KV.KVClient(_channel);
                _watchClient = new Watch.WatchClient(_channel);
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

        #region Private Methods

        /// <summary>
        /// Converts RangeResponse to Dictionary
        /// </summary>
        /// <returns>IDictionary corresponding the RangeResponse</returns>
        /// <param name="resp">RangeResponse received from etcd server</param>
        private static IDictionary<string, string> RangeRespondToDictionary(RangeResponse resp)
        {
            var resDictionary = new Dictionary<string, string>();
            foreach (var kv in resp.Kvs)
            {
                resDictionary.Add(kv.Key.ToStringUtf8(), kv.Value.ToStringUtf8());
            }
            return resDictionary;
        }

        /// <summary>
        /// Gets the range end for prefix
        /// </summary>
        /// <returns>The range end for prefix</returns>
        /// <param name="prefixKey">Prefix key</param>
        private string GetRangeEnd(string prefixKey)
        {
            StringBuilder rangeEnd = new StringBuilder(prefixKey);
            rangeEnd[rangeEnd.Length - 1] = ++rangeEnd[rangeEnd.Length - 1];
            return rangeEnd.ToString();
        }

        /// <summary>
        /// Used to authenticate etcd server through basic auth
        /// </summary>
        private void Authenticate()
        {

            _authClient = new Auth.AuthClient(_channel);
            var authRes = _authClient.Authenticate(new AuthenticateRequest
            {
                Name = _username,
                Password = _password
            });

            _authToken = authRes.Token;
            _headers = new Metadata
            {
                { "Authorization", _authToken }
            };
        }

        private void ResetConnection()
        {

            Dispose(true);
            Init();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get the etcd response for a specified key
        /// </summary>
        /// <returns>The etcd response for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public RangeResponse Get(string key)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                rangeResponse = _kvClient.Range(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return rangeResponse;
        }

        /// <summary>
        /// Get the etcd response for a specified key in async
        /// </summary>
        /// <returns>The etcd response for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<RangeResponse> GetAsync(string key)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                rangeResponse = await _kvClient.RangeAsync(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
                , _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return rangeResponse;
        }

        /// <summary>
        /// Get the value for a specified key
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public string GetVal(string key)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                rangeResponse = Get(key);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Get the value for a specified key in async
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<string> GetValAsync(string key)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                rangeResponse = await GetAsync(key);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public IDictionary<string, string> GetRange(string prefixKey)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);

                rangeResponse = _kvClient.Range(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);

            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return RangeRespondToDictionary(rangeResponse);
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public async Task<IDictionary<string, string>> GetRangeAsync(string prefixKey)
        {
            RangeResponse rangeResponse = new RangeResponse();
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);

                rangeResponse = await _kvClient.RangeAsync(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }

            return RangeRespondToDictionary(rangeResponse);
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public PutResponse Put(string key, string val)
        {
            try
            {
                return _kvClient.Put(new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(val)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public async Task<PutResponse> PutAsync(string key, string val)
        {
            try
            {
                return await _kvClient.PutAsync(new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(val)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public DeleteRangeResponse Delete(string key)
        {
            try
            {
                return _kvClient.DeleteRange(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public async Task<DeleteRangeResponse> DeleteAsync(string key)
        {
            try
            {
                return await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes all keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public DeleteRangeResponse DeleteRange(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);
                return _kvClient.DeleteRange(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes all keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public async Task<DeleteRangeResponse> DeleteRangeAsync(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);
                return await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TxnResponse Transaction(TxnRequest request)
        {
            try
            {
                return _kvClient.Txn(request, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction in async.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TxnResponse> TransactionAsync(TxnRequest request)
        {
            try
            {
                return await _kvClient.TxnAsync(request, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Compact compacts the event history in the etcd key-value store. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CompactionResponse Compact(CompactionRequest request)
        {
            try
            {
                return _kvClient.Compact(request, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Compact compacts the event history in the etcd key-value store in async. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CompactionResponse> CompactAsync(CompactionRequest request)
        {
            try
            {
                return await _kvClient.CompactAsync(request, _headers);
            }
            catch (Grpc.Core.RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
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
    #endregion

}
