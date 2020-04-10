using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        /// <summary>
        /// LeaseGrant creates a lease which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.leaseClient
                .LeaseGrant(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.leaseClient
                .LeaseGrantAsync(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.leaseClient
                .LeaseRevoke(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.leaseClient
                .LeaseRevokeAsync(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="leaseId"></param>
        /// <param name="cancellationToken"></param>
        public async Task LeaseKeepAlive(long leaseId, CancellationToken cancellationToken)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                        _balancer.GetConnection().leaseClient.LeaseKeepAlive(cancellationToken: cancellationToken))
                    {
                        LeaseKeepAliveRequest request = new LeaseKeepAliveRequest
                        {
                            ID = leaseId
                        };

                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await leaser.RequestStream.WriteAsync(request);
                            if (!await leaser.ResponseStream.MoveNext(cancellationToken))
                            {
                                await leaser.RequestStream.CompleteAsync();
                                throw new EndOfStreamException();
                            }

                            LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                            if (update.ID != leaseId || update.TTL == 0) // expired
                            {
                                await leaser.RequestStream.CompleteAsync();
                                return;
                            }

                            await Task.Delay(TimeSpan.FromMilliseconds(update.TTL * 1000 / 3), cancellationToken);
                        }
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="method"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                        _balancer.GetConnection().leaseClient
                            .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(cancellationToken))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                method(update);
                            }
                        }, cancellationToken);

                        await leaser.RequestStream.WriteAsync(request);
                        await leaser.RequestStream.CompleteAsync();
                        await leaserTask;
                    }

                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="methods"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null)
        {

            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                        _balancer.GetConnection().leaseClient
                            .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(cancellationToken))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                foreach (Action<LeaseKeepAliveResponse> method in methods)
                                {
                                    method(update);
                                }

                            }
                        }, cancellationToken);

                        await leaser.RequestStream.WriteAsync(request);
                        await leaser.RequestStream.CompleteAsync();
                        await leaserTask;
                    }

                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="method"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {

                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                        _balancer.GetConnection().leaseClient
                            .LeaseKeepAlive(headers, cancellationToken: cancellationToken))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(cancellationToken))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                method(update);
                            }
                        }, cancellationToken);

                        foreach (LeaseKeepAliveRequest request in requests)
                        {
                            await leaser.RequestStream.WriteAsync(request);
                        }

                        await leaser.RequestStream.CompleteAsync();
                        await leaserTask;
                    }

                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null, DateTime? deadline = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser =
                        _balancer.GetConnection().leaseClient
                            .LeaseKeepAlive(headers, deadline, cancellationToken))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(cancellationToken))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                foreach (Action<LeaseKeepAliveResponse> method in methods)
                                {
                                    method(update);
                                }

                            }
                        }, cancellationToken);

                        foreach (LeaseKeepAliveRequest request in requests)
                        {
                            await leaser.RequestStream.WriteAsync(request);
                        }

                        await leaser.RequestStream.CompleteAsync();
                        await leaserTask;
                    }

                    success = true;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    retryCount++;
                    if (retryCount >= _balancer._numNodes)
                    {
                        throw;
                    }
                }
            }

        }

        /// <summary>
        /// LeaseTimeToLive retrieves lease information.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return CallEtcd((connection) => connection.leaseClient
                .LeaseTimeToLive(request, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// LeaseTimeToLive retrieves lease information.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The call object.</returns>
        public async Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            return await CallEtcdAsync(async (connection) => await connection.leaseClient
                .LeaseTimeToLiveAsync(request, headers, deadline, cancellationToken));
        }
    }
}