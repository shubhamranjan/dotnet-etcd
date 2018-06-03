using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_etcd
{

    #region Public Classes
    public class EtcdClient : IDisposable
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
        /// Certificate contents to be used to connect to etcd.
        /// </summary>
        private readonly string _cert;

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

        private AuthType _authType;
        enum AuthType { None, SSL, Basic, BasicSSL };
        #endregion

        #region Initializers


        /// <summary>
        /// Initializes etcd client with no auth over an insecure channel
        /// </summary>
        /// <param name="host">etcd server hostname</param>
        /// <param name="port">etcd server port</param>
        public EtcdClient(string host, int port)
        {
            _host = host;
            _port = port;
            _authType = AuthType.None;

            Init();
        }

        /// <summary>
        /// Initializes etcd client with no auth over an secure channel
        /// </summary>
        /// <param name="host">etcd server hostname</param>
        /// <param name="port">etcd server port</param>
        /// <param name="cert">Certificate contents to connect to etcd server</param>
        public EtcdClient(string host, int port, string cert)
        {
            _host = host;
            _port = port;
            _cert = cert;
            _authType = AuthType.SSL;

            Init();
        }

        /// <summary>
        /// Initializes etcd client with basic auth over an insecure channel
        /// </summary>
        /// <param name="host">etcd server hostname</param>
        /// <param name="port">etcd server port</param>
        /// <param name="username">Username for basic auth on etcd</param>
        /// <param name="password">Password for basic auth on etcd</param>
        public EtcdClient(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _authType = AuthType.Basic;
        }

        /// <summary>
        /// Initializes etcd client with basic auth over a isecure channel
        /// </summary>
        /// <param name="host">etcd server hostname</param>
        /// <param name="port">etcd server port</param>
        /// <param name="username">Username for basic auth on etcd</param>
        /// <param name="password">Password for basic auth on etcd</param>
        /// <param name="cert">Certificate contents to connect to etcd server</param>
        public EtcdClient(string host, int port, string username, string password, string cert)
        {
            _host = host;
            _port = port;
            _cert = cert;
            _username = username;
            _password = password;
            _authType = AuthType.BasicSSL;
        }

        private void Init()
        {
            try
            {
                switch (_authType)
                {
                    case AuthType.None:

                        _channel = new Channel(_host, _port, ChannelCredentials.Insecure);
                        _kvClient = new KV.KVClient(_channel);

                        break;
                    case AuthType.SSL:

                        _channel = new Channel(_host, _port, new SslCredentials(_cert));
                        _kvClient = new KV.KVClient(_channel);

                        break;
                    case AuthType.Basic:

                        Authenticate();

                        _channel = new Channel(_host, _port, ChannelCredentials.Insecure);
                        _kvClient = new KV.KVClient(_channel);

                        break;
                    case AuthType.BasicSSL:
                    default:

                        Authenticate();

                        _channel = new Channel(_host, _port, new SslCredentials(_cert));
                        _kvClient = new KV.KVClient(_channel);

                        break;
                }
            }
            catch
            {
                // ignore
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
            var authChannel = new Channel(_host, ChannelCredentials.Insecure);
            _authClient = new Auth.AuthClient(authChannel);
            var authRes = _authClient.Authenticate(new AuthenticateRequest
            {
                Name = _username,
                Password = _password
            });
            var shutdownAsync = authChannel.ShutdownAsync();
            shutdownAsync.Dispose();

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
        /// Get the value for a specified key
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public string Get(string key)
        {

            try
            {
                var rangeResponse = _kvClient.Range(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);

                return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
            }
            catch
            {
                ResetConnection();
            }

            return String.Empty;
        }

        /// <summary>
        /// Get the value for a specified key in async
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<string> GetAsync(string key)
        {
            try
            {
                var rangeResponse = await _kvClient.RangeAsync(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
                , _headers);

                return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
            }
            catch
            {
                ResetConnection();
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public IDictionary<string, string> GetRange(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);

                var response = _kvClient.Range(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);

                return RangeRespondToDictionary(response);
            }
            catch
            {
                ResetConnection();
            }

            return RangeRespondToDictionary(new RangeResponse());
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public async Task<IDictionary<string, string>> GetRangeAsync(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);

                var response = await _kvClient.RangeAsync(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
                return RangeRespondToDictionary(response);
            }
            catch
            {
                ResetConnection();
            }

            return RangeRespondToDictionary(new RangeResponse());
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public void Put(string key, string val)
        {
            try
            {
                var putResponse = _kvClient.Put(new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(val)
                }, _headers);
            }
            catch
            {
                ResetConnection();
            }
        }

        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public async void PutAsync(string key, string val)
        {
            try
            {
                await _kvClient.PutAsync(new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(val)
                }, _headers);
            }
            catch
            {
                ResetConnection();
            }
        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public void Delete(string key)
        {
            try
            {
                _kvClient.DeleteRange(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);
            }
            catch
            {
                ResetConnection();
            }
        }

        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public async void DeleteAsync(string key)
        {
            try
            {
                await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(key)
                }, _headers);
            }
            catch
            {
                ResetConnection();
            }
        }

        /// <summary>
        /// Deletes all keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public void DeleteRange(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);
                _kvClient.DeleteRange(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch
            {
                ResetConnection();
            }
        }

        /// <summary>
        /// Deletes all keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public async void DeleteRangeAsync(string prefixKey)
        {
            try
            {
                var rangeEnd = GetRangeEnd(prefixKey);
                await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch
            {
                ResetConnection();
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
