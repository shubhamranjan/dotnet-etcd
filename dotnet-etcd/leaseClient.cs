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
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseGrantResponse response = new LeaseGrantResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().leaseClient.LeaseGrant(request, headers);
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
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseGrantResponse response = new LeaseGrantResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().leaseClient.LeaseGrantAsync(request, headers);
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
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseRevokeResponse response = new LeaseRevokeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().leaseClient.LeaseRevoke(request, headers);
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
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseRevokeResponse response = new LeaseRevokeResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().leaseClient.LeaseRevokeAsync(request, headers);
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
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="leaseId"></param>
        /// <param name="token"></param>
        public async Task LeaseKeepAlive(long leaseId, CancellationToken token)
        {
            LeaseKeepAliveRequest request = new LeaseKeepAliveRequest();
            request.ID = leaseId;

            long? ttl = null;
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _balancer.GetConnection().leaseClient.LeaseKeepAlive())
                    {
                        token.ThrowIfCancellationRequested();

                        await leaser.RequestStream.WriteAsync(request);
                        await leaser.RequestStream.CompleteAsync();
                        if (!await leaser.ResponseStream.MoveNext(token))
                            throw new EndOfStreamException();

                        LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                        if (update.ID != leaseId || update.TTL == 0)  // expired
                            return;
                        if (ttl == null)
                            ttl = update.TTL;

                        await Task.Delay((int)(ttl * 1000 / 3), token);
                    }
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
        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method, CancellationToken token, Grpc.Core.Metadata headers = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _balancer.GetConnection().leaseClient.LeaseKeepAlive(headers))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(token))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                method(update);
                            }
                        });

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
                        throw ex;
                    }
                }
            }

        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token, Grpc.Core.Metadata headers = null)
        {

            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _balancer.GetConnection().leaseClient.LeaseKeepAlive(headers))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(token))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                foreach (Action<LeaseKeepAliveResponse> method in methods)
                                {
                                    method(update);
                                }

                            }
                        });

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
                        throw ex;
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
        /// <param name="token"></param>
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method, CancellationToken token, Grpc.Core.Metadata headers = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {

                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _balancer.GetConnection().leaseClient.LeaseKeepAlive(headers))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(token))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                method(update);
                            }
                        });

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
                        throw ex;
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
        public async Task LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token, Grpc.Core.Metadata headers = null)
        {
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _balancer.GetConnection().leaseClient.LeaseKeepAlive(headers))
                    {
                        Task leaserTask = Task.Run(async () =>
                        {
                            while (await leaser.ResponseStream.MoveNext(token))
                            {
                                LeaseKeepAliveResponse update = leaser.ResponseStream.Current;
                                foreach (Action<LeaseKeepAliveResponse> method in methods)
                                {
                                    method(update);
                                }

                            }
                        });

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
                        throw ex;
                    }
                }
            }

        }

        public LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseTimeToLiveResponse response = new LeaseTimeToLiveResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = _balancer.GetConnection().leaseClient.LeaseTimeToLive(request, headers);
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

        public async Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request, Grpc.Core.Metadata headers = null)
        {
            LeaseTimeToLiveResponse response = new LeaseTimeToLiveResponse();
            bool success = false;
            int retryCount = 0;
            while (!success)
            {
                try
                {
                    response = await _balancer.GetConnection().leaseClient.LeaseTimeToLiveAsync(request, headers);
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
