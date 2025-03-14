using System;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.interfaces
{
    /// <summary>
    /// Interface for the WatchManager class to make it mockable for testing
    /// </summary>
    public interface IWatchManager : IDisposable
    {
        /// <summary>
        /// Watches for changes to a key or range of keys
        /// </summary>
        /// <param name="request">The watch request</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">Optional headers to include with the request</param>
        /// <param name="deadline">Optional deadline for the request</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The ID of the watch</returns>
        long Watch(Etcdserverpb.WatchRequest request, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Watches for changes to a key or range of keys asynchronously
        /// </summary>
        /// <param name="request">The watch request</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">Optional headers to include with the request</param>
        /// <param name="deadline">Optional deadline for the request</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that resolves to the ID of the watch</returns>
        Task<long> WatchAsync(Etcdserverpb.WatchRequest request, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Watches for changes to a key range
        /// </summary>
        /// <param name="path">The path to watch</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">Optional headers to include with the request</param>
        /// <param name="deadline">Optional deadline for the request</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The ID of the watch</returns>
        long WatchRange(string path, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels a watch
        /// </summary>
        /// <param name="watchId">The ID of the watch to cancel</param>
        void CancelWatch(long watchId);
    }
}
