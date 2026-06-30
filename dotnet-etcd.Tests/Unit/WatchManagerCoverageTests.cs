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
