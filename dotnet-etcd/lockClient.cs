using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
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
        /// <returns></returns>
        public LockResponse Lock(string name)
        {
            return Lock(new LockRequest()
            {
                Name = ByteString.CopyFromUtf8(name),
            });
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
        /// <returns></returns>
        public LockResponse Lock(LockRequest request)
        {
            try
            {
                return _lockClient.Lock(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
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
        /// <returns></returns>
        public async Task<LockResponse> LockAsync(string name)
        {
            return await LockAsync(new LockRequest()
            {
                Name = ByteString.CopyFromUtf8(name),
            });
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
        /// <returns></returns>
        public async Task<LockResponse> LockAsync(LockRequest request)
        {
            try
            {
                return await _lockClient.LockAsync(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
        }

        /// <summary>
        /// Unlock takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="key">the lock ownership key granted by Lock.</param>
        /// <returns></returns>
        public UnlockResponse Unlock(string key)
        {
            return Unlock(new UnlockRequest()
            {
                Key = ByteString.CopyFromUtf8(key),
            });
        }
        
        /// <summary>
        /// Unlock takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <returns></returns>
        public UnlockResponse Unlock(UnlockRequest request)
        {
            try
            {
                return _lockClient.Unlock(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
        }

        /// <summary>
        /// UnlockAsync takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="key">the lock ownership key granted by Lock.</param>
        /// <returns></returns>
        public async Task<UnlockResponse> UnlockAsync(string key)
        {
            return await UnlockAsync(new UnlockRequest()
            {
                Key = ByteString.CopyFromUtf8(key),
            });
        }

        /// <summary>
        /// UnlockAsync takes a key returned by Lock and releases the hold on lock. The
        /// next Lock caller waiting for the lock will then be woken up and given
        /// ownership of the lock.
        /// </summary>
        /// <param name="request">The request to send to the server</param>
        /// <returns></returns>
        public async Task<UnlockResponse> UnlockAsync(UnlockRequest request)
        {
            try
            {
                return await _lockClient.UnlockAsync(request, _headers);
            }
            catch (RpcException)
            {
                ResetConnection();
                throw;
            }
        }
    }
}