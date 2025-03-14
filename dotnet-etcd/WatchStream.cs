// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
public class WatchStream : IWatchStream
{
    private readonly ConcurrentDictionary<long, Action<WatchResponse>> _callbacks = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _responseProcessingTask;

    private readonly IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> _streamingCall;

    // Track the next temporary key to use for pending watch requests
    private long _nextTempKey = -1;

    /// <summary>
    ///     Creates a new WatchStream
    /// </summary>
    /// <param name="streamingCall">The streaming call to use</param>
    public WatchStream(IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> streamingCall)
    {
        _streamingCall = streamingCall ?? throw new ArgumentNullException(nameof(streamingCall));
        _responseProcessingTask = Task.Run(ProcessWatchResponses);
    }

    /// <summary>
    ///     Creates a watch for the specified request
    /// </summary>
    /// <param name="request">The watch request</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <returns>A task that completes when the watch is created</returns>
    public async Task CreateWatchAsync(WatchRequest request, Action<WatchResponse> callback)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

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
    }

    private async Task ProcessWatchResponses()
    {
        try
        {
            while (await _streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                WatchResponse response = _streamingCall.ResponseStream.Current;

                // If this is a watch creation response, update the callback dictionary
                if (response.Created)
                {
                    // Find the oldest pending watch request (lowest negative key)
                    // This assumes watches are created in the order they are requested
                    long? oldestPendingKey = null;
                    foreach (long key in _callbacks.Keys)
                    {
                        if (key < 0 && (!oldestPendingKey.HasValue || key < oldestPendingKey.Value))
                        {
                            oldestPendingKey = key;
                        }
                    }

                    // If we found a pending watch request, move its callback to the assigned watch ID
                    if (oldestPendingKey.HasValue &&
                        _callbacks.TryRemove(oldestPendingKey.Value, out Action<WatchResponse> callback))
                    {
                        _callbacks[response.WatchId] = callback;

                        // Also invoke the callback with the creation response
                        callback(response);
                    }
                }
                // For non-creation responses, just invoke the callback if we have one
                else if (_callbacks.TryGetValue(response.WatchId, out Action<WatchResponse> cb))
                {
                    cb(response);
                }

                // If the watch was canceled, remove the callback
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
            Console.Error.WriteLine($"Error processing watch responses: {ex}");
        }
    }
}
