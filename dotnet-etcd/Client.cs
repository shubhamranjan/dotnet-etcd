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
    public class EtcdClient
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
        /// The username for etcd server for basic auth
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// The password for etcd server for basic auth
        /// </summary>
        private readonly string _password;

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
        /// The host connection map for reusing etcd connections
        /// </summary>
        private static ConcurrentDictionary<string, Client> _hostConnectionMap = new ConcurrentDictionary<string, Client>();

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

            if (UsingOldConnection()) return;

            _channel = new Channel(_host, port, ChannelCredentials.Insecure);
            _kvClient = new KV.KVClient(_channel);
            _hostConnectionMap.TryAdd(_host, new Client
            {
                Channel = _channel,
                KvClient = _kvClient
            });
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

            if (UsingOldConnection()) return;

            var credentials = new SslCredentials(cert);
            _channel = new Channel(_host, port, credentials);
            _kvClient = new KV.KVClient(_channel);
            _hostConnectionMap.TryAdd(_host, new Client
            {
                Channel = _channel,
                KvClient = _kvClient
            });
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
            _username = username;
            _password = password;

            if (UsingOldConnection()) return;

            Authenticate();

            _channel = new Channel(_host, port, ChannelCredentials.Insecure);
            _kvClient = new KV.KVClient(_channel);
            _hostConnectionMap.TryAdd(_host, new Client
            {
                Channel = _channel,
                KvClient = _kvClient
            });
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
            _username = username;
            _password = password;

            if (UsingOldConnection()) return;

            Authenticate();

            var credentials = new SslCredentials(cert);
            _channel = new Channel(_host, port, credentials);
            _kvClient = new KV.KVClient(_channel);
            _hostConnectionMap.TryAdd(_host, new Client
            {
                Channel = _channel,
                KvClient = _kvClient
            });
        }


        #endregion

        #region Private Methods
        /// <summary>
        /// Checks if an connection exists for the host and the connection can be reused
        /// </summary>
        /// <returns><c>true</c>, if an old connection can be reused, <c>false</c> otherwise</returns>
        private bool UsingOldConnection()
        {
            try
            {
                if (!_hostConnectionMap.ContainsKey(_host)) return false;

                _channel = _hostConnectionMap[_host].Channel;
                _kvClient = _hostConnectionMap[_host].KvClient;


                if (HealthCheck()) return true;

                _hostConnectionMap.TryRemove(_host, out _);


            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Pings etcd server for connection check
        /// </summary>
        /// <returns><c>true</c>, if health check was successfull, <c>false</c> otherwise</returns>
        public bool HealthCheck()
        {
            try
            {
                Maintenance.MaintenanceClient maintenanceClient = new Maintenance.MaintenanceClient(_channel);
                var res = maintenanceClient.Status(new StatusRequest(), _headers);
                if (String.IsNullOrWhiteSpace(res.Version))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

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
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the value for a specified key
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public string Get(string key)
        {
            var rangeResponse = _kvClient.Range(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, _headers);
            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Get the value for a specified key in async
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<string> GetAsync(string key)
        {
            var rangeResponse = await _kvClient.RangeAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }
            , _headers);
            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public IDictionary<string, string> GetRange(string prefixKey)
        {
            var rangeEnd = GetRangeEnd(prefixKey);

            var response = _kvClient.Range(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, _headers);
            return RangeRespondToDictionary(response);
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public async Task<IDictionary<string, string>> GetRangeAsync(string prefixKey)
        {
            var rangeEnd = GetRangeEnd(prefixKey);

            var response = await _kvClient.RangeAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, _headers);
            return RangeRespondToDictionary(response);
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public void Put(string key, string val)
        {
            var putResponse = _kvClient.Put(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, _headers);
        }

        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public async void PutAsync(string key, string val)
        {
            await _kvClient.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, _headers);

        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public void Delete(string key)
        {
            _kvClient.DeleteRange(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, _headers);
        }

        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public async void DeleteAsync(string key)
        {
            var deleteRequest =
            await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, _headers);
        }

        /// <summary>
        /// Deletes all keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public void DeleteRange(string prefixKey)
        {
            var rangeEnd = GetRangeEnd(prefixKey);
            _kvClient.DeleteRange(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, _headers);
        }

        /// <summary>
        /// Deletes all keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public async void DeleteRangeAsync(string prefixKey)
        {
            var rangeEnd = GetRangeEnd(prefixKey);
            await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, _headers);
        }

        #endregion

    }
    #endregion

    #region private Classes
    /// <summary>
    /// Class containing all etcd communication channels and clients
    /// </summary>
    internal class Client
    {
        /// <summary>
        /// GRPC channel through which client will communicate
        /// </summary>
        public Channel Channel;

        /// <summary>
        /// Key-value client
        /// </summary>
        public KV.KVClient KvClient;
    }
    #endregion
}
