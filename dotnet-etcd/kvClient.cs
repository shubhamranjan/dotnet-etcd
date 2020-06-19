using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Google.Protobuf;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        /// Get the etcd response for a specified RangeRequest
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The etcd response for the specified request</returns>
        public RangeResponse Get(RangeRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.kvClient
                .Range(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Get the etcd response for a specified key
        /// </summary>
        /// <param name="key">Key for which value need to be fetched</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The etcd response for the specified key</returns>
        public RangeResponse Get(string key, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return Get(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Get the etcd response for a specified key in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The etcd response for the specified request</returns>
        public async Task<RangeResponse> GetAsync(RangeRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.kvClient
                .RangeAsync(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Get the etcd response for a specified key in async
        /// </summary>
        /// <param name="key">Key for which value need to be fetched</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The etcd response for the specified key</returns>
        public async Task<RangeResponse> GetAsync(string key, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await GetAsync(new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Get the value for a specified key
        /// </summary>
        /// <param name="key">Key for which value need to be fetched</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The value for the specified key</returns>
        public string GetVal(string key, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            RangeResponse rangeResponse = Get(key, headers, deadline, cancellationToken);
            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Get the value for a specified key in async
        /// </summary>
        /// <param name="key">Key for which value need to be fetched</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The value for the specified key</returns>
        public async Task<string> GetValAsync(string key, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            RangeResponse rangeResponse = await GetAsync(key, headers, deadline, cancellationToken);
            return rangeResponse.Count != 0 ? rangeResponse.Kvs[0].Value.ToStringUtf8().Trim() : string.Empty;
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Prefix key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>RangeResponse containing range of key-values</returns>
        public RangeResponse GetRange(string prefixKey, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return Get(new RangeRequest
            {
                Key = GetStringByteForRangeRequests(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd),
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Prefix key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>RangeResponse containing range of key-values</returns>
        public async Task<RangeResponse> GetRangeAsync(string prefixKey, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return await GetAsync(new RangeRequest
            {
                Key = GetStringByteForRangeRequests(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Prefix key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Dictionary containing range of key-values</returns>
        public IDictionary<string, string> GetRangeVal(string prefixKey, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return RangeRespondToDictionary(Get(new RangeRequest
            {
                Key = GetStringByteForRangeRequests(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Gets the range of keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Prefix key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Dictionary containing range of key-values</returns>
        public async Task<IDictionary<string, string>> GetRangeValAsync(string prefixKey,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return RangeRespondToDictionary(await GetAsync(new RangeRequest
            {
                Key = GetStringByteForRangeRequests(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public PutResponse Put(PutRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.kvClient.Put(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Sets the key value in etcd
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public PutResponse Put(string key, string val, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return Put(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<PutResponse> PutAsync(PutRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.kvClient
                .PutAsync(request, headers, deadline, cancellationToken));
        }


        /// <summary>
        /// Sets the key value in etcd in async
        /// </summary>
        /// <param name="key">Key for which value need to be set</param>
        /// <param name="val">Value corresponding the key</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<PutResponse> PutAsync(string key, string val, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                Value = ByteString.CopyFromUtf8(val)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public DeleteRangeResponse Delete(DeleteRangeRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.kvClient
                .DeleteRange(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Delete the specified key in etcd
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public DeleteRangeResponse Delete(string key, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return Delete(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers, deadline, cancellationToken);
        }


        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<DeleteRangeResponse> DeleteAsync(DeleteRangeRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.kvClient
                .DeleteRangeAsync(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Delete the specified key in etcd in async
        /// </summary>
        /// <param name="key">Key which needs to be deleted</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<DeleteRangeResponse> DeleteAsync(string key, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(key)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Deletes all keys with the specified prefix
        /// </summary>
        /// <param name="prefixKey">Common prefix of all keys that need to be deleted</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public DeleteRangeResponse DeleteRange(string prefixKey, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return Delete(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Deletes all keys with the specified prefix in async
        /// </summary>
        /// <param name="prefixKey">Commin prefix of all keys that need to be deleted</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<DeleteRangeResponse> DeleteRangeAsync(string prefixKey, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            string rangeEnd = GetRangeEnd();
            return await DeleteAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(rangeEnd)
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public TxnResponse Transaction(TxnRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.kvClient.Txn(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        ///  Txn processes multiple requests in a single transaction in async.
        /// A txn request increments the revision of the key-value store
        /// and generates events with the same revision for every completed request.
        /// It is not allowed to modify the same key several times within one txn.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<TxnResponse> TransactionAsync(TxnRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.kvClient
                .TxnAsync(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Compact compacts the event history in the etcd key-value store. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public CompactionResponse Compact(CompactionRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.kvClient
                .Compact(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Compact compacts the event history in the etcd key-value store in async. The key-value
        /// store should be periodically compacted or the event history will continue to grow
        /// indefinitely.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<CompactionResponse> CompactAsync(CompactionRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.kvClient
                .CompactAsync(request, headers, deadline, cancellationToken));
        }
    }
}