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
        /// LeaseGrant creates a lease which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseGrantResponse LeaseGrant(LeaseGrantRequest request, Metadata headers = null)
        {

            return _balancer.GetConnection().leaseClient.LeaseGrant(request, headers);

        }

        /// <summary>
        /// LeaseGrant creates a lease in async which expires if the server does not receive a keepAlive
        /// within a given time to live period. All keys attached to the lease will be expired and
        /// deleted if the lease expires. Each expired key generates a delete event in the event history.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseGrantResponse> LeaseGrantAsync(LeaseGrantRequest request, Metadata headers = null)
        {

            return await _balancer.GetConnection().leaseClient.LeaseGrantAsync(request, headers);

        }

        /// <summary>
        /// LeaseRevoke revokes a lease. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public LeaseRevokeResponse LeaseRevoke(LeaseRevokeRequest request, Metadata headers = null)
        {

            return _balancer.GetConnection().leaseClient.LeaseRevoke(request, headers);

        }

        /// <summary>
        /// LeaseRevoke revokes a lease in async. All keys attached to the lease will expire and be deleted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaseRevokeResponse> LeaseRevokeAsync(LeaseRevokeRequest request, Metadata headers = null)
        {

            return await _balancer.GetConnection().leaseClient.LeaseRevokeAsync(request, headers);

        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse> method, CancellationToken token, Metadata headers = null)
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


        }

        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest request, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token, Metadata headers = null)
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


        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="method"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse> method, CancellationToken token, Metadata headers = null)
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


        }


        /// <summary>
        /// LeaseKeepAlive keeps the lease alive by streaming keep alive requests from the client
        /// to the server and streaming keep alive responses from the server to the client.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="methods"></param>
        /// <param name="token"></param>
        public async void LeaseKeepAlive(LeaseKeepAliveRequest[] requests, Action<LeaseKeepAliveResponse>[] methods, CancellationToken token, Metadata headers = null)
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


        }

        public LeaseTimeToLiveResponse LeaseTimeToLive(LeaseTimeToLiveRequest request, Metadata headers = null)
        {

            return _balancer.GetConnection().leaseClient.LeaseTimeToLive(request, headers);

        }

        public async Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(LeaseTimeToLiveRequest request, Metadata headers = null)
        {


            return await _balancer.GetConnection().leaseClient.LeaseTimeToLiveAsync(request, headers);

        }
    }
}
