using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;

using Grpc.Core;

using V3Lockpb;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// Lock acquires a distributed shared lock on a given named lock.
        /// On success, it will return a unique key that exists so long as the
        /// lock is held by the caller. This key can be used in conjunction with
        /// transactions to safely ensure updates to etcd only occur while holding
        /// lock ownership. The lock is held until Unlock is called on the key or the
        /// lease associate with the owner expires.
        /// </summary>
        /// <param name="name">is the identifier for the distributed shared lock to be acquired.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public LockResponse Lock(string name, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Lock(new LockRequest()
            {
                Name = ByteString.CopyFromUtf8(name),
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Lock acquires a distributed shared lock on a given named lock.
        /// On success, it will return a unique key that exists so long as the
        /// lock is held by the caller. This key can be used in conjunction with
        /// transactions to safely ensure updates to etcd only occur while holding
        /// lock ownership. The lock is held until Unlock is called on the key or the
        /// lease associate with the owner expires.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public LockResponse Lock(LockRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            LockResponse response = new LockResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().lockClient.Lock(request, headers, deadline, cancellationToken);
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
        /// LockAsync acquires a distributed shared lock on a given named lock.
        /// On success, it will return a unique key that exists so long as the
        /// lock is held by the caller. This key can be used in conjunction with
        /// transactions to safely ensure updates to etcd only occur while holding
        /// lock ownership. The lock is held until Unlock is called on the key or the
        /// lease associate with the owner expires.
        /// </summary>
        /// <param name="name">is the identifier for the distributed shared lock to be acquired.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public async Task<LockResponse> LockAsync(string name, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await LockAsync(new LockRequest()
            {
                Name = ByteString.CopyFromUtf8(name),
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// LockAsync acquires a distributed shared lock on a given named lock.
        /// On success, it will return a unique key that exists so long as the
        /// lock is held by the caller. This key can be used in conjunction with
        /// transactions to safely ensure updates to etcd only occur while holding
        /// lock ownership. The lock is held until Unlock is called on the key or the
        /// lease associate with the owner expires.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public async Task<LockResponse> LockAsync(LockRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            LockResponse response = new LockResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().lockClient
                        .LockAsync(request, headers, deadline, cancellationToken);
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
        /// Unlock takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="key">the lock ownership key granted by Lock.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public UnlockResponse Unlock(string key, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Unlock(new UnlockRequest()
            {
                Key = ByteString.CopyFromUtf8(key),
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Unlock takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public UnlockResponse Unlock(UnlockRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            UnlockResponse response = new UnlockResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().lockClient
                        .Unlock(request, headers, deadline, cancellationToken);
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
        /// UnlockAsync takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="key">the lock ownership key granted by Lock.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public async Task<UnlockResponse> UnlockAsync(string key, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UnlockAsync(new UnlockRequest()
            {
                Key = ByteString.CopyFromUtf8(key),
            }, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// UnlockAsync takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns></returns>
        public async Task<UnlockResponse> UnlockAsync(UnlockRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            UnlockResponse response = new UnlockResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().lockClient
                        .UnlockAsync(request, headers, deadline, cancellationToken);
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