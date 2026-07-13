#nullable enable
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

    /// <summary>
    ///     gRPC allows only one pending write per stream ("Only one write can be pending at a time").
    ///     Creates and cancels are issued from user threads and from the reconnect loop, so the writes
    ///     must be serialized. The lock covers the write only — never a wait for a server response, or
    ///     a create awaiting its acknowledgement would block every cancel and reconnect behind it.
    /// </summary>
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private readonly IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> _streamingCall;
    private readonly Action? _onConnectionFailure;


    /// <summary>
    ///     Creates a new Watcher
    /// </summary>
    /// <param name="streamingCall">The streaming call to use</param>
    /// <param name="onConnectionFailure">Action to invoke when connection fails</param>
    public Watcher(IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> streamingCall, Action? onConnectionFailure = null)
    {
        _streamingCall = streamingCall ?? throw new ArgumentNullException(nameof(streamingCall));
        _onConnectionFailure = onConnectionFailure;
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

        long watchId = request.CreateRequest.WatchId;

        // Register before writing: the server can answer before WriteAsync returns.
        _callbacks[watchId] = callback;

        try
        {
            await WriteAsync(request).ConfigureAwait(false);
        }
        catch
        {
            // The create never reached the server; don't leave a callback behind for a watch that
            // does not exist.
            _callbacks.TryRemove(watchId, out _);
            throw;
        }
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

        await WriteAsync(request).ConfigureAwait(false);

        // Remove the callback
        _callbacks.TryRemove(watchId, out _);
    }

    private async Task WriteAsync(WatchRequest request)
    {
        await _writeLock.WaitAsync(_cts.Token).ConfigureAwait(false);
        try
        {
            await _streamingCall.RequestStream.WriteAsync(request).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task ProcessWatchResponses()
    {
        try
        {
            while (await _streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                WatchResponse response = _streamingCall.ResponseStream.Current;
                if (!_callbacks.TryGetValue(response.WatchId, out Action<WatchResponse>? cb))
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
        catch (RpcException ex)
        {
            // Log a simplified message for expected connection failures
            Console.WriteLine($"Watch stream connection lost: {ex.StatusCode} - {ex.Message}");
            _onConnectionFailure?.Invoke();
        }
        catch (Exception ex)
        {
            // Log the exception
            await Console.Error.WriteAsync($"Error processing watch responses: {ex}");
            _onConnectionFailure?.Invoke();
#if DEBUG
            // Only re-throw in debug mode to help with debugging
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

        // Deliberately not disposing _writeLock/_cts: a write may be in flight, and disposing them
        // underneath it would surface as an ObjectDisposedException from inside the semaphore instead
        // of the stream's own cancellation. Neither holds an unmanaged resource here, so letting the
        // GC reclaim them is safe.

        GC.SuppressFinalize(this);
    }
}
