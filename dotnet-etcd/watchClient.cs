using Etcdserverpb;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_etcd
{
    public partial class EtcdClient : IDisposable
    {
        public async void Watch(WatchRequest request, Action<WatchResponse> method)
        {

            try
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher = _watchClient.Watch())
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext())
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    });

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }

            }
            catch (RpcException)
            {
                // If connection issue, then re-initate the watch
                ResetConnection();
                Watch(request, method);
            }
            catch
            {
                throw;
            }
        }

        public async void Watch(WatchRequest[] requests, Action<WatchResponse> method)
        {

            try
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher = _watchClient.Watch())
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext())
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    });

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }

            }
            catch (RpcException)
            {
                // If connection issue, then re-initate the watch
                ResetConnection();
                Watch(requests, method);
            }
            catch
            {
                throw;
            }
        }
    }
}
