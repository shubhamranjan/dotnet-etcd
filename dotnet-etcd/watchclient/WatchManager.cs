#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;

namespace dotnet_etcd;

/// <summary>
///     Manages watch streams and provides a way to cancel watches
/// </summary>
public class WatchManager : IWatchManager
{
    /// <summary>
    ///     How long to wait for etcd to acknowledge a watch before giving up. Generous, because the
    ///     stream may have to reconnect (e.g. the server is restarting) before the create is accepted.
    /// </summary>
    private static readonly TimeSpan CreateWatchTimeout = TimeSpan.FromSeconds(30);

    private readonly object _lockObject = new();
    private readonly ConcurrentDictionary<long, WatchCancellation> _watches = new();
    private readonly ConcurrentDictionary<long, long> _watchIdMapping = new();

    /// <summary>Watches whose create request is still awaiting the server's Created acknowledgement.</summary>
    private readonly ConcurrentDictionary<long, TaskCompletionSource<WatchResponse>> _pendingCreates = new();

    /// <summary>
    ///     Tail of the callback chain. Every response is appended to this single chain so user
    ///     callbacks run off the receive loop but remain serialized and ordered, as they were when the
    ///     loop invoked them inline. Note a slow callback queues responses without bound — callbacks
    ///     must not block indefinitely.
    /// </summary>
    private readonly object _dispatchLock = new();

    private Task _dispatchChain = Task.CompletedTask;

    private readonly
        Func<Metadata?, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>
        _watchStreamFactory;

    private bool _disposed;
    private long _nextWatchId = 1;
    private Watcher? _watchStream;

    /// <summary>
    ///     Creates a new WatchManager
    /// </summary>
    /// <param name="watchStreamFactory">A factory function that creates a new watch stream</param>
    public WatchManager(
        Func<Metadata?, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>
            watchStreamFactory) => _watchStreamFactory =
        watchStreamFactory ?? throw new ArgumentNullException(nameof(watchStreamFactory));

    /// <summary>
    ///     Creates a new watch request
    /// </summary>
    /// <param name="request">The watch requests to create</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    public async Task<long> WatchAsync(WatchRequest request, Action<WatchResponse> callback, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a new watch stream if needed
        EnsureWatchStream(headers, deadline, cancellationToken);

        // Generate a new watch ID
        long watchId = Interlocked.Increment(ref _nextWatchId);

        // Create a cancellation token source that can be used to cancel the watch
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Work on our own copy: the watch id is stamped onto the request and the reconnect path
        // rewrites StartRevision on it, and neither should mutate the caller's object (which callers
        // may reuse across watches).
        request = request.Clone();
        request.CreateRequest.WatchId = watchId;

        // Create a watch cancellation object
        WatchCancellation watchCancellation = new()
        {
            WatchId = watchId,
            CancellationTokenSource = cts,
            Request = request,
            Callback = WrappedCallback,

            // Seed the resume point with the revision the caller asked to start from (etcd clientv3
            // does the same: `nextRev := w.initReq.rev`). Without this, a watch created with an
            // explicit StartRevision would look identical to a "from now" watch, and the Created ack —
            // which carries the *current* cluster revision — would advance the resume point past the
            // backlog the caller asked to replay.
            NextRevision = request.CreateRequest.StartRevision
        };

        // Completed when etcd acknowledges the watch. Continuations MUST run asynchronously: the
        // callback below is invoked from the stream's receive loop, so resuming the awaiting caller
        // inline would run its code on that loop and stall event delivery for every watch on the
        // stream (and deadlock outright if the caller then blocks).
        TaskCompletionSource<WatchResponse> acknowledged = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Register the watch BEFORE writing the create request: the server can answer while WriteAsync
        // is still in flight, and both TrackResumeRevision and the reconnect loop ignore watches that
        // are not in _watches yet.
        _watches[watchId] = watchCancellation;
        _pendingCreates[watchId] = acknowledged;

