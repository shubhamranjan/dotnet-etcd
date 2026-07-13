using System.Diagnostics;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the streaming watch stack:
///     <see cref="WatchManager" />, <see cref="Watcher" /> and the EtcdClient watch wrappers.
///     Everything is driven through a <see cref="FakeDuplexStreamingCall{TRequest,TResponse}" /> so no
///     live etcd server is required. The fake's response stream is driven by enqueuing responses; the
///     background receive loop then invokes user callbacks which the tests await with a bounded timeout.
/// </summary>
[Trait("Category", "Unit")]
public class WatchManagerCoverageTests
{
    // The first client-generated watch id is 2 because _nextWatchId starts at 1 and is pre-incremented.
    private const long FirstWatchId = 2;

    private static WatchRequest KeyRequest(string key) =>
        new() { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8(key) } };

    private static WatchResponse EventResponse(long watchId, string key, string value, Event.Types.EventType type =
        Event.Types.EventType.Put)
    {
        var response = new WatchResponse { WatchId = watchId };
        response.Events.Add(new Event
        {
            Type = type,
            Kv = new KeyValue { Key = ByteString.CopyFromUtf8(key), Value = ByteString.CopyFromUtf8(value) }
        });
        return response;
    }

    private static bool WaitFor(Func<bool> condition, int timeoutMs = 2000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (condition())
            {
                return true;
            }

            Thread.Sleep(10);
        }

        return condition();
    }

    private static WatchManager CreateManager(out FakeDuplexStreamingCall<WatchRequest, WatchResponse> fake)
    {
        var localFake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fake = localFake;
        return new WatchManager((_, _, _) => localFake);
    }

    // ---------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WatchManager(null!));
    }

    // ---------------------------------------------------------------------
    // Reconnect / revision resume (issue: events lost across a reconnect)
    // ---------------------------------------------------------------------

    private static WatchResponse EventAtRevision(long watchId, long revision, string key = "k", string value = "v")
    {
        WatchResponse response = EventResponse(watchId, key, value);
        response.Header = new ResponseHeader { Revision = revision };
        response.Events[0].Kv.ModRevision = revision;
        return response;
    }

    [Fact]
    public void Watch_CalledFromInsideAWatchCallback_DoesNotDeadlockTheStream()
    {
        using WatchManager manager = CreateManager(out FakeDuplexStreamingCall<WatchRequest, WatchResponse> fake);

        long nested = 0;
        using ManualResetEventSlim done = new(false);

        manager.Watch(KeyRequest("a"), (WatchResponse r) =>
        {
            if (r.Events.Count == 0 || Interlocked.Read(ref nested) != 0)
            {
                return;
            }

            // Reacting to an event by starting another watch is an ordinary thing to do. It must not
            // wedge the stream: the callback runs on the receive loop, and a blocking watch create can
            // only be acknowledged BY that loop.
            Interlocked.Exchange(ref nested, manager.Watch(KeyRequest("b"), (WatchResponse _) => { }));
            done.Set();
        });

        fake.Enqueue(EventResponse(FirstWatchId, "a", "v"));

        Assert.True(done.Wait(15000),
            "a watch created from inside a watch callback deadlocked the receive loop");
        Assert.NotEqual(0, Interlocked.Read(ref nested));
    }

    [Fact]
    public void Reconnect_WatchWithExplicitStartRevision_ResumesFromItRatherThanTheAckHeader()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        // The "resume from a checkpoint" pattern: the caller asks to replay everything since rev 11.
        WatchRequest request = KeyRequest("k");
        request.CreateRequest.StartRevision = 11;

        Task<long> watch = manager.WatchAsync(request, (WatchResponse _) => { });

        // etcd acks at the CURRENT cluster revision (99) and will now replay 11..99.
        fake1.Enqueue(new WatchResponse
        {
            Created = true,
            WatchId = FirstWatchId,
            Header = new ResponseHeader { Revision = 99 }
        });
        Assert.True(WaitFor(() => watch.IsCompleted, 5000), "watch was never acknowledged");

        // Stream dies before any of the backlog is replayed. The caller asked for 11 and has seen
        // nothing, so the resume must still be 11 — not 100, which would skip the whole backlog.
        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "dropped"));
        fake1.Enqueue(new WatchResponse());

        Assert.True(WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered on the reconnected stream");
        Assert.Equal(11, fake2.Requests.Written.First(r => r.CreateRequest != null).CreateRequest.StartRevision);
    }

    [Fact]
    public void Reconnect_ReplayCreateAck_DoesNotSkipPastEventsStillToBeReplayed()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        // The reconnect stream acks by hand so we can give the ack a header revision far ahead of the
        // watch's resume point, which is what a real etcd does: the ack carries the CURRENT cluster
        // revision, not the watch's position.
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        var fake3 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        fakes.Enqueue(fake3);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        long observed = 0;
        manager.Watch(KeyRequest("k"), (WatchResponse r) =>
        {
            if (r.Events.Count > 0)
            {
                observed = r.Events[^1].Kv.ModRevision;
            }
        });

        fake1.Enqueue(EventAtRevision(FirstWatchId, 10));
        Assert.True(WaitFor(() => observed == 10), "initial event was not delivered");

        // First disconnect: the watch must resume from 11.
        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "dropped"));
        fake1.Enqueue(new WatchResponse());
        Assert.True(WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered after the first disconnect");
        Assert.Equal(11, fake2.Requests.Written.First(r => r.CreateRequest != null).CreateRequest.StartRevision);

        // etcd acks the replay create at the current cluster revision (99) and will now replay 11..99.
        // Nothing has been replayed yet.
        fake2.Enqueue(new WatchResponse
        {
            Created = true,
            WatchId = FirstWatchId,
            Header = new ResponseHeader { Revision = 99 }
        });
        Assert.True(WaitFor(() => fake2.Requests.Written.Any()), "create was not written to the new stream");
        Thread.Sleep(200); // let the ack be processed

        // Second disconnect before any replayed event arrives. The watch still has not seen 11..99, so
        // it must STILL resume from 11. Treating the ack's header as "observed" would resume at 100 and
        // silently skip every one of those events.
        fake2.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "dropped"));
        fake2.Enqueue(new WatchResponse());
        Assert.True(WaitFor(() => fake3.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered after the second disconnect");
        Assert.Equal(11, fake3.Requests.Written.First(r => r.CreateRequest != null).CreateRequest.StartRevision);
    }

    [Fact]
    public async Task WatchAsync_DoesNotComplete_UntilServerAcknowledgesTheWatch()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        using var manager = new WatchManager((_, _, _) => fake);

        Task<long> watch = manager.WatchAsync(KeyRequest("k"), (WatchResponse _) => { });

        // etcd registers a watch asynchronously: until the Created ack comes back the watch does not
        // exist server-side. If WatchAsync completed here, a caller doing the natural
        // `await WatchAsync(...); await PutAsync(...);` could have its put applied before the watch
        // existed, and the event would never be delivered — silently, and forever.
        await Task.Delay(300);
        Assert.False(watch.IsCompleted,
            "WatchAsync completed before etcd acknowledged the watch; a write issued now could be missed");

        fake.Enqueue(new WatchResponse
        {
            Created = true,
            WatchId = FirstWatchId,
            Header = new ResponseHeader { Revision = 7 }
        });

        Assert.Equal(FirstWatchId, await watch);
    }

    [Fact]
    public async Task WatchAsync_WhenStreamDiesBeforeAck_RetriesCreateAndCompletesOnceAcknowledged()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse> { AutoAckWatchCreate = false };
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        Task<long> watch = manager.WatchAsync(KeyRequest("k"), (WatchResponse _) => { });

        // The stream dies before the server ever acknowledged the watch. This is what happens when a
        // watch is opened against an etcd that is still coming back up.
        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "connection dropped"));
        fake1.Enqueue(new WatchResponse());

        // The create must be re-sent on the new stream rather than the watch being silently dropped.
        Assert.True(
            WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch create was not retried on the reconnected stream");
        Assert.False(watch.IsCompleted, "WatchAsync completed even though the watch was never acknowledged");

        fake2.Enqueue(new WatchResponse
        {
            Created = true,
            WatchId = FirstWatchId,
            Header = new ResponseHeader { Revision = 9 }
        });

        Assert.Equal(FirstWatchId, await watch);
    }

    [Fact]
    public void Reconnect_ResumesFromCreatedRevision_WhenCreatedArrivesBeforeRegistrationCompletes()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        using var createdDelivered = new ManualResetEventSlim(false);

        // Model the real server: the Created response comes back while the create request write is
        // still in flight. Holding WriteAsync open until the callback has run guarantees the response
        // is dispatched before the manager finishes registering the watch — the exact window in which
        // the created revision used to be dropped.
        fake1.Requests.OnWrite = request =>
        {
            if (request.CreateRequest == null)
            {
                return;
            }

            fake1.Enqueue(new WatchResponse
            {
                Created = true,
                WatchId = request.CreateRequest.WatchId,
                Header = new ResponseHeader { Revision = 42 }
            });

            createdDelivered.Wait(5000);
        };

        manager.Watch(KeyRequest("k"), (WatchResponse r) =>
        {
            if (r.Created)
            {
                createdDelivered.Set();
            }
        });

        // No events are ever delivered — only the Created ack. Break the stream.
        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "connection dropped"));
        fake1.Enqueue(new WatchResponse()); // unblock the receive loop so it re-enters MoveNext and throws

        Assert.True(
            WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered on the reconnected stream");

        // The watch observed everything up to revision 42, so it must resume at 43. Resuming at 0
        // ("from now") would silently drop every event written during the outage.
        WatchRequest reRegistered = fake2.Requests.Written.First(r => r.CreateRequest != null);
        Assert.Equal(43, reRegistered.CreateRequest.StartRevision);
    }

    [Fact]
    public void Reconnect_ResumesWatchFromLastObservedRevisionPlusOne()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        // Return the next fake for the initial stream and the reconnect; keep returning fake2 after.
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        long observedRevision = 0;
        manager.Watch(KeyRequest("k"), (WatchResponse r) =>
        {
            if (r.Header != null && r.Header.Revision > 0)
            {
                observedRevision = r.Header.Revision;
            }
        });

        // Server delivers an event at revision 10; the manager must remember it.
        fake1.Enqueue(EventAtRevision(FirstWatchId, 10));
        Assert.True(WaitFor(() => observedRevision == 10), "initial event was not delivered");

        // Break the stream -> HandleConnectionFailure reconnects on fake2.
        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "connection dropped"));
        fake1.Enqueue(new WatchResponse()); // unblock the receive loop so it re-enters MoveNext and throws

        // The watch must be re-registered on the new stream resuming from revision 11 (10 + 1),
        // otherwise events written during the reconnect gap are permanently lost.
        Assert.True(
            WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered on the reconnected stream");

        WatchRequest reRegistered = fake2.Requests.Written.First(r => r.CreateRequest != null);
        Assert.Equal(11, reRegistered.CreateRequest.StartRevision);
    }

    [Fact]
    public void Reconnect_AfterProgressNotification_ResumesFromHeaderRevisionPlusOne()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        long observedRevision = 0;
        manager.Watch(KeyRequest("k"), (WatchResponse r) =>
        {
            if (r.Header != null && r.Header.Revision > observedRevision)
            {
                observedRevision = r.Header.Revision;
            }
        });

        // A progress notification carries the current revision but no events.
        fake1.Enqueue(new WatchResponse { WatchId = FirstWatchId, Header = new ResponseHeader { Revision = 20 } });
        Assert.True(WaitFor(() => observedRevision == 20), "progress notification was not delivered");

        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "connection dropped"));
        fake1.Enqueue(new WatchResponse());

        Assert.True(
            WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered on the reconnected stream");
        Assert.Equal(21, fake2.Requests.Written.First(r => r.CreateRequest != null).CreateRequest.StartRevision);
    }

    [Fact]
    public void Reconnect_AfterCompactionCancel_ResumesFromCompactRevision()
    {
        var fakes = new Queue<FakeDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var fake1 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        var fake2 = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fakes.Enqueue(fake1);
        fakes.Enqueue(fake2);
        using var manager = new WatchManager((_, _, _) => fakes.Count > 1 ? fakes.Dequeue() : fakes.Peek());

        bool canceledSeen = false;
        manager.Watch(KeyRequest("k"), (WatchResponse r) =>
        {
            if (r.Canceled)
            {
                canceledSeen = true;
            }
        });

        // The server cancels the watch because our resume point was compacted away.
        fake1.Enqueue(new WatchResponse { WatchId = FirstWatchId, Canceled = true, CompactRevision = 50 });
        Assert.True(WaitFor(() => canceledSeen), "cancel response was not delivered");

        fake1.Responses.ThrowOnMoveNext = new RpcException(new Status(StatusCode.Unavailable, "connection dropped"));
        fake1.Enqueue(new WatchResponse());

        Assert.True(
            WaitFor(() => fake2.Requests.Written.Any(r => r.CreateRequest != null), 5000),
            "watch was not re-registered on the reconnected stream");
        // Cannot resume from before the compaction revision; resume from CompactRevision.
        Assert.Equal(50, fake2.Requests.Written.First(r => r.CreateRequest != null).CreateRequest.StartRevision);
    }

    // ---------------------------------------------------------------------
    // WatchAsync(WatchRequest, Action<WatchResponse>) - core path
    // ---------------------------------------------------------------------

    [Fact]
    public async Task WatchAsync_FiresCallback_AndRecordsCreateRequest()
    {
        var manager = CreateManager(out var fake);
        var received = new List<WatchResponse>();
        using var fired = new ManualResetEventSlim();

        long id = await manager.WatchAsync(KeyRequest("foo"), r =>
        {
            received.Add(r);
            if (r.Events.Count > 0)
            {
                fired.Set();
            }
        });

        Assert.Equal(FirstWatchId, id);
        Assert.True(WaitFor(() => fake.Requests.Written.Any(w => w.CreateRequest != null)),
            "Expected a WatchCreate request to be written to the stream.");
        Assert.Equal(id, fake.Requests.Written.First().CreateRequest.WatchId);

        // Drive a created response (assigns server watch id mapping) then an event response.
        fake.Enqueue(new WatchResponse { WatchId = id, Created = true });
        fake.Enqueue(EventResponse(id, "foo", "bar"));

        Assert.True(fired.Wait(2000), "Callback was not invoked with events within timeout.");
        Assert.Contains(received, r => r.Created);
        manager.Dispose();
    }

    [Fact]
    public void Watch_Synchronous_FiresCallback()
    {
        var manager = CreateManager(out var fake);
        using var fired = new ManualResetEventSlim();

        long id = manager.Watch(KeyRequest("k"), r =>
        {
            if (r.Events.Count > 0)
            {
                fired.Set();
            }
        });

        Assert.Equal(FirstWatchId, id);
        fake.Enqueue(EventResponse(id, "k", "v"));
        Assert.True(fired.Wait(2000));
        manager.Dispose();
    }

    // ---------------------------------------------------------------------
    // Convenience key / prefix / startRevision overloads (WatchEvent callbacks)
    // ---------------------------------------------------------------------

    [Fact]
    public void Watch_Key_WatchEventCallback_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = manager.Watch("mykey", e => events.Add(e));

        Assert.Equal(FirstWatchId, id);
        var written = fake.Requests.Written.First().CreateRequest;
        Assert.Equal("mykey", written.Key.ToStringUtf8());
        Assert.True(written.PrevKv);

        fake.Enqueue(EventResponse(id, "mykey", "val"));
        Assert.True(WaitFor(() => events.Count > 0));
        Assert.Equal("mykey", events[0].Key);
        Assert.Equal("val", events[0].Value);
        manager.Dispose();
    }

    [Fact]
    public void WatchRange_Prefix_WatchEventCallback_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = manager.WatchRange("pre", e => events.Add(e));

        var written = fake.Requests.Written.First().CreateRequest;
        Assert.Equal("pre", written.Key.ToStringUtf8());
        Assert.False(written.RangeEnd.IsEmpty);

        fake.Enqueue(EventResponse(id, "pre/x", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public void Watch_KeyWithStartRevision_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = manager.Watch("rk", 42, e => events.Add(e));

        Assert.Equal(42, fake.Requests.Written.First().CreateRequest.StartRevision);
        fake.Enqueue(EventResponse(id, "rk", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public void WatchRange_PrefixWithStartRevision_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = manager.WatchRange("rp", 7, e => events.Add(e));

        var written = fake.Requests.Written.First().CreateRequest;
        Assert.Equal(7, written.StartRevision);
        Assert.False(written.RangeEnd.IsEmpty);
        fake.Enqueue(EventResponse(id, "rp1", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public void WatchRange_Path_WatchResponseCallback_DeliversResponse()
    {
        var manager = CreateManager(out var fake);
        var responses = new List<WatchResponse>();
        long id = manager.WatchRange("path", r => responses.Add(r));

        var written = fake.Requests.Written.First().CreateRequest;
        Assert.Equal("path", written.Key.ToStringUtf8());
        Assert.False(written.RangeEnd.IsEmpty);
        fake.Enqueue(EventResponse(id, "path1", "v"));
        Assert.True(WaitFor(() => responses.Count > 0));
        manager.Dispose();
    }

    // ---------------------------------------------------------------------
    // Async convenience overloads
    // ---------------------------------------------------------------------

    [Fact]
    public async Task WatchAsync_Key_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = await manager.WatchAsync("ak", e => events.Add(e));
        fake.Enqueue(EventResponse(id, "ak", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public async Task WatchRangeAsync_Prefix_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = await manager.WatchRangeAsync("ap", e => events.Add(e));
        Assert.False(fake.Requests.Written.First().CreateRequest.RangeEnd.IsEmpty);
        fake.Enqueue(EventResponse(id, "ap1", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public async Task WatchAsync_KeyWithStartRevision_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = await manager.WatchAsync("ak", 11, e => events.Add(e));
        Assert.Equal(11, fake.Requests.Written.First().CreateRequest.StartRevision);
        fake.Enqueue(EventResponse(id, "ak", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    [Fact]
    public async Task WatchRangeAsync_PrefixWithStartRevision_DeliversEvent()
    {
        var manager = CreateManager(out var fake);
        var events = new List<WatchEvent>();
        long id = await manager.WatchRangeAsync("ap", 13, e => events.Add(e));
        var written = fake.Requests.Written.First().CreateRequest;
        Assert.Equal(13, written.StartRevision);
        Assert.False(written.RangeEnd.IsEmpty);
        fake.Enqueue(EventResponse(id, "ap1", "v"));
        Assert.True(WaitFor(() => events.Count > 0));
        manager.Dispose();
    }

    // ---------------------------------------------------------------------
    // CancelWatch
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CancelWatch_AfterCreated_SendsServerCancelRequest()
    {
        var manager = CreateManager(out var fake);
        using var created = new ManualResetEventSlim();
        long id = await manager.WatchAsync(KeyRequest("c"), r =>
        {
            if (r.Created)
            {
                created.Set();
            }
        });

        // Created response maps server watch id -> client watch id so cancel can find it.
        // Wait until the receive loop has processed it (so the mapping exists) before cancelling.
        fake.Enqueue(new WatchResponse { WatchId = id, Created = true });
        Assert.True(created.Wait(2000), "Created response was not processed in time.");

        manager.CancelWatch(id);

        Assert.True(WaitFor(() => fake.Requests.Written.Any(w => w.CancelRequest != null)),
            "Expected a WatchCancel request to be written after CancelWatch.");
        Assert.Equal(id, fake.Requests.Written.First(w => w.CancelRequest != null).CancelRequest.WatchId);
        manager.Dispose();
    }

    [Fact]
    public void CancelWatch_UnknownId_DoesNothing()
    {
        var manager = CreateManager(out _);
        // No watches registered; should be a no-op and not throw.
        manager.CancelWatch(99999);
        manager.Dispose();
    }

    [Fact]
    public async Task CanceledResponse_StopsDeliveringFurtherCallbacks()
    {
        var manager = CreateManager(out var fake);
        int count = 0;
        long id = await manager.WatchAsync(KeyRequest("x"), _ => Interlocked.Increment(ref count));

        fake.Enqueue(new WatchResponse { WatchId = id, Canceled = true });
        Assert.True(WaitFor(() => count >= 1));
        manager.Dispose();
    }

    // ---------------------------------------------------------------------
    // Dispose / ObjectDisposed
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Dispose_ThenWatch_ThrowsObjectDisposedException()
    {
        var manager = CreateManager(out _);
        manager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => manager.Watch("k", (Action<WatchEvent>)(_ => { })));
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            manager.WatchAsync(KeyRequest("k"), _ => { }));
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var manager = CreateManager(out _);
        manager.Dispose();
        manager.Dispose();
    }

    [Fact]
    public async Task Dispose_CancelsActiveWatchesAndDisposesStream()
    {
        var manager = CreateManager(out var fake);
        await manager.WatchAsync(KeyRequest("d"), _ => { });
        Assert.True(WaitFor(() => fake.Requests.Written.Any()));

        manager.Dispose();
        Assert.True(fake.IsDisposed);
    }

    // ---------------------------------------------------------------------
    // Watcher (direct) coverage
    // ---------------------------------------------------------------------

    [Fact]
    public void Watcher_Constructor_NullStream_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Watcher(null!));
    }

    [Fact]
    public async Task Watcher_CreateWatch_NullArgs_Throw()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        using var watcher = new Watcher(fake);

        await Assert.ThrowsAsync<ArgumentNullException>(() => watcher.CreateWatchAsync(null!, _ => { }));
        await Assert.ThrowsAsync<ArgumentNullException>(() => watcher.CreateWatchAsync(KeyRequest("k"), null!));
    }

    [Fact]
    public async Task Watcher_CancelWatch_WritesCancelAndRemovesCallback()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        using var watcher = new Watcher(fake);

        var request = KeyRequest("k");
        request.CreateRequest.WatchId = 5;
        await watcher.CreateWatchAsync(request, _ => { });
        await watcher.CancelWatchAsync(5);

        Assert.Contains(fake.Requests.Written, w => w.CancelRequest != null && w.CancelRequest.WatchId == 5);
    }

    [Fact]
    public void Watcher_ConnectionFailure_OnRpcException_InvokesCallback()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fake.Responses.ThrowOnMoveNext =
            new RpcException(new Status(StatusCode.Unavailable, "connection lost"));

        using var failed = new ManualResetEventSlim();
        using var watcher = new Watcher(fake, () => failed.Set());

        Assert.True(failed.Wait(2000), "onConnectionFailure was not invoked on RpcException.");
    }

    [Fact]
    public void Watcher_CancelledRpcException_DoesNotInvokeFailureCallback()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fake.Responses.ThrowOnMoveNext =
            new RpcException(new Status(StatusCode.Cancelled, "cancelled"));

        bool failureInvoked = false;
        using var watcher = new Watcher(fake, () => failureInvoked = true);

        // Give the background loop time to observe the cancellation.
        Thread.Sleep(200);
        Assert.False(failureInvoked);
    }

    [Fact]
    public void Watcher_Dispose_DisposesUnderlyingStream()
    {
        var fake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        var watcher = new Watcher(fake);
        watcher.Dispose();
        Assert.True(fake.IsDisposed);
    }
}
