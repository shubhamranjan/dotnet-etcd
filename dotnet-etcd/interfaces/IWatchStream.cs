using System;
using System.Threading.Tasks;
using Etcdserverpb;

namespace dotnet_etcd.interfaces;

/// <summary>
///     Interface for the Watcher class to make it mockable for testing
/// </summary>
public interface IWatcher : IDisposable
{
    /// <summary>
    ///     Creates a watch for the specified request
    /// </summary>
    /// <param name="request">The watch request</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <returns>A task that completes when the watch is created</returns>
    Task CreateWatchAsync(WatchRequest request, Action<WatchResponse> callback);

    /// <summary>
    ///     Cancels a watch with the specified ID
    /// </summary>
    /// <param name="watchId">The ID of the watch to cancel</param>
    /// <returns>A task that completes when the watch is canceled</returns>
    Task CancelWatchAsync(long watchId);
}
