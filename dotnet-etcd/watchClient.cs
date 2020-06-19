using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.helper;
using Etcdserverpb;

using Google.Protobuf;

using Grpc.Core;

using static Mvccpb.Event.Types;

namespace dotnet_etcd
{
    /// <summary>
    /// WatchEvent class is used for retrieval of minimal
    /// data from watch events on etcd.
    /// </summary>
    public class WatchEvent
    {
        /// <summary>
        /// etcd Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// etcd value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// etcd watch event type (PUT,DELETE etc.)
        /// </summary>
        public EventType Type { get; set; }

    }

    // TODO: Add range methods for methods having  key as input instead of a whole watch request
    // TODO: Update documentation on how to call range requests for watch using watch request as input param
    public partial class EtcdClient
    {
        #region Watch Key

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest request, Action<WatchResponse> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest request, Action<WatchResponse>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchResponse> method in methods)
                            {
                                method(update);
                            }
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest request, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update.Events.Select(i =>
                                {
                                    return new WatchEvent
                                    {
                                        Key = i.Kv.Key.ToStringUtf8(),
                                        Value = i.Kv.Value.ToStringUtf8(),
                                        Type = i.Type
                                    };
                                }).ToArray()
                            );
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest request, Action<WatchEvent[]>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchEvent[]> method in methods)
                            {
                                method(update.Events.Select(i =>
                                    {
                                        return new WatchEvent
                                        {
                                            Key = i.Kv.Key.ToStringUtf8(),
                                            Value = i.Kv.Value.ToStringUtf8(),
                                            Type = i.Type
                                        };
                                    }).ToArray()
                                );
                            }
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches the keys according to the specified watch requests and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest[] requests, Action<WatchResponse> method,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest[] requests, Action<WatchResponse>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchResponse> method in methods)
                            {
                                method(update);
                            }
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest[] requests, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update.Events.Select(i =>
                                {
                                    return new WatchEvent
                                    {
                                        Key = i.Kv.Key.ToStringUtf8(),
                                        Value = i.Kv.Value.ToStringUtf8(),
                                        Type = i.Type
                                    };
                                }).ToArray()
                            );
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key according to the specified watch requests and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Request containing keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task Watch(WatchRequest[] requests, Action<WatchEvent[]>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchEvent[]> method in methods)
                            {
                                method(update.Events.Select(i =>
                                    {
                                        return new WatchEvent
                                        {
                                            Key = i.Kv.Key.ToStringUtf8(),
                                            Value = i.Kv.Value.ToStringUtf8(),
                                            Type = i.Type
                                        };
                                    }).ToArray()
                                );
                            }
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches the specified key and passes the watch response to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchResponse> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchResponse>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="key">Key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string key, Action<WatchEvent[]>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key)
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchResponse> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key)
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified keys and passes the watch response to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchResponse>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key)
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key)
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified keys and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="keys">Keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void Watch(string[] keys, Action<WatchEvent[]>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string key in keys)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = ByteString.CopyFromUtf8(key)
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), methods, headers, deadline, cancellationToken));
        }

        #endregion

        #region Watch Range of keys

        /// <summary>
        /// Watches a key range according to the specified watch request and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest request, Action<WatchResponse> method,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch request and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest request, Action<WatchResponse>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchResponse> method in methods)
                            {
                                method(update);
                            }
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest request, Action<WatchEvent[]> method,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update.Events.Select(i =>
                                {
                                    return new WatchEvent
                                    {
                                        Key = i.Kv.Key.ToStringUtf8(),
                                        Value = i.Kv.Value.ToStringUtf8(),
                                        Type = i.Type
                                    };
                                }).ToArray()
                            );
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch request and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="request">Watch Request containing key to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest request, Action<WatchEvent[]>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchEvent[]> method in methods)
                            {
                                method(update.Events.Select(i =>
                                    {
                                        return new WatchEvent
                                        {
                                            Key = i.Kv.Key.ToStringUtf8(),
                                            Value = i.Kv.Value.ToStringUtf8(),
                                            Type = i.Type
                                        };
                                    }).ToArray()
                                );
                            }
                        }
                    }, cancellationToken);

                    await watcher.RequestStream.WriteAsync(request);
                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches the key range according to the specified watch requests and
        /// passes the watch response to the method provided.
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest[] requests, Action<WatchResponse> method,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update);
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch requests and
        /// passes the watch response to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest[] requests, Action<WatchResponse>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchResponse> method in methods)
                            {
                                method(update);
                            }
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch request and
        /// passes the minimal watch event data to the method provided. 
        /// </summary>
        /// <param name="requests">Watch Requests containing keys to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest[] requests, Action<WatchEvent[]> method,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            method(update.Events.Select(i =>
                                {
                                    return new WatchEvent
                                    {
                                        Key = i.Kv.Key.ToStringUtf8(),
                                        Value = i.Kv.Value.ToStringUtf8(),
                                        Type = i.Type
                                    };
                                }).ToArray()
                            );
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches a key range according to the specified watch requests and
        /// passes the minimal watch event data to the methods provided. 
        /// </summary>
        /// <param name="requests">Watch Request containing keys to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public async Task WatchRange(WatchRequest[] requests, Action<WatchEvent[]>[] methods,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            await CallEtcdAsync(async (connection) =>
            {
                using (AsyncDuplexStreamingCall<WatchRequest, WatchResponse> watcher =
                    connection.watchClient.Watch(headers, deadline, cancellationToken))
                {
                    Task watcherTask = Task.Run(async () =>
                    {
                        while (await watcher.ResponseStream.MoveNext(cancellationToken))
                        {
                            WatchResponse update = watcher.ResponseStream.Current;
                            foreach (Action<WatchEvent[]> method in methods)
                            {
                                method(update.Events.Select(i =>
                                    {
                                        return new WatchEvent
                                        {
                                            Key = i.Kv.Key.ToStringUtf8(),
                                            Value = i.Kv.Value.ToStringUtf8(),
                                            Type = i.Type
                                        };
                                    }).ToArray()
                                );
                            }
                        }
                    }, cancellationToken);

                    foreach (WatchRequest request in requests)
                    {
                        await watcher.RequestStream.WriteAsync(request);
                    }

                    await watcher.RequestStream.CompleteAsync();
                    await watcherTask;
                }
            });
        }

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchResponse> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = GetStringByteForRangeRequests(path),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the watch response to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchResponse>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = GetStringByteForRangeRequests(path),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = GetStringByteForRangeRequests(path),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the methods provided.
        /// </summary>
        /// <param name="path">Path to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string path, Action<WatchEvent[]>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            WatchRequest request = new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = GetStringByteForRangeRequests(path),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                }
            };

            AsyncHelper.RunSync(async () => await Watch(request, methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchResponse> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the watch response to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which watch response should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchResponse>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), methods, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="method">Method to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchEvent[]> method, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), method, headers, deadline, cancellationToken));
        }

        /// <summary>
        /// Watches the specified key range and passes the minimal watch events data to the method provided.
        /// </summary>
        /// <param name="paths">Paths to be watched</param>
        /// <param name="methods">Methods to which minimal watch events data should be passed on</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        public void WatchRange(string[] paths, Action<WatchEvent[]>[] methods, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default)
        {
            List<WatchRequest> requests = new List<WatchRequest>();

            foreach (string path in paths)
            {
                WatchRequest request = new WatchRequest()
                {
                    CreateRequest = new WatchCreateRequest()
                    {
                        Key = GetStringByteForRangeRequests(path),
                        RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd())
                    }
                };
                requests.Add(request);
            }

            AsyncHelper.RunSync(async () => await Watch(requests.ToArray(), methods, headers, deadline, cancellationToken));
        }

        #endregion
    }
}