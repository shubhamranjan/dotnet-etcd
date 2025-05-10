using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;

namespace dotnet_etcd;

/// <summary>
///     Manages a bidirectional streaming connection to the etcd watch API
/// </summary>
#pragma warning disable CA1711
public class WatchStream : IWatchStream
#pragma warning restore CA1711
{
    private readonly ConcurrentDictionary<long, Action<WatchResponse>> _callbacks = new();
    private readonly CancellationTokenSource _cts = new();


    private readonly IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> _streamingCall;

    // Track the next temporary key to use for pending watch requests
    private long _nextTempKey = -1;

    /// <summary>
    ///     Creates a new WatchStream
    /// </summary>
    /// <param name="streamingCall">The streaming call to use</param>
    public WatchStream(IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> streamingCall) =>
        _streamingCall = streamingCall ?? throw new ArgumentNullException(nameof(streamingCall));

    /// <summary>
    ///     Creates a watch for the specified request
    /// </summary>
    /// <param name="request">The watch request</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <returns>A task that completes when the watch is created</returns>
    public async Task CreateWatchAsync(WatchRequest request, Action<WatchResponse> callback)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(callback);

        // Use a negative temporary key for pending watch requests
        // This ensures they don't conflict with actual watch IDs from etcd (which are positive)
        long tempKey = Interlocked.Decrement(ref _nextTempKey);
        _callbacks[tempKey] = callback;

        // Send the watch request
        await _streamingCall.RequestStream.WriteAsync(request);
    }

    /// <summary>
    ///     Cancels a watch with the specified ID
    /// </summary>
    /// <param name="watchId">The ID of the watch to cancel</param>
    /// <returns>A task that completes when the watch is canceled</returns>
    public async Task CancelWatchAsync(long watchId)
    {
        // Send a cancel request
        WatchRequest request = new() { CancelRequest = new WatchCancelRequest { WatchId = watchId } };

        await _streamingCall.RequestStream.WriteAsync(request);

        // Remove the callback
        _callbacks.TryRemove(watchId, out _);
    }

    /// <summary>
    ///     Disposes the watch stream
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _streamingCall.RequestStream.CompleteAsync().Wait();
        _streamingCall.Dispose();

        GC.SuppressFinalize(this);
    }
}
