using System.Text;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class WatchRangeIntegrationTests
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;
    private readonly string _testPrefix;

    public WatchRangeIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });
        _testPrefix = $"test-range-{Guid.NewGuid()}/";
    }

    [Fact]
    public async Task WatchRange_ShouldReceiveEvents_ForAllKeysInRange()
    {
        // Arrange
        var key1 = $"{_testPrefix}key1";
        var key2 = $"{_testPrefix}key2";
        var key3 = $"{_testPrefix}key3";

        var receivedEvents = new List<Event>();
        var eventCountReached = new ManualResetEventSlim(false);

        // Start watching the range
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(_testPrefix),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(_testPrefix))
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events) receivedEvents.Add(evt);

                if (receivedEvents.Count >= 3) // We expect 3 PUT events
                    eventCountReached.Set();
            }));

        // Act - add keys to the range
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key1),
            Value = ByteString.CopyFromUtf8("value1")
        });

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key2),
            Value = ByteString.CopyFromUtf8("value2")
        });

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key3),
            Value = ByteString.CopyFromUtf8("value3")
        });

        // Assert
        var allEventsReceived = eventCountReached.Wait(TimeSpan.FromSeconds(5));

        // Clean up
        _client.CancelWatch(watchId);

        // Verify results
        Assert.True(allEventsReceived, "Not all watch events were received within timeout");
        Assert.Equal(3, receivedEvents.Count);

        // Verify we received events for all three keys
        var receivedKeys = new HashSet<string>();
        foreach (var evt in receivedEvents) receivedKeys.Add(evt.Kv.Key.ToStringUtf8());

        Assert.Contains(key1, receivedKeys);
        Assert.Contains(key2, receivedKeys);
        Assert.Contains(key3, receivedKeys);
    }

    [Fact]
    public async Task WatchRange_ShouldNotReceiveEvents_ForKeysOutsideRange()
    {
        // Arrange
        var keyInRange = $"{_testPrefix}in-range";
        var keyOutsideRange = $"outside-{_testPrefix}";

        var receivedEvents = new List<Event>();
        var eventReceived = new ManualResetEventSlim(false);

        // Start watching the range
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(_testPrefix),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(_testPrefix))
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events)
                {
                    receivedEvents.Add(evt);
                    eventReceived.Set();
                }
            }));

        // Act - add keys both inside and outside the range
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(keyOutsideRange),
            Value = ByteString.CopyFromUtf8("outside-value")
        });

        // Wait a bit to see if we get any events for the outside key
        await Task.Delay(1000);

        // Now add a key inside the range
        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(keyInRange),
            Value = ByteString.CopyFromUtf8("in-range-value")
        });

        // Assert
        var eventWasReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));

        // Clean up
        _client.CancelWatch(watchId);

        // Verify results
        Assert.True(eventWasReceived, "No watch events were received within timeout");
        Assert.Single(receivedEvents);
        Assert.Equal(keyInRange, receivedEvents[0].Kv.Key.ToStringUtf8());
        Assert.DoesNotContain(receivedEvents, e => e.Kv.Key.ToStringUtf8() == keyOutsideRange);
    }

    [Fact]
    public async Task WatchRange_WithPrefixEnd_ShouldWatchAllKeysWithPrefix()
    {
        // Arrange
        var prefix = $"prefix-{Guid.NewGuid()}/";
        var key1 = $"{prefix}key1";
        var key2 = $"{prefix}key2";
        var keyOutsidePrefix = $"outside-{prefix}key";

        var receivedEvents = new List<Event>();
        var eventCountReached = new ManualResetEventSlim(false);

        // Start watching with prefix
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(prefix),
                    RangeEnd = ByteString.CopyFromUtf8(GetRangeEnd(prefix))
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events) receivedEvents.Add(evt);

                if (receivedEvents.Count >= 2) // We expect 2 events for keys with the prefix
                    eventCountReached.Set();
            }));

        // Act - add keys
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key1),
            Value = ByteString.CopyFromUtf8("value1")
        });

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key2),
            Value = ByteString.CopyFromUtf8("value2")
        });

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(keyOutsidePrefix),
            Value = ByteString.CopyFromUtf8("outside-value")
        });

        // Assert
        var allEventsReceived = eventCountReached.Wait(TimeSpan.FromSeconds(5));

        // Clean up
        _client.CancelWatch(watchId);

        // Verify results
        Assert.True(allEventsReceived, "Not all watch events were received within timeout");
        Assert.Equal(2, receivedEvents.Count);

        // Verify we received events for the correct keys
        var receivedKeys = new HashSet<string>();
        foreach (var evt in receivedEvents) receivedKeys.Add(evt.Kv.Key.ToStringUtf8());

        Assert.Contains(key1, receivedKeys);
        Assert.Contains(key2, receivedKeys);
        Assert.DoesNotContain(keyOutsidePrefix, receivedKeys);
    }

    // Helper method to get the range end for a prefix
    private string GetRangeEnd(string prefix)
    {
        var prefixBytes = Encoding.UTF8.GetBytes(prefix);
        for (var i = prefixBytes.Length - 1; i >= 0; i--)
            if (prefixBytes[i] < 0xFF)
            {
                prefixBytes[i]++;
                return Encoding.UTF8.GetString(prefixBytes, 0, i + 1);
            }

        // If all bytes are 0xFF, return an empty string
        return string.Empty;
    }
}