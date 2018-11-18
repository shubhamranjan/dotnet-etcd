using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
                string rangeEnd = GetRangeEnd(prefixKey);

                rangeResponse = _kvClient.Range(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);

            }
            catch (RpcException)
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
                string rangeEnd = GetRangeEnd(prefixKey);

                rangeResponse = await _kvClient.RangeAsync(new RangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
                string rangeEnd = GetRangeEnd(prefixKey);
                return _kvClient.DeleteRange(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (RpcException)
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
                string rangeEnd = GetRangeEnd(prefixKey);
                return await _kvClient.DeleteRangeAsync(new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(prefixKey),
                    RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
                }, _headers);
            }
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
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
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
            catch
            {
                throw;
            }
        }

    }

}
