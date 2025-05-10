using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Etcdserverpb;
using Google.Protobuf;
using Mvccpb;
using Xunit;

namespace dotnet_etcd.Tests.Integration;

[Trait("Category", "Integration")]
public class WatchClientIntegrationTests : IDisposable
{
    private readonly EtcdClient _client;
    private readonly string _testKeyPrefix = "watch-test-";
    private readonly List<string> _keysToCleanup = new();

    public WatchClientIntegrationTests()
    {
        _client = new EtcdClient("127.0.0.1:2379");
    }

    public void Dispose()
    {
        // Clean up all test keys
        foreach (var key in _keysToCleanup) _client.DeleteAsync(key).Wait();
        _client.Dispose();

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WatchSingleKey_ShouldReceiveEvents()
    {
        // Arrange
        var testKey = $"{_testKeyPrefix}single-key";
        _keysToCleanup.Add(testKey);
        var events = new List<WatchEvent>();

        // Act - Start watching
        _client.Watch(testKey, (response) =>
        {
            events.AddRange(response.Events.Select(evt => new WatchEvent
            {
                Key = evt.Kv.Key.ToStringUtf8(),
                Value = evt.Kv.Value.ToStringUtf8(),
                Type = evt.Type
            }));
        });

        // Wait a bit to ensure watch is established
        await Task.Delay(1000);

        // Perform operations
        await _client.PutAsync(testKey, "value1");
        await Task.Delay(100); // Small delay to ensure event processing
        await _client.PutAsync(testKey, "value2");
        await Task.Delay(100);
        await _client.DeleteAsync(testKey);

        // Wait a bit for events to be processed
        await Task.Delay(1000);

        // Assert
        Assert.Equal(3, events.Count);
        Assert.Equal(Event.Types.EventType.Put, events[0].Type);
        Assert.Equal("value1", events[0].Value);
        Assert.Equal(Event.Types.EventType.Put, events[1].Type);
        Assert.Equal("value2", events[1].Value);
        Assert.Equal(Event.Types.EventType.Delete, events[2].Type);
    }

    [Fact]
    public async Task WatchMultipleKeys_ShouldReceiveEvents()
    {
        // Arrange
        var testKey1 = $"{_testKeyPrefix}multi-key-1";
        var testKey2 = $"{_testKeyPrefix}multi-key-2";
        _keysToCleanup.Add(testKey1);
        _keysToCleanup.Add(testKey2);

        var events1 = new List<WatchEvent>();
        var events2 = new List<WatchEvent>();

        // Act - Start watching both keys
        _client.Watch(testKey1, (response) =>
        {
            foreach (var evt in response.Events)
                events1.Add(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(),
                    Value = evt.Kv.Value.ToStringUtf8(),
                    Type = evt.Type
                });
        });

        _client.Watch(testKey2, (response) =>
        {
            foreach (var evt in response.Events)
                events2.Add(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(),
                    Value = evt.Kv.Value.ToStringUtf8(),
                    Type = evt.Type
                });
        });

        // Wait a bit to ensure watches are established
        await Task.Delay(1000);

        // Perform operations
        await _client.PutAsync(testKey1, "value1");
        await Task.Delay(100);
        await _client.PutAsync(testKey2, "value2");
        await Task.Delay(100);
        await _client.DeleteAsync(testKey1);
        await Task.Delay(100);
        await _client.DeleteAsync(testKey2);

        // Wait a bit for events to be processed
        await Task.Delay(1000);

        // Assert
        Assert.Equal(2, events1.Count);
        Assert.Equal(Event.Types.EventType.Put, events1[0].Type);
        Assert.Equal("value1", events1[0].Value);
        Assert.Equal(Event.Types.EventType.Delete, events1[1].Type);

