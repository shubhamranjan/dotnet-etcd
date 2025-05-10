using System;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.interfaces;

/// <summary>
///     Interface for WatchManager to make it testable
/// </summary>
public interface IWatchManager : IDisposable
{
    /// <summary>
    ///     Creates a new watch request
    /// </summary>
    /// <param name="request">The watch request to create</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    Task<long> WatchAsync(WatchRequest request, Action<WatchResponse> callback, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new watch request
    /// </summary>
    /// <param name="request">The watch request to create</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    long Watch(WatchRequest request, Action<WatchResponse> callback, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a key range
    /// </summary>
    /// <param name="path">The path to watch</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    long WatchRange(string path, Action<WatchResponse> callback, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a specific key
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    long Watch(string key, Action<WatchEvent> action, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a range of keys with a prefix
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    long WatchRange(string prefixKey, Action<WatchEvent> action, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a specific key with start revision
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="startRevision">Start revision</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    long Watch(string key, long startRevision, Action<WatchEvent> action, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a range of keys with a prefix and start revision
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="startRevision">Start revision</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    long WatchRange(string prefixKey, long startRevision, Action<WatchEvent> action, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a specific key asynchronously
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    Task<long> WatchAsync(string key, Action<WatchEvent> action, Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a range of keys with a prefix asynchronously
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    Task<long> WatchRangeAsync(string prefixKey, Action<WatchEvent> action, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a specific key with start revision asynchronously
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="startRevision">Start revision</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    Task<long> WatchAsync(string key, long startRevision, Action<WatchEvent> action, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Watches a range of keys with a prefix and start revision asynchronously
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="startRevision">Start revision</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    Task<long> WatchRangeAsync(string prefixKey, long startRevision, Action<WatchEvent> action, Metadata headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cancels a watch
    /// </summary>
    /// <param name="watchId">Watch ID to cancel</param>
    void CancelWatch(long watchId);
}