        try
        {
            await _watchStream!.CreateWatchAsync(request, WrappedCallback).ConfigureAwait(false);

            // Wait until etcd has actually registered the watch. Creating a watch is asynchronous on
            // the server, so until the Created ack arrives the watch does not exist: a caller that
            // wrote a key as soon as Watch() returned could have the write applied first and never
            // see the event. If the stream dies while we wait, HandleConnectionFailure re-sends the
            // create on the new stream and its ack completes this same task.
            TimeSpan ackTimeout = AckTimeoutFor(deadline);

            WatchResponse ack = await acknowledged.Task
                .WaitAsync(ackTimeout, cts.Token)
                .ConfigureAwait(false);

            if (ack.Canceled)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    $"etcd rejected the watch: {ack.CancelReason}"));
            }
        }
        catch (Exception ex)
        {
            // The watch was never established. Cancel first so any response that shows up late is
            // ignored, then drop the entry so the reconnect loop won't try to re-register a watch the
            // caller believes failed.
            _watches.TryRemove(watchId, out _);
            SafeCancel(cts);
            cts.Dispose();

            // The server may nonetheless have registered the watch (e.g. we timed out waiting for an
            // ack that was merely slow). Best-effort tear it down rather than leak a server-side
            // watcher that streams into a callback nobody listens to.
            if (_watchStream != null)
            {
                _ = _watchStream.CancelWatchAsync(watchId).ContinueWith(
                    t => _ = t.Exception, TaskScheduler.Default);
            }

            throw ex is TimeoutException
                ? new RpcException(new Status(StatusCode.DeadlineExceeded,
                    $"etcd did not acknowledge the watch within {AckTimeoutFor(deadline).TotalSeconds:0}s"))
                : ex;
        }
        finally
        {
            _pendingCreates.TryRemove(watchId, out _);
        }

        return watchId;

        // Create a wrapper callback that checks if the watch has been canceled
        void WrappedCallback(WatchResponse response)
        {
            // Complete the create before the cancellation guard below: a watch that is cancelled or
            // disposed while its create is still in flight must still release the awaiting caller
            // rather than leave it blocked until the timeout.
            if ((response.Created || response.Canceled) &&
                _pendingCreates.TryGetValue(watchId, out TaskCompletionSource<WatchResponse>? pending))
            {
                pending.TrySetResult(response);
            }

            if (cts.IsCancellationRequested)
            {
                return;
            }

            // If this is a creation response, update our watch ID mapping
            if (response.Created)
            {
                // Map the server-assigned watch ID to our client-generated watch ID
                _watchIdMapping[response.WatchId] = watchId;
            }

            // Track the revision to resume from on reconnect so no events are missed in the gap.
            TrackResumeRevision(watchId, response);

            Dispatch(watchId, response, callback, cts);
        }
    }

    /// <summary>
    ///     Hands a response to the user's callback off the stream's receive loop.
    ///     <para>
    ///         The receive loop invokes callbacks inline, so running user code on it would let one slow
    ///         or blocking callback stall event delivery for every watch on the stream — and a callback
    ///         that starts another watch would deadlock outright, because the loop it is blocking is the
    ///         only thing that could deliver the new watch's acknowledgement.
    ///     </para>
    ///     <para>
    ///         Responses are appended to a single chain, so callbacks stay serialized and in order
    ///         exactly as they were when they ran on the receive loop. That matters: overloads such as
    ///         WatchRange(string[] paths, method) hand the SAME delegate to several watches, and those
    ///         callers are entitled to assume it is never entered concurrently.
    ///     </para>
    /// </summary>
    private void Dispatch(long watchId, WatchResponse response, Action<WatchResponse> callback,
        CancellationTokenSource cts)
    {
        lock (_dispatchLock)
        {
            _dispatchChain = _dispatchChain.ContinueWith(_ =>
            {
                if (cts.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    callback(response);
                }
                catch (Exception ex)
                {
                    // A throwing user callback must not fault the chain and silently stop delivery of
                    // every subsequent event for this watch.
                    Console.Error.WriteLine($"Watch callback for watch {watchId} threw: {ex}");
                }
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }
    }

    /// <summary>
    ///     How long to wait for the create acknowledgement. Honours the caller's deadline when one is
    ///     given, so a caller that asked for a short deadline is not held for the full default.
    /// </summary>
    private static TimeSpan AckTimeoutFor(DateTime? deadline)
    {
        if (deadline == null)
        {
            return CreateWatchTimeout;
        }

        TimeSpan remaining = deadline.Value.ToUniversalTime() - DateTime.UtcNow;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero
            : remaining < CreateWatchTimeout ? remaining
            : CreateWatchTimeout;
    }

    /// <summary>
    ///     Cancels a token source that another owner may already have disposed. Cancel() throws on a
    ///     disposed source (Dispose() itself is idempotent), and both the create-failure path and
    ///     CancelWatch/Dispose can reach the same source.
    /// </summary>
    private static void SafeCancel(CancellationTokenSource cts)
    {
        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already torn down by the other owner; nothing to do.
        }
    }

    /// <summary>
    ///     Advances the stored resume revision for a watch based on a received response, mirroring the
    ///     etcd clientv3 "nextRev" logic: after an event batch resume from lastEvent.ModRevision + 1;
    ///     for a created/progress notification with no events, advance to the header revision. A
    ///     compaction cancel resets the resume point to the compact revision. Only ever moves forward.
    /// </summary>
    private void TrackResumeRevision(long clientWatchId, WatchResponse response)
    {
        if (!_watches.TryGetValue(clientWatchId, out WatchCancellation? watch))
        {
            return;
        }

        long candidate = watch.NextRevision;

        if (response.Canceled && response.CompactRevision > 0)
        {
            // Our resume point was compacted away; the earliest we can resume from is CompactRevision.
            candidate = response.CompactRevision;
        }
        else if (response.Created)
        {
            // A Created ack carries the CURRENT cluster revision, which says nothing about what this
            // watch has observed. Only use it to seed a brand new "watch from now" watch: for a watch
            // being re-registered after a reconnect (StartRevision > 0) the server is about to replay
            // the backlog from that revision, and advancing to the ack's header would skip straight
            // past it — losing exactly the events the resume exists to recover.
            if (candidate == 0 && response.Header != null && response.Header.Revision > 0)
            {
                candidate = response.Header.Revision + 1;
            }
        }
        else if (response.Events != null && response.Events.Count > 0)
        {
            long lastModRevision = response.Events[^1].Kv.ModRevision;
            if (lastModRevision + 1 > candidate)
            {
                candidate = lastModRevision + 1;
            }
        }
        else if (response.Header != null && response.Header.Revision > 0)
        {
            // Progress notification (no events): everything up to the header revision has been
            // observed, so the next start revision is header.Revision + 1.
            long boundary = response.Header.Revision + 1;
            if (boundary > candidate)
            {
                candidate = boundary;
            }
        }

        watch.NextRevision = candidate;
    }

    /// <summary>
    ///     Creates a new watch request
    /// </summary>
    /// <param name="request">The watch request to create</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    public long Watch(WatchRequest request, Action<WatchResponse> callback, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        // Run the async method synchronously. GetAwaiter().GetResult() rather than Wait()+Result so a
        // failure surfaces as the RpcException the async overloads throw, not wrapped in an
        // AggregateException that a `catch (RpcException)` would miss.
        Task<long> task = Task.Run(() => WatchAsync(request, callback, headers, deadline, cancellationToken), cancellationToken);
        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Watches a specific key
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    public long Watch(string key, Action<WatchEvent> action, Metadata? headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the key
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(key), ProgressNotify = true, PrevKv = true
            }
        };

        // Call the Watch method with the request
        return Watch(request, Callback, headers, deadline, cancellationToken);

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        void Callback(WatchResponse response)
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new() { Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type };
                action(watchEvent);
            }
        }
    }

    /// <summary>
    ///     Watches a range of keys with a prefix
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    public long WatchRange(string prefixKey, Action<WatchEvent> action, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the range
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(prefixKey)),
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the Watch method with the request
        return Watch(request, callback, headers, deadline, cancellationToken);
    }

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
    public long Watch(string key, long startRevision, Action<WatchEvent> action, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the key with start revision
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                StartRevision = startRevision,
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the Watch method with the request
        return Watch(request, callback, headers, deadline, cancellationToken);
    }

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
    public long WatchRange(string prefixKey, long startRevision, Action<WatchEvent> action, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the range with start revision
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(prefixKey)),
                StartRevision = startRevision,
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the Watch method with the request
        return Watch(request, callback, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches a specific key asynchronously
    /// </summary>
    /// <param name="key">Key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    public async Task<long> WatchAsync(string key, Action<WatchEvent> action, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the key
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(key), ProgressNotify = true, PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the WatchAsync method with the request
        return await WatchAsync(request, callback, headers, deadline, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Watches a range of keys with a prefix asynchronously
    /// </summary>
    /// <param name="prefixKey">Prefix key to watch</param>
    /// <param name="action">Action to be executed when watch event is triggered</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>Watch ID</returns>
    public async Task<long> WatchRangeAsync(string prefixKey, Action<WatchEvent> action, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the range
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(prefixKey)),
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the WatchAsync method with the request
        return await WatchAsync(request, callback, headers, deadline, cancellationToken).ConfigureAwait(false);
    }

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
    public async Task<long> WatchAsync(string key, long startRevision, Action<WatchEvent> action,
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the key with start revision
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(key),
                StartRevision = startRevision,
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the WatchAsync method with the request
        return await WatchAsync(request, callback, headers, deadline, cancellationToken).ConfigureAwait(false);
    }

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
    public async Task<long> WatchRangeAsync(string prefixKey, long startRevision, Action<WatchEvent> action,
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the range with start revision
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(prefixKey),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(prefixKey)),
                StartRevision = startRevision,
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Create a wrapper callback that converts the WatchResponse to a WatchEvent
        Action<WatchResponse> callback = response =>
        {
            if (response.Events == null)
            {
                return;
            }

            foreach (Event evt in response.Events)
            {
                WatchEvent watchEvent = new()
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                };
                action(watchEvent);
            }
        };

        // Call the WatchAsync method with the request
        return await WatchAsync(request, callback, headers, deadline, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Watches a key range
    /// </summary>
    /// <param name="path">The path to watch</param>
    /// <param name="callback">The callback to invoke when a watch event is received</param>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    public long WatchRange(string path, Action<WatchResponse> callback, Metadata? headers = null,
        DateTime? deadline = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Create a watch request for the range
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(path),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                ProgressNotify = true,
                PrevKv = true
            }
        };

        // Call the Watch method with the request
        return Watch(request, callback, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Cancels a watch request
    /// </summary>
    /// <param name="watchId">The ID of the watch to cancel</param>
    public void CancelWatch(long watchId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_watches.TryRemove(watchId, out WatchCancellation? watchCancellation))
        {
            return;
        }

        // Cancel the watch
        SafeCancel(watchCancellation.CancellationTokenSource);

        // Find the server watch ID that corresponds to our client watch ID
        long serverWatchId = GetServerWatchId(watchId);

        // Cancel the watch on the server if we found a mapping
        if (serverWatchId != -1 && _watchStream != null)
        {
            _watchIdMapping.TryRemove(serverWatchId, out _);
            _watchStream.CancelWatchAsync(serverWatchId).ContinueWith(_ =>
            {
                // Ignore exceptions
            });
        }

        // Dispose the cancellation token source
        watchCancellation.CancellationTokenSource.Dispose();
    }

    /// <summary>
    ///     Disposes the watch manager
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Release anyone still waiting for a watch to be acknowledged, so disposing the manager can
        // never leave a caller blocked until the create timeout.
        foreach (TaskCompletionSource<WatchResponse> pending in _pendingCreates.Values)
        {
            pending.TrySetException(new ObjectDisposedException(nameof(WatchManager)));
        }

        _pendingCreates.Clear();

        // Cancel all watches
        foreach (WatchCancellation watchCancellation in _watches.Values)
        {
            SafeCancel(watchCancellation.CancellationTokenSource);
            watchCancellation.CancellationTokenSource.Dispose();
        }

        _watches.Clear();

        // Dispose the watch stream
        if (_watchStream != null)
        {
            _watchStream.Dispose();
            _watchStream = null;
        }

        GC.SuppressFinalize(this);
    }

    private static string GetRangeEnd(string path)
    {
        // Calculate the range end for the given path
        // This is the same logic used in EtcdClient.GetRangeEnd
        byte[] bytes = Encoding.UTF8.GetBytes(path);
        for (int i = bytes.Length - 1; i >= 0; i--)
        {
            if (bytes[i] >= 0xff)
            {
                continue;
            }

            bytes[i]++;
            return Encoding.UTF8.GetString(bytes, 0, i + 1);
        }

        return string.Empty;
    }

    /// <summary>
    ///     Gets the server watch ID for a client watch ID
    /// </summary>
    /// <param name="clientWatchId">The client watch ID</param>
    /// <returns>The server watch ID, or -1 if not found</returns>
    private long GetServerWatchId(long clientWatchId)
    {
        foreach (KeyValuePair<long, long> kvp in _watchIdMapping)
        {
            if (kvp.Value == clientWatchId)
            {
                return kvp.Key;
            }
        }

        return -1;
    }

    /// <param name="cancellationToken">An optional token for canceling the call</param>
    private void EnsureWatchStream(Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            if (_watchStream != null)
            {
                return;
            }

            // Create a new watch stream
            IAsyncDuplexStreamingCall<WatchRequest, WatchResponse> watchStreamCall =
                _watchStreamFactory(headers, deadline, cancellationToken);
            _watchStream = new Watcher(watchStreamCall, HandleConnectionFailure);
        }
    }

    private void HandleConnectionFailure()
    {
        Watcher? abandoned;

        lock (_lockObject)
        {
            abandoned = _watchStream;
            _watchStream = null;
            _watchIdMapping.Clear();
        }

        // Tear the old stream down. Leaving it undisposed keeps its receive loop, gRPC call and
        // callbacks alive: if that stream is in fact still healthy, etcd goes on delivering the same
        // events on it as well as on the replacement, and every event is handed to the callback twice.
        abandoned?.Dispose();

        // Must run async to avoid blocking the caller (which might be the dead stream loop)
        Task.Run(async () =>
        {
            try
            {
                // Wait small delay to allow network to stabilize
                await Task.Delay(500);

                lock (_lockObject)
                {
                   if (_disposed) return;
                   EnsureWatchStream(null, null, default); 
                }

                bool anyFailed = false;

                foreach (var watch in _watches.Values)
                {
                    // Resume from the revision after the last observed event so events written while
                    // the stream was down are replayed instead of lost. A watch whose create was never
                    // acknowledged has nothing to resume from, but nothing can have been missed either:
                    // its Watch() call has not returned yet, so the caller cannot have written anything.
                    if (watch.NextRevision > 0 && watch.Request.CreateRequest != null)
                    {
                        watch.Request.CreateRequest.StartRevision = watch.NextRevision;
                    }

                    try
                    {
                        await _watchStream!.CreateWatchAsync(watch.Request, watch.Callback).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Keep going: one watch failing to re-register must not strand the others. But
                        // remember it failed — swallowing it here would otherwise disable the retry
                        // below and leave that watch silently dead for the life of the client.
                        anyFailed = true;
                        Console.Error.WriteLine($"Failed to re-register watch {watch.WatchId}: {ex.Message}");
                    }
                }

                if (anyFailed)
                {
                    throw new InvalidOperationException("one or more watches could not be re-registered");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Watch reconnection failed: {ex.Message}");
                // Retry in 5s
                _ = Task.Delay(5000).ContinueWith(_ => HandleConnectionFailure());
            }
        });
    }

    private class WatchCancellation
    {
        public long WatchId { get; set; }
        public required CancellationTokenSource CancellationTokenSource { get; set; }
        public required WatchRequest Request { get; set; }
        public required Action<WatchResponse> Callback { get; set; }


        /// <summary>
        ///     The revision to resume this watch from if the stream is re-established. Tracks the
        ///     revision after the last event/notification observed, so a reconnect does not miss
        ///     events written during the gap (mirrors the etcd clientv3 nextRev behavior).
        /// </summary>
        public long NextRevision { get; set; }
    }
}
