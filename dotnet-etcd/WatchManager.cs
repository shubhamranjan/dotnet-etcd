// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd
{
    /// <summary>
    /// Manages watch streams and provides a way to cancel watches
    /// </summary>
    public class WatchManager : IWatchManager
    {
        private readonly Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<Etcdserverpb.WatchRequest, Etcdserverpb.WatchResponse>> _watchStreamFactory;
        private readonly ConcurrentDictionary<long, WatchCancellation> _watches = new ConcurrentDictionary<long, WatchCancellation>();
        private IWatchStream _watchStream;
        private readonly object _lockObject = new object();
        private bool _disposed = false;
        private long _nextWatchId = 1;
        private readonly ConcurrentDictionary<long, long> _watchIdMapping = new ConcurrentDictionary<long, long>();

        /// <summary>
        /// Creates a new WatchManager
        /// </summary>
        /// <param name="watchStreamFactory">A factory function that creates a new watch stream</param>
        public WatchManager(Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<Etcdserverpb.WatchRequest, Etcdserverpb.WatchResponse>> watchStreamFactory)
        {
            _watchStreamFactory = watchStreamFactory ?? throw new ArgumentNullException(nameof(watchStreamFactory));
        }

        /// <summary>
        /// Creates a new watch request
        /// </summary>
        /// <param name="request">The watch request to create</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">The initial metadata to send with the call</param>
        /// <param name="deadline">An optional deadline for the call</param>
        /// <param name="cancellationToken">An optional token for canceling the call</param>
        /// <returns>A watch ID that can be used to cancel the watch</returns>
        public async Task<long> WatchAsync(Etcdserverpb.WatchRequest request, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WatchManager));

            // Create a new watch stream if needed
            EnsureWatchStream(headers, deadline, cancellationToken);

            // Generate a new watch ID
            long watchId = Interlocked.Increment(ref _nextWatchId);

            // Create a cancellation token source that can be used to cancel the watch
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Create a watch cancellation object
            var watchCancellation = new WatchCancellation
            {
                WatchId = watchId,
                CancellationTokenSource = cts
            };

            // Create a wrapper callback that checks if the watch has been canceled
            Action<Etcdserverpb.WatchResponse> wrappedCallback = response =>
            {
                if (!cts.IsCancellationRequested)
                {
                    // If this is a create response, update our watch ID mapping
                    if (response.Created)
                    {
                        // Map the server-assigned watch ID to our client-generated watch ID
                        _watchIdMapping[response.WatchId] = watchId;
                    }

                    callback(response);
                }
            };

            // Create the watch
            await _watchStream.CreateWatchAsync(request, wrappedCallback).ConfigureAwait(false);

            // Add the watch cancellation to the dictionary
            _watches[watchId] = watchCancellation;

            // Since we don't get a server watch ID from CreateWatchAsync, we can't map it
            // The server will assign a watch ID and include it in the watch response
            // Our wrappedCallback will handle this mapping when it receives the response

            return watchId;
        }

        /// <summary>
        /// Creates a new watch request
        /// </summary>
        /// <param name="request">The watch request to create</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">The initial metadata to send with the call</param>
        /// <param name="deadline">An optional deadline for the call</param>
        /// <param name="cancellationToken">An optional token for canceling the call</param>
        /// <returns>A watch ID that can be used to cancel the watch</returns>
        public long Watch(Etcdserverpb.WatchRequest request, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            // Run the async method synchronously
            var task = Task.Run(() => WatchAsync(request, callback, headers, deadline, cancellationToken));
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Watches a key range
        /// </summary>
        /// <param name="path">The path to watch</param>
        /// <param name="callback">The callback to invoke when a watch event is received</param>
        /// <param name="headers">The initial metadata to send with the call</param>
        /// <param name="deadline">An optional deadline for the call</param>
        /// <param name="cancellationToken">An optional token for canceling the call</param>
        /// <returns>A watch ID that can be used to cancel the watch</returns>
        public long WatchRange(string path, Action<Etcdserverpb.WatchResponse> callback, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WatchManager));

            // Create a watch request for the range
            var request = new Etcdserverpb.WatchRequest
            {
                CreateRequest = new Etcdserverpb.WatchCreateRequest
                {
                    Key = Google.Protobuf.ByteString.CopyFromUtf8(path),
                    RangeEnd = Google.Protobuf.ByteString.CopyFromUtf8(GetRangeEnd(path)),
                    ProgressNotify = true,
                    PrevKv = true
                }
            };

            // Call the Watch method with the request
            return Watch(request, callback, headers, deadline, cancellationToken);
        }

        private string GetRangeEnd(string path)
        {
            // Calculate the range end for the given path
            // This is the same logic used in EtcdClient.GetRangeEnd
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(path);
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] < 0xff)
                {
                    bytes[i]++;
                    return System.Text.Encoding.UTF8.GetString(bytes, 0, i + 1);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the server watch ID for a client watch ID
        /// </summary>
        /// <param name="clientWatchId">The client watch ID</param>
        /// <returns>The server watch ID, or -1 if not found</returns>
        private long GetServerWatchId(long clientWatchId)
        {
            foreach (var kvp in _watchIdMapping)
            {
                if (kvp.Value == clientWatchId)
                {
                    return kvp.Key;
                }
            }

            return -1;
        }

        /// <summary>
        /// Cancels a watch request
        /// </summary>
        /// <param name="watchId">The ID of the watch to cancel</param>
        public void CancelWatch(long watchId)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WatchManager));

            if (_watches.TryRemove(watchId, out var watchCancellation))
            {
                // Cancel the watch
                watchCancellation.CancellationTokenSource.Cancel();

                // Find the server watch ID that corresponds to our client watch ID
                long serverWatchId = GetServerWatchId(watchId);

                // Cancel the watch on the server if we found a mapping
                if (serverWatchId != -1 && _watchStream != null)
                {
                    _watchIdMapping.TryRemove(serverWatchId, out _);
                    _watchStream.CancelWatchAsync(serverWatchId).ContinueWith(task =>
                    {
                        // Ignore exceptions
                    });
                }

                // Dispose the cancellation token source
                watchCancellation.CancellationTokenSource.Dispose();
            }
        }

        private void EnsureWatchStream(Metadata headers, DateTime? deadline, CancellationToken cancellationToken)
        {
            lock (_lockObject)
            {
                if (_watchStream == null)
                {
                    // Create a new watch stream
                    var watchStreamCall = _watchStreamFactory(headers, deadline, cancellationToken);
                    _watchStream = new WatchStream(watchStreamCall);
                }
            }
        }

        /// <summary>
        /// Disposes the watch manager
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Cancel all watches
            foreach (var watchCancellation in _watches.Values)
            {
                watchCancellation.CancellationTokenSource.Cancel();
                watchCancellation.CancellationTokenSource.Dispose();
            }

            _watches.Clear();

            // Dispose the watch stream
            if (_watchStream != null)
            {
                _watchStream.Dispose();
                _watchStream = null;
            }
        }

        public class WatchCancellation
        {
            public long WatchId { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }
    }
}
