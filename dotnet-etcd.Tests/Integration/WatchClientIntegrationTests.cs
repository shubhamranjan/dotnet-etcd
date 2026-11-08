using System.Collections.Concurrent;
using System.Diagnostics;
using Etcdserverpb;
using Mvccpb;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
///     End-to-end watch behaviour against a live etcd. Every wait here is for an observable
///     condition (an event arriving on a watch), never a fixed sleep: Watch() only returns once
///     etcd has acknowledged the watch, and a second "control" watch on the same key proves that a
///     write has propagated when asserting that a cancelled or expired watch did NOT see it.
/// </summary>
[Trait("Category", "Integration")]
public class WatchClientIntegrationTests : IDisposable
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private readonly EtcdClient _client = new("127.0.0.1:2379");
    private readonly string _testKeyPrefix = $"watch-test-{Guid.NewGuid()}-";
    private readonly List<string> _keysToCleanup = new();

    public void Dispose()
    {
        foreach (var key in _keysToCleanup) _client.DeleteAsync(key).Wait();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private string Key(string suffix)
    {
        var key = _testKeyPrefix + suffix;
        _keysToCleanup.Add(key);
        return key;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, Func<string> describe)
    {
        var sw = Stopwatch.StartNew();
        while (!condition())
        {
            if (sw.Elapsed > WaitTimeout) throw new TimeoutException($"timed out waiting: {describe()}");
            await Task.Delay(10);
        }
    }

    /// <summary>Thread-safe event sink; watch callbacks run on the client's dispatch thread.</summary>
    private sealed class Recorder
    {
        private readonly ConcurrentQueue<WatchEvent> _events = new();

        public int Count => _events.Count;
        public WatchEvent[] Events => _events.ToArray();

        public void Record(WatchResponse response)
        {
            foreach (var evt in response.Events)
                _events.Enqueue(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(), Value = evt.Kv.Value.ToStringUtf8(), Type = evt.Type
                });
        }

        public Task WaitForAsync(int count) =>
            WaitUntilAsync(() => Count >= count, () => $"expected {count} events, have {Count}");
    }

    [Fact]
    public async Task WatchSingleKey_ShouldReceiveEvents()
    {
        var key = Key("single-key");
        var recorder = new Recorder();
        _client.Watch(key, recorder.Record);

        await _client.PutAsync(key, "value1");
        await _client.PutAsync(key, "value2");
        await _client.DeleteAsync(key);
        await recorder.WaitForAsync(3);

        var events = recorder.Events;
        Assert.Equal(3, events.Length);
        Assert.Equal((Event.Types.EventType.Put, "value1"), (events[0].Type, events[0].Value));
        Assert.Equal((Event.Types.EventType.Put, "value2"), (events[1].Type, events[1].Value));
        Assert.Equal(Event.Types.EventType.Delete, events[2].Type);
    }

    [Fact]
    public async Task WatchMultipleKeys_ShouldReceiveEvents()
    {
        var key1 = Key("multi-key-1");
        var key2 = Key("multi-key-2");
        var recorder1 = new Recorder();
        var recorder2 = new Recorder();
        _client.Watch(key1, recorder1.Record);
        _client.Watch(key2, recorder2.Record);

        await _client.PutAsync(key1, "value1");
        await _client.PutAsync(key2, "value2");
        await _client.DeleteAsync(key1);
        await _client.DeleteAsync(key2);
        await recorder1.WaitForAsync(2);
        await recorder2.WaitForAsync(2);

        Assert.Equal(new[] { Event.Types.EventType.Put, Event.Types.EventType.Delete },
            recorder1.Events.Select(e => e.Type));
        Assert.Equal("value1", recorder1.Events[0].Value);
        Assert.Equal(new[] { Event.Types.EventType.Put, Event.Types.EventType.Delete },
            recorder2.Events.Select(e => e.Type));
        Assert.Equal("value2", recorder2.Events[0].Value);
    }

    [Fact]
    public async Task WatchKeyRange_ShouldReceiveEvents()
    {
        var prefix = _testKeyPrefix + "range-";
        var key1 = Key("range-key1");
        var key2 = Key("range-key2");
        var recorder = new Recorder();
        _client.WatchRange(prefix, recorder.Record);

        await _client.PutAsync(key1, "value1");
        await _client.PutAsync(key2, "value2");
        await _client.DeleteAsync(key1);
        await _client.DeleteAsync(key2);
        await recorder.WaitForAsync(4);

        var events = recorder.Events;
        Assert.Equal(4, events.Length);
        Assert.Equal((Event.Types.EventType.Put, "value1"), (events[0].Type, events[0].Value));
        Assert.Equal((Event.Types.EventType.Put, "value2"), (events[1].Type, events[1].Value));
        Assert.Equal(Event.Types.EventType.Delete, events[2].Type);
        Assert.Equal(Event.Types.EventType.Delete, events[3].Type);
    }

    [Fact]
    public async Task WatchWithCancellation_ShouldStopReceivingEvents()
    {
        var key = Key("cancel-key");
        var cancelled = new Recorder();
        var control = new Recorder();
        using var cts = new CancellationTokenSource();
        _client.Watch(key, cancelled.Record, cancellationToken: cts.Token);
        _client.Watch(key, control.Record);

        await _client.PutAsync(key, "value1");
        await cancelled.WaitForAsync(1);
        await control.WaitForAsync(1);

        cts.Cancel();

        // The control watch seeing value2 proves the write propagated; the cancelled one must not.
        await _client.PutAsync(key, "value2");
        await control.WaitForAsync(2);

        Assert.Equal(new[] { "value1" }, cancelled.Events.Select(e => e.Value));
        Assert.Equal(new[] { "value1", "value2" }, control.Events.Select(e => e.Value));
    }

    [Fact]
    public async Task WatchWithDeadline_ShouldStopAfterDeadline_WithoutAffectingOtherWatches()
    {
        var key = Key("deadline-key");
        var expiring = new Recorder();
        var lasting = new Recorder();
        var deadline = DateTime.UtcNow.AddSeconds(1);
        _client.Watch(key, expiring.Record, deadline: deadline);
        _client.Watch(key, lasting.Record);

        await _client.PutAsync(key, "value1");
        await expiring.WaitForAsync(1);
        await lasting.WaitForAsync(1);

        // The deadline itself is the only thing we have to wait for here.
        var untilDeadline = deadline.AddMilliseconds(250) - DateTime.UtcNow;
        if (untilDeadline > TimeSpan.Zero) await Task.Delay(untilDeadline);

        await _client.PutAsync(key, "value2");
        await lasting.WaitForAsync(2);

        Assert.Equal(new[] { "value1" }, expiring.Events.Select(e => e.Value));
        Assert.Equal(new[] { "value1", "value2" }, lasting.Events.Select(e => e.Value));
    }
}
