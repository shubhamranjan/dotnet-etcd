using System;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// MemberAdd adds a member into the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberAddResponse MemberAdd(MemberAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberAddResponse response = new MemberAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().clusterClient
                        .MemberAdd(request, headers, deadline, cancellationToken);
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
        /// MemberAddAsync adds a member into the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberAddResponse> MemberAddAsync(MemberAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberAddResponse response = new MemberAddResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().clusterClient
                        .MemberAddAsync(request, headers, deadline, cancellationToken);
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
        /// MemberRemove removes an existing member from the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberRemoveResponse MemberRemove(MemberRemoveRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberRemoveResponse response = new MemberRemoveResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().clusterClient
                        .MemberRemove(request, headers, deadline, cancellationToken);
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
        /// MemberRemoveAsync removes an existing member from the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberRemoveResponse> MemberRemoveAsync(MemberRemoveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberRemoveResponse response = new MemberRemoveResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().clusterClient
                        .MemberRemoveAsync(request, headers, deadline, cancellationToken);
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
        /// MemberUpdate updates the member configuration
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberUpdateResponse MemberUpdate(MemberUpdateRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberUpdateResponse response = new MemberUpdateResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().clusterClient
                        .MemberUpdate(request, headers, deadline, cancellationToken);
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
        /// MemberUpdateAsync updates the member configuration in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberUpdateResponse> MemberUpdateAsync(MemberUpdateRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberUpdateResponse response = new MemberUpdateResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().clusterClient
                        .MemberUpdateAsync(request, headers, deadline, cancellationToken);
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
        /// MemberList lists all the members in the cluster
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public MemberListResponse MemberList(MemberListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberListResponse response = new MemberListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().clusterClient
                        .MemberList(request, headers, deadline, cancellationToken);
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
        /// MemberListAsync lists all the members in the cluster in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<MemberListResponse> MemberListAsync(MemberListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            MemberListResponse response = new MemberListResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().clusterClient
                        .MemberListAsync(request, headers, deadline, cancellationToken);
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
