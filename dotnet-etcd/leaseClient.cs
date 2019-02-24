using Etcdserverpb;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request)
        {
            try
            {
                return _leaseClient.LeaseGrant(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request)
        {
            try
            {
                return await _leaseClient.LeaseGrantAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request)
        {
            try
            {
                return _leaseClient.LeaseRevoke(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request)
        {
            try
            {
                return await _leaseClient.LeaseRevokeAsync(request, _headers);
            }
            catch (RpcException ex)
            {
                ResetConnection(ex);
                throw;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method, CancellationToken token)
        {
            try
            {
                using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _leaseClient.LeaseKeepAlive())
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

            }
            catch (RpcException ex) when (ex.Status.Equals(StatusCode.Unavailable))
            {
                // If connection issue, then re-initate the LeaseKeepAlive request
                ResetConnection(ex);
                LeaseKeepAlive(request, method, token);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token)
        {

            try
            {
                using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _leaseClient.LeaseKeepAlive())
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

            }
            catch (RpcException ex) when (ex.Status.Equals(StatusCode.Unavailable))
            {
                // If connection issue, then re-initate the LeaseKeepAlive request
                ResetConnection(ex);
                LeaseKeepAlive(request, methods, token);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method, CancellationToken token)
        {

            try
            {
                using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _leaseClient.LeaseKeepAlive())
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

            }
            catch (RpcException ex) when (ex.Status.Equals(StatusCode.Unavailable))
            {
                // If connection issue, then re-initate the LeaseKeepAlive request
                ResetConnection(ex);
                LeaseKeepAlive(requests, method, token);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token)
        {

            try
            {
                using (AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> leaser = _leaseClient.LeaseKeepAlive())
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

            }
            catch (RpcException ex) when (ex.Status.Equals(StatusCode.Unavailable))
            {
                // If connection issue, then re-initate the watch
                ResetConnection(ex);
                LeaseKeepAlive(requests, methods, token);
            }
            catch
            {
                throw;
            }
        }

    }
}