        Assert.Equal(2, events2.Count);
        Assert.Equal(Event.Types.EventType.Put, events2[0].Type);
        Assert.Equal("value2", events2[0].Value);
        Assert.Equal(Event.Types.EventType.Delete, events2[1].Type);
    }

    [Fact]
    public async Task WatchKeyRange_ShouldReceiveEvents()
    {
        // Arrange
        var prefix = $"{_testKeyPrefix}range-";
        var testKey1 = $"{prefix}key1";
        var testKey2 = $"{prefix}key2";
        _keysToCleanup.Add(testKey1);
        _keysToCleanup.Add(testKey2);

        var events = new List<WatchEvent>();

        // Act - Start watching the range
        _client.WatchRange(prefix, (response) =>
        {
            foreach (var evt in response.Events)
                events.Add(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(),
                    Value = evt.Kv.Value.ToStringUtf8(),
                    Type = evt.Type
                });
        });

        // Wait a bit to ensure watch is established
        await Task.Delay(1000);

        // Perform operations
        await _client.PutAsync(testKey1, "value1");
        await Task.Delay(100);
        await _client.PutAsync(testKey2, "value2");
        await Task.Delay(100);
        await _client.DeleteAsync(testKey1);
        await Task.Delay(100);
        await _client.DeleteAsync(testKey2);

        // Wait a bit for events to be processed
        await Task.Delay(1000);

        // Assert
        Assert.Equal(4, events.Count);
        Assert.Equal(Event.Types.EventType.Put, events[0].Type);
        Assert.Equal("value1", events[0].Value);
        Assert.Equal(Event.Types.EventType.Put, events[1].Type);
        Assert.Equal("value2", events[1].Value);
        Assert.Equal(Event.Types.EventType.Delete, events[2].Type);
        Assert.Equal(Event.Types.EventType.Delete, events[3].Type);
    }

    [Fact]
    public async Task WatchWithCancellation_ShouldStopReceivingEvents()
    {
        // Arrange
        var testKey = $"{_testKeyPrefix}cancel-key";
        _keysToCleanup.Add(testKey);
        var events = new List<WatchEvent>();
        var cts = new CancellationTokenSource();

        // Act - Start watching with cancellation token
        _client.Watch(testKey, (response) =>
        {
            foreach (var evt in response.Events)
                events.Add(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(),
                    Value = evt.Kv.Value.ToStringUtf8(),
                    Type = evt.Type
                });
        }, cancellationToken: cts.Token);

        // Wait a bit to ensure watch is established
        await Task.Delay(1000);

        // Perform first operation
        await _client.PutAsync(testKey, "value1");
        await Task.Delay(100);

        // Cancel the watch
        cts.Cancel();
        await Task.Delay(1000);

        // Perform second operation (should not be received)
        await _client.PutAsync(testKey, "value2");
        await Task.Delay(100);

        // Assert
        Assert.Single(events);
        Assert.Equal(Event.Types.EventType.Put, events[0].Type);
        Assert.Equal("value1", events[0].Value);
    }

    [Fact]
    public async Task WatchWithDeadline_ShouldStopAfterDeadline()
    {
        // Arrange
        var testKey = $"{_testKeyPrefix}deadline-key";
        _keysToCleanup.Add(testKey);
        var events = new List<WatchEvent>();

        // Act - Start watching with a short deadline
        _client.Watch(testKey, (response) =>
        {
            foreach (var evt in response.Events)
                events.Add(new WatchEvent
                {
                    Key = evt.Kv.Key.ToStringUtf8(),
                    Value = evt.Kv.Value.ToStringUtf8(),
                    Type = evt.Type
                });
        }, deadline: DateTime.UtcNow.AddSeconds(2));

        // Wait a bit to ensure watch is established
        await Task.Delay(1000);

        // Perform first operation
        await _client.PutAsync(testKey, "value1");
        await Task.Delay(100);

        // Wait for deadline to pass
        await Task.Delay(2000);

        // Perform second operation (should not be received)
        await _client.PutAsync(testKey, "value2");
        await Task.Delay(100);

        // Assert
        Assert.Single(events);
        Assert.Equal(Event.Types.EventType.Put, events[0].Type);
        Assert.Equal("value1", events[0].Value);
    }
}