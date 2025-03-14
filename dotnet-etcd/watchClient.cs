// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using static Mvccpb.Event.Types;

namespace dotnet_etcd;

/// <summary>
///     WatchEvent class is used for retrieval of minimal
///     data from watch events on etcd.
/// </summary>
public class WatchEvent
{
    /// <summary>
    ///     etcd Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     etcd value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     etcd watch event type (PUT,DELETE etc.)
    /// </summary>
    public EventType Type { get; set; }
}

public partial class EtcdClient
{
    // CancelWatch methods are already defined in EtcdClient.cs

    /// <summary>
    ///     Watches the specified requests and passes the watch response to the methods provided.
    /// </summary>
    /// <param name="requests">Watch Requests containing keys to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(WatchRequest[] requests, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (requests.Length != methods.Length)
        {
            throw new ArgumentException("The number of requests must match the number of methods");
        }

        for (int i = 0; i < requests.Length; i++)
        {
            _watchManager.Watch(requests[i], methods[i], headers, deadline, cancellationToken);
        }
    }

    /// <summary>
    ///     Watches the specified requests and passes the watch events to the methods provided.
    /// </summary>
    /// <param name="requests">Watch Requests containing keys to be watched</param>
    /// <param name="methods">Methods to which watch events should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(WatchRequest[] requests, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (requests.Length != methods.Length)
        {
            throw new ArgumentException("The number of requests must match the number of methods");
        }

        for (int i = 0; i < requests.Length; i++)
        {
            // Create a wrapper that converts WatchResponse to WatchEvent[]
            Action<WatchResponse> wrapper = response =>
            {
                WatchEvent[] events = response.Events.Select(e =>
                {
                    return new WatchEvent
                    {
                        Key = e.Kv.Key.ToStringUtf8(), Value = e.Kv.Value.ToStringUtf8(), Type = e.Type
                    };
                }).ToArray();

                methods[i](events);
            };

            _watchManager.Watch(requests[i], wrapper, headers, deadline, cancellationToken);
        }
    }

    /// <summary>
    ///     Watches the specified requests and passes the watch events to the methods provided asynchronously.
    /// </summary>
    /// <param name="requests">Watch Requests containing keys to be watched</param>
    /// <param name="methods">Methods to which watch events should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with an array of watch IDs</returns>
    public async Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchEvent[]>[] methods,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (requests.Length != methods.Length)
        {
            throw new ArgumentException("The number of requests must match the number of methods");
        }

        long[] watchIds = new long[requests.Length];

        for (int i = 0; i < requests.Length; i++)
        {
            // Create a wrapper that converts WatchResponse to WatchEvent[]
            Action<WatchResponse> wrapper = response =>
            {
                WatchEvent[] events = response.Events.Select(e =>
                {
                    return new WatchEvent
                    {
                        Key = e.Kv.Key.ToStringUtf8(), Value = e.Kv.Value.ToStringUtf8(), Type = e.Type
                    };
                }).ToArray();

                methods[i](events);
            };

            watchIds[i] = await _watchManager.WatchAsync(requests[i], wrapper, headers, deadline, cancellationToken)
                .ConfigureAwait(false);
        }

        return watchIds;
    }

    #region Watch Key

    /// <summary>
    ///     Watches a key according to the specified watch request and
    ///     passes the watch response to the method provided.
    /// </summary>
    /// <param name="request">Watch Request containing key to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with the watch ID</returns>
    public Task<long> WatchAsync(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        _watchManager.WatchAsync(request, method, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches a key according to the specified watch request and
    ///     passes the watch response to the methods provided.
    /// </summary>
    /// <param name="request">Watch Request containing key to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with the watch ID</returns>
    public Task<long> WatchAsync(WatchRequest request, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Create a wrapper method that calls all the methods
        Action<WatchResponse> wrapper = response =>
        {
            foreach (Action<WatchResponse> method in methods)
            {
                method(response);
            }
        };

        return _watchManager.WatchAsync(request, wrapper, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches a key according to the specified watch requests and
    ///     passes the watch response to the method provided.
    /// </summary>
    /// <param name="requests">Watch Requests containing keys to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with an array of watch IDs</returns>
    public async Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        long[] watchIds = new long[requests.Length];

        for (int i = 0; i < requests.Length; i++)
        {
            watchIds[i] = await _watchManager.WatchAsync(requests[i], method, headers, deadline, cancellationToken)
                .ConfigureAwait(false);
        }

        return watchIds;
    }

    /// <summary>
    ///     Watches a key according to the specified watch requests and
    ///     passes the watch response to the methods provided.
    /// </summary>
    /// <param name="requests">Watch Requests containing keys to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with an array of watch IDs</returns>
    public async Task<long[]> WatchAsync(WatchRequest[] requests, Action<WatchResponse>[] methods,
        Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (requests.Length != methods.Length)
        {
            throw new ArgumentException("The number of requests must match the number of methods");
        }

        long[] watchIds = new long[requests.Length];

        for (int i = 0; i < requests.Length; i++)
        {
            watchIds[i] = await _watchManager.WatchAsync(requests[i], methods[i], headers, deadline, cancellationToken)
                .ConfigureAwait(false);
        }

        return watchIds;
    }

    /// <summary>
    ///     Watches a key according to the specified watch request and
    ///     passes the minimal watch event data to the method provided.
    /// </summary>
    /// <param name="request">Watch Request containing key to be watched</param>
    /// <param name="method">Method to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(WatchRequest request, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Create a wrapper that converts WatchResponse to WatchEvent[]
        Action<WatchResponse> wrapper = response =>
        {
            WatchEvent[] events = response.Events.Select(i =>
            {
                return new WatchEvent
                {
                    Key = i.Kv.Key.ToStringUtf8(), Value = i.Kv.Value.ToStringUtf8(), Type = i.Type
                };
            }).ToArray();

            method(events);
        };

        return _watchManager.WatchAsync(request, wrapper, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches a key according to the specified watch request and
    ///     passes the minimal watch event data to the methods provided.
    /// </summary>
    /// <param name="request">Watch Request containing key to be watched</param>
    /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(WatchRequest request, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Create a wrapper that converts WatchResponse to WatchEvent[] and calls all methods
        Action<WatchResponse> wrapper = response =>
        {
            WatchEvent[] events = response.Events.Select(i =>
            {
                return new WatchEvent
                {
                    Key = i.Kv.Key.ToStringUtf8(), Value = i.Kv.Value.ToStringUtf8(), Type = i.Type
                };
            }).ToArray();

            foreach (Action<WatchEvent[]> method in methods)
            {
                method(events);
            }
        };

        return _watchManager.WatchAsync(request, wrapper, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key and passes the watch response to the method provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(WatchRequest request, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request },
        new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches a key according to the specified watch request and
    ///     passes the watch response to the methods provided.
    /// </summary>
    /// <param name="request">Watch Request containing key to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(WatchRequest request, Action<WatchResponse>[] methods,
        Metadata headers = null, DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(new WatchRequest[1] { request }, methods, headers,
        deadline, cancellationToken);


    /// <summary>
    ///     Watches the specified keys and passes the watch response to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string key, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(new string[1] { key },
        new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified key and passes the watch response to the methods provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string key, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        Watch(new string[1] { key }, methods, headers, deadline, cancellationToken);


    /// <summary>
    ///     Watches the specified keys and passes the watch response to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string[] keys, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(keys, new Action<WatchResponse>[1] { method }, headers,
        deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the watch response to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        List<WatchRequest> requests = new();

        foreach (string key in keys)
        {
            WatchRequest request = new()
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(key), ProgressNotify = true, PrevKv = true
                }
            };
            requests.Add(request);
        }

        Watch(requests.ToArray(), methods, headers, deadline, cancellationToken);
    }


    /// <summary>
    ///     Watches the specified key and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="method">Method to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string key, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(new string[1] { key },
        new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified key and passes the minimal watch events data to the methods provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string key, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        Watch(new string[1] { key }, methods, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="method">Method to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string[] keys, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => Watch(keys, new Action<WatchEvent[]>[1] { method }, headers,
        deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public void Watch(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        List<WatchRequest> requests = new();

        foreach (string key in keys)
        {
            WatchRequest request = new()
            {
                CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8(key) }
            };
            requests.Add(request);
        }

        Watch(requests.ToArray(), methods, headers, deadline, cancellationToken);
    }


    /// <summary>
    ///     Watches the specified key and passes the watch response to the method provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string key, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key },
        new Action<WatchResponse>[1] { method }, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified key and passes the watch response to the methods provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string key, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        WatchAsync(new string[1] { key }, methods, headers, deadline, cancellationToken);


    /// <summary>
    ///     Watches the specified keys and passes the watch response to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string[] keys, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => WatchAsync(keys, new Action<WatchResponse>[1] { method },
        headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the watch response to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string[] keys, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        List<WatchRequest> requests = new();

        foreach (string key in keys)
        {
            WatchRequest request = new()
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(key), ProgressNotify = true, PrevKv = true
                }
            };
            requests.Add(request);
        }

        return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="method">Method to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string key, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => WatchAsync(new string[1] { key },
        new Action<WatchEvent[]>[1] { method }, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified key and passes the minimal watch events data to the methods provided.
    /// </summary>
    /// <param name="key">Key to be watched</param>
    /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string key, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) =>
        WatchAsync(new string[1] { key }, methods, headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="method">Method to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string[] keys, Action<WatchEvent[]> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default) => WatchAsync(keys, new Action<WatchEvent[]>[1] { method },
        headers, deadline, cancellationToken);

    /// <summary>
    ///     Watches the specified keys and passes the minimal watch events data to the method provided.
    /// </summary>
    /// <param name="keys">Keys to be watched</param>
    /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    public Task WatchAsync(string[] keys, Action<WatchEvent[]>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        List<WatchRequest> requests = new();

        foreach (string key in keys)
        {
            WatchRequest request = new()
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(key), ProgressNotify = true, PrevKv = true
                }
            };
            requests.Add(request);
        }

        return WatchAsync(requests.ToArray(), methods, headers, deadline, cancellationToken);
    }

    #endregion

    #region Watch Range of keys

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided.
    /// </summary>
    /// <param name="path">Path to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    public long WatchRange(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = GetStringByteForRangeRequests(path),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                ProgressNotify = true,
                PrevKv = true
            }
        };

        return _watchManager.Watch(request, method, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the methods provided.
    /// </summary>
    /// <param name="path">Path to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A watch ID that can be used to cancel the watch</returns>
    public long WatchRange(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Create a wrapper method that calls all the methods
        Action<WatchResponse> wrapper = response =>
        {
            foreach (Action<WatchResponse> method in methods)
            {
                method(response);
            }
        };

        return WatchRange(path, wrapper, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided.
    /// </summary>
    /// <param name="paths">Paths to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>An array of watch IDs that can be used to cancel the watches</returns>
    public long[] WatchRange(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        long[] watchIds = new long[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            watchIds[i] = WatchRange(paths[i], method, headers, deadline, cancellationToken);
        }

        return watchIds;
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided.
    /// </summary>
    /// <param name="paths">Paths to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>An array of watch IDs that can be used to cancel the watches</returns>
    public long[] WatchRange(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (paths.Length != methods.Length)
        {
            throw new ArgumentException("The number of paths must match the number of methods");
        }

        long[] watchIds = new long[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            watchIds[i] = WatchRange(paths[i], methods[i], headers, deadline, cancellationToken);
        }

        return watchIds;
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided asynchronously.
    /// </summary>
    /// <param name="path">Path to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with the watch ID</returns>
    public Task<long> WatchRangeAsync(string path, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        WatchRequest request = new()
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = GetStringByteForRangeRequests(path),
                RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(path)),
                ProgressNotify = true,
                PrevKv = true
            }
        };

        return _watchManager.WatchAsync(request, method, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the methods provided asynchronously.
    /// </summary>
    /// <param name="path">Path to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with the watch ID</returns>
    public Task<long> WatchRangeAsync(string path, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Create a wrapper method that calls all the methods
        Action<WatchResponse> wrapper = response =>
        {
            foreach (Action<WatchResponse> method in methods)
            {
                method(response);
            }
        };

        return WatchRangeAsync(path, wrapper, headers, deadline, cancellationToken);
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided asynchronously.
    /// </summary>
    /// <param name="paths">Paths to be watched</param>
    /// <param name="method">Method to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with an array of watch IDs</returns>
    public async Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse> method, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        long[] watchIds = new long[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            watchIds[i] = await WatchRangeAsync(paths[i], method, headers, deadline, cancellationToken)
                .ConfigureAwait(false);
        }

        return watchIds;
    }

    /// <summary>
    ///     Watches the specified key range and passes the watch response to the method provided asynchronously.
    /// </summary>
    /// <param name="paths">Paths to be watched</param>
    /// <param name="methods">Methods to which watch response should be passed on</param>
    /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
    /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
    /// <param name="cancellationToken">An optional token for canceling the call.</param>
    /// <returns>A task that completes with an array of watch IDs</returns>
    public async Task<long[]> WatchRangeAsync(string[] paths, Action<WatchResponse>[] methods, Metadata headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        if (paths.Length != methods.Length)
        {
            throw new ArgumentException("The number of paths must match the number of methods");
        }

        long[] watchIds = new long[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            watchIds[i] = await WatchRangeAsync(paths[i], methods[i], headers, deadline, cancellationToken)
                .ConfigureAwait(false);
        }

        return watchIds;
    }

    #endregion
}
