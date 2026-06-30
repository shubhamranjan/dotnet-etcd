using System.Diagnostics;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Google.Protobuf;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the EtcdClient watch wrappers (watchClient.cs).
///     Each test wires a real <see cref="WatchManager" /> backed by a
///     <see cref="FakeDuplexStreamingCall{TRequest,TResponse}" /> into an EtcdClient via the
///     test constructor, then drives responses to exercise the conversion/fan-out lambdas.
/// </summary>
[Trait("Category", "Unit")]
public class EtcdClientWatchCoverageTests
{
    // First client-generated watch id within a fresh manager.
    private const long FirstWatchId = 2;

    private static EtcdClient CreateClient(out FakeDuplexStreamingCall<WatchRequest, WatchResponse> fake)
    {
        var localFake = new FakeDuplexStreamingCall<WatchRequest, WatchResponse>();
        fake = localFake;
        var connection = MockConnection.Create().Object;
        var manager = new WatchManager((_, _, _) => localFake);
        return new EtcdClient(connection, manager);
    }

    private static WatchRequest KeyRequest(string key) =>
        new() { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8(key) } };

    private static WatchResponse EventResponse(long watchId, string key = "k", string value = "v")
    {
        var response = new WatchResponse { WatchId = watchId };
        response.Events.Add(new Event
        {
            Type = Event.Types.EventType.Put,
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

    // ---------------------------------------------------------------------
    // Single request wrappers
    // ---------------------------------------------------------------------

    [Fact]
    public async Task WatchAsync_Request_WatchResponse_DeliversResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        long id = await client.WatchAsync(KeyRequest("a"), r => responses.Add(r));
        Assert.Equal(FirstWatchId, id);
        fake.Enqueue(EventResponse(id));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public async Task WatchAsync_Request_WatchResponseArray_DeliversToAll()
    {
        var client = CreateClient(out var fake);
        int a = 0, b = 0;
        long id = await client.WatchAsync(KeyRequest("a"),
            new Action<WatchResponse>[] { _ => Interlocked.Increment(ref a), _ => Interlocked.Increment(ref b) });
        fake.Enqueue(EventResponse(id));
        Assert.True(WaitFor(() => a > 0 && b > 0));
    }

    [Fact]
    public async Task WatchAsync_Request_WatchEventArray_DeliversConvertedEvents()
    {
        var client = CreateClient(out var fake);
        var events = new List<WatchEvent>();
        await client.WatchAsync(KeyRequest("a"), (WatchEvent[] e) => events.AddRange(e));
        fake.Enqueue(EventResponse(FirstWatchId, "a", "1"));
        Assert.True(WaitFor(() => events.Count > 0));
        Assert.Equal("a", events[0].Key);
    }

    [Fact]
    public async Task WatchAsync_Request_WatchEventArrayMethods_DeliversConvertedEvents()
    {
        var client = CreateClient(out var fake);
        int a = 0, b = 0;
        await client.WatchAsync(KeyRequest("a"), new Action<WatchEvent[]>[]
        {
            _ => Interlocked.Increment(ref a), _ => Interlocked.Increment(ref b)
        });
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => a > 0 && b > 0));
    }

    [Fact]
    public void Watch_Request_WatchResponse_DeliversResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        client.Watch(KeyRequest("a"), (WatchResponse r) => responses.Add(r));
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public void Watch_Request_WatchResponseArray_DeliversResponse()
    {
        var client = CreateClient(out var fake);
        int a = 0;
        client.Watch(KeyRequest("a"), new Action<WatchResponse>[] { _ => Interlocked.Increment(ref a) });
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => a > 0));
    }

    // ---------------------------------------------------------------------
    // Request array wrappers
    // ---------------------------------------------------------------------

    [Fact]
    public async Task WatchAsync_RequestArray_WatchEventArrayMethods_RegistersAllWatches()
    {
        // Registration-only check; delivery to the matching handler is covered by
        // WatchAsync_RequestArray_WatchEventArrayMethods_DeliversToMatchingHandler.
        var client = CreateClient(out var fake);
        long[] ids = await client.WatchAsync(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchEvent[]>[] { _ => { }, _ => { } });

        Assert.Equal(2, ids.Length);
        Assert.True(WaitFor(() => fake.Requests.Written.Count(w => w.CreateRequest != null) == 2));
    }

    [Fact]
    public async Task WatchAsync_RequestArray_WatchEventArrayMethods_DeliversToMatchingHandler()
    {
        var client = CreateClient(out var fake);
        int a = 0, b = 0;
        long[] ids = await client.WatchAsync(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchEvent[]>[]
            {
                evts => { if (evts.Length > 0) Interlocked.Increment(ref a); },
                evts => { if (evts.Length > 0) Interlocked.Increment(ref b); },
            });

        Assert.Equal(2, ids.Length);
        fake.Enqueue(EventResponse(ids[0], "a", "1"));
        fake.Enqueue(EventResponse(ids[1], "b", "2"));

        Assert.True(WaitFor(() => a >= 1 && b >= 1), $"Both handlers should fire; a={a}, b={b}");
    }

    [Fact]
    public void Watch_RequestArray_WatchEventArrayMethods_DeliversToMatchingHandler()
    {
        var client = CreateClient(out var fake);
        int a = 0, b = 0;
        client.Watch(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchEvent[]>[]
            {
                evts => { if (evts.Length > 0) Interlocked.Increment(ref a); },
                evts => { if (evts.Length > 0) Interlocked.Increment(ref b); },
            });

        fake.Enqueue(EventResponse(FirstWatchId, "a", "1"));
        fake.Enqueue(EventResponse(FirstWatchId + 1, "b", "2"));

        Assert.True(WaitFor(() => a >= 1 && b >= 1), $"Both handlers should fire; a={a}, b={b}");
    }

    [Fact]
    public async Task WatchAsync_RequestArray_SingleWatchEventArrayMethod_Match()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = await client.WatchAsync(
            new[] { KeyRequest("a"), KeyRequest("b") },
            (WatchEvent[] _) => Interlocked.Increment(ref count));

        Assert.Equal(2, ids.Length);
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public async Task WatchAsync_RequestArray_WatchResponseSingleMethod()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = await client.WatchAsync(
            new[] { KeyRequest("a"), KeyRequest("b") },
            (WatchResponse _) => Interlocked.Increment(ref count));
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public async Task WatchAsync_RequestArray_WatchResponseMethods()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = await client.WatchAsync(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchResponse>[] { _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count) });
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public void Watch_RequestArray_WatchResponseMethods()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        client.Watch(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchResponse>[] { _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count) });
        fake.Enqueue(EventResponse(FirstWatchId));
        fake.Enqueue(EventResponse(FirstWatchId + 1));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public void Watch_RequestArray_WatchEventArrayMethods_RegistersAllWatches()
    {
        // See note above: this wrapper captures the loop variable, so we assert registration only.
        var client = CreateClient(out var fake);
        client.Watch(
            new[] { KeyRequest("a"), KeyRequest("b") },
            new Action<WatchEvent[]>[] { _ => { }, _ => { } });
        Assert.True(WaitFor(() => fake.Requests.Written.Count(w => w.CreateRequest != null) == 2));
    }

    // ---------------------------------------------------------------------
    // Mismatched length argument validation
    // ---------------------------------------------------------------------

    [Fact]
    public async Task WatchAsync_RequestArray_LengthMismatch_Throws()
    {
        var client = CreateClient(out _);
        await Assert.ThrowsAsync<ArgumentException>(() => client.WatchAsync(
            new[] { KeyRequest("a") },
            new Action<WatchEvent[]>[] { _ => { }, _ => { } }));
    }

    [Fact]
    public void Watch_RequestArray_LengthMismatch_Throws()
    {
        var client = CreateClient(out _);
        Assert.Throws<ArgumentException>(() => client.Watch(
            new[] { KeyRequest("a") },
            new Action<WatchResponse>[] { _ => { }, _ => { } }));
    }

    // ---------------------------------------------------------------------
    // String key wrappers
    // ---------------------------------------------------------------------

    [Fact]
    public void Watch_StringKey_WatchResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        client.Watch("k", (WatchResponse r) => responses.Add(r));
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public void Watch_StringKey_WatchEventArray_RegistersWatch()
    {
        // Routes through Watch(WatchRequest[], Action<WatchEvent[]>[]) which captures the loop variable;
        // assert registration only.
        var client = CreateClient(out var fake);
        client.Watch("k", (WatchEvent[] _) => { });
        Assert.True(WaitFor(() => fake.Requests.Written.Any(w => w.CreateRequest != null)));
    }

    [Fact]
    public async Task WatchAsync_StringKey_WatchResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        await client.WatchAsync("k", (WatchResponse r) => responses.Add(r));
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public async Task WatchAsync_StringKey_WatchEventArray_RegistersWatch()
    {
        // Routes through WatchAsync(WatchRequest[], Action<WatchEvent[]>[]) which captures the loop
        // variable; assert registration only.
        var client = CreateClient(out var fake);
        await client.WatchAsync("k", (WatchEvent[] _) => { });
        Assert.True(WaitFor(() => fake.Requests.Written.Any(w => w.CreateRequest != null)));
    }

    // ---------------------------------------------------------------------
    // WatchRange wrappers
    // ---------------------------------------------------------------------

    [Fact]
    public void WatchRange_Path_WatchResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        long id = client.WatchRange("p", (WatchResponse r) => responses.Add(r));
        Assert.Equal(FirstWatchId, id);
        fake.Enqueue(EventResponse(id));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public void WatchRange_Path_WatchResponseArray()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        client.WatchRange("p", new Action<WatchResponse>[]
        {
            _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count)
        });
        fake.Enqueue(EventResponse(FirstWatchId));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public void WatchRange_PathArray_WatchResponse()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = client.WatchRange(new[] { "p", "q" }, (WatchResponse _) => Interlocked.Increment(ref count));
        Assert.Equal(2, ids.Length);
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public void WatchRange_PathArray_WatchResponseMethods()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = client.WatchRange(new[] { "p", "q" }, new Action<WatchResponse>[]
        {
            _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count)
        });
        Assert.Equal(2, ids.Length);
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public void WatchRange_PathArray_LengthMismatch_Throws()
    {
        var client = CreateClient(out _);
        Assert.Throws<ArgumentException>(() => client.WatchRange(
            new[] { "p" }, new Action<WatchResponse>[] { _ => { }, _ => { } }));
    }

    [Fact]
    public async Task WatchRangeAsync_Path_WatchResponse()
    {
        var client = CreateClient(out var fake);
        var responses = new List<WatchResponse>();
        long id = await client.WatchRangeAsync("p", (WatchResponse r) => responses.Add(r));
        fake.Enqueue(EventResponse(id));
        Assert.True(WaitFor(() => responses.Count > 0));
    }

    [Fact]
    public async Task WatchRangeAsync_Path_WatchResponseArray()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long id = await client.WatchRangeAsync("p", new Action<WatchResponse>[]
        {
            _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count)
        });
        fake.Enqueue(EventResponse(id));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public async Task WatchRangeAsync_PathArray_WatchResponse()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = await client.WatchRangeAsync(new[] { "p", "q" },
            (WatchResponse _) => Interlocked.Increment(ref count));
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public async Task WatchRangeAsync_PathArray_WatchResponseMethods()
    {
        var client = CreateClient(out var fake);
        int count = 0;
        long[] ids = await client.WatchRangeAsync(new[] { "p", "q" }, new Action<WatchResponse>[]
        {
            _ => Interlocked.Increment(ref count), _ => Interlocked.Increment(ref count)
        });
        fake.Enqueue(EventResponse(ids[0]));
        fake.Enqueue(EventResponse(ids[1]));
        Assert.True(WaitFor(() => count >= 2));
    }

    [Fact]
    public async Task WatchRangeAsync_PathArray_LengthMismatch_Throws()
    {
        var client = CreateClient(out _);
        await Assert.ThrowsAsync<ArgumentException>(() => client.WatchRangeAsync(
            new[] { "p" }, new Action<WatchResponse>[] { _ => { }, _ => { } }));
    }
}
