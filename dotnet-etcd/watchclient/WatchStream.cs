using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd;

/// <summary>
///     Manages a bidirectional streaming connection to the etcd watch API
/// </summary>
public class Watcher : IWatcher
{
    private readonly ConcurrentDictionary<long, Action<WatchResponse>> _callbacks = new();
    private readonly CancellationTokenSource _cts = new();

    private readonly IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> _streamingCall;


    /// <summary>
    ///     Creates a new Watcher
    /// </summary>
    /// <param name="streamingCall">The streaming call to use</param>
    public Watcher(IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> streamingCall)
    {
        _streamingCall = streamingCall ?? throw new ArgumentNullException(nameof(streamingCall));
        _ = ProcessWatchResponses();
    }


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

        _callbacks[request.CreateRequest.WatchId] = callback;

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

    private async Task ProcessWatchResponses()
    {
        try
        {
            while (await _streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                WatchResponse response = _streamingCall.ResponseStream.Current;
                if (!_callbacks.TryGetValue(response.WatchId, out Action<WatchResponse> cb))
                {
                    continue;
                }

                cb(response);

                // If the watch was canceled, remove the callback after invoking it
                if (response.Canceled)
                {
                    _callbacks.TryRemove(response.WatchId, out _);
                }
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // This is expected when the stream is canceled
        }
        catch (OperationCanceledException)
        {
            // This is expected when the token is canceled
        }
        catch (Exception ex)
        {
            // Log the exception
            await Console.Error.WriteAsync($"Error processing watch responses: {ex}");
#if DEBUG            // Only re-throw in debug mode to help with debugging
            throw;
#endif
        }
    }

    /// <summary>
    ///     Disposes the watch stream
    /// </summary>
    public void Dispose()
    {
        _cts.Cancel();
        _streamingCall.Dispose();

        GC.SuppressFinalize(this);
    }
}
