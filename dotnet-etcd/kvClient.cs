using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {

        /// <summary>
        /// Get the etcd response for a specified RangeRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The etcd response for the specified request</returns>
        public RangeResponse Get(RangeRequest request, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    rangeResponse = _balancer.GetConnection().kvClient.Range(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }

            return rangeResponse;
        }

        /// <summary>
        /// Get the etcd response for a specified key
        /// </summary>
        /// <returns>The etcd response for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public RangeResponse Get(string key, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            rangeResponse = Get(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers);


            return rangeResponse;
        }

        /// <summary>
        /// Get the etcd response for a specified key in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The etcd response for the specified request</returns>
        public async Task<RangeResponse> GetAsync(RangeRequest request, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    rangeResponse = await _balancer.GetConnection().kvClient.RangeAsync(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }

            return rangeResponse;
        }

        /// <summary>
        /// Get the etcd response for a specified key in async
        /// </summary>
        /// <returns>The etcd response for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<RangeResponse> GetAsync(string key, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            rangeResponse = await GetAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers);


            return rangeResponse;
        }

        /// <summary>
        /// Get the value for a specified key
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public string GetVal(string key, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            rangeResponse = Get(key, headers);


            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Get the value for a specified key in async
        /// </summary>
        /// <returns>The value for the specified key</returns>
        /// <param name="key">Key for which value need to be fetched</param>
        public async Task<string> GetValAsync(string key, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            rangeResponse = await GetAsync(key, headers);


            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <returns>RangeResponse containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public RangeResponse GetRange(string prefixKey, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            string rangeEnd = GetRangeEnd(prefixKey);

            rangeResponse = Get(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);



            return rangeResponse;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <returns>RangeResponse containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public async Task<RangeResponse> GetRangeAsync(string prefixKey, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            string rangeEnd = GetRangeEnd(prefixKey);

            rangeResponse = await GetAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);

            return rangeResponse;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public IDictionary<string, string> GetRangeVal(string prefixKey, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            string rangeEnd = GetRangeEnd(prefixKey);

            rangeResponse = Get(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);



            return RangeRespondToDictionary(rangeResponse);
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <returns>Dictionary containing range of key-values</returns>
        /// <param name="prefixKey">Prefix key</param>
        public async Task<IDictionary<string, string>> GetRangeValAsync(string prefixKey, Metadata headers = null)
        {
            RangeResponse rangeResponse = new RangeResponse();

            string rangeEnd = GetRangeEnd(prefixKey);

            rangeResponse = await GetAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);


            return RangeRespondToDictionary(rangeResponse);
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PutResponse Put(PutRequest request, Metadata headers = null)
        {
            PutResponse response = new PutResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().kvClient.Put(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public PutResponse Put(string key, string val, Metadata headers = null)
        {

            return Put(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, headers);

        }

        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PutResponse> PutAsync(PutRequest request, Metadata headers = null)
        {
            PutResponse response = new PutResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().kvClient.PutAsync(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }


        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <returns></returns>
        public async Task<PutResponse> PutAsync(string key, string val, Metadata headers = null)
        {

            return await PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, headers);

        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DeleteRangeResponse Delete(DeleteRangeRequest request, Metadata headers = null)
        {
            DeleteRangeResponse response = new DeleteRangeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().kvClient.DeleteRange(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public DeleteRangeResponse Delete(string key, Metadata headers = null)
        {

            return Delete(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers);

        }


        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<DeleteRangeResponse> DeleteAsync(DeleteRangeRequest request, Metadata headers = null)
        {
            DeleteRangeResponse response = new DeleteRangeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().kvClient.DeleteRangeAsync(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        public async Task<DeleteRangeResponse> DeleteAsync(string key, Metadata headers = null)
        {

            return await DeleteAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers);

        }

        /// <summary>
        /// Deletes all keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public DeleteRangeResponse DeleteRange(string prefixKey, Metadata headers = null)
        {

            string rangeEnd = GetRangeEnd(prefixKey);
            return Delete(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);

        }

        /// <summary>
        /// Deletes all keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        public async Task<DeleteRangeResponse> DeleteRangeAsync(string prefixKey, Metadata headers = null)
        {

            string rangeEnd = GetRangeEnd(prefixKey);
            return await DeleteAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers);

        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TxnResponse Transaction(TxnRequest request, Metadata headers = null)
        {
            TxnResponse response = new TxnResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().kvClient.Txn(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;

        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction in async.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TxnResponse> TransactionAsync(TxnRequest request, Metadata headers = null)
        {
            TxnResponse response = new TxnResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().kvClient.TxnAsync(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Compact compacts the event history in the etcd key-value store. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CompactionResponse Compact(CompactionRequest request, Metadata headers = null)
        {
            CompactionResponse response = new CompactionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().kvClient.Compact(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Compact compacts the event history in the etcd key-value store in async. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CompactionResponse> CompactAsync(CompactionRequest request, Metadata headers = null)
        {
            CompactionResponse response = new CompactionResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().kvClient.CompactAsync(request, headers);
                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw ex;
                    }
                }
            }
            return response;
        }

    }

}
