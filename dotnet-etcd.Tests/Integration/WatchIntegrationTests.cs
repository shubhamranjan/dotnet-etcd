using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class WatchIntegrationTests
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;

    public WatchIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });
    }

    [Fact]
    public async Task Watch_ShouldReceiveEvents_WhenKeyIsModified()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var initialValue = "initial-value";
        var updatedValue = "updated-value";

        var receivedEvents = new List<Event>();
        var eventReceived = new ManualResetEventSlim(false);

        // Put initial value
        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8(initialValue)
        });

        // Start watching the key
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey)
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events) receivedEvents.Add(evt);

                if (receivedEvents.Count > 0) eventReceived.Set();
            }));

        // Act - update the key
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8(updatedValue)
        });

        // Assert
        var eventWasReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));

        // Clean up
        _client.CancelWatch(watchId);

        // Verify results
        Assert.True(eventWasReceived, "Watch event was not received within timeout");
        Assert.NotEmpty(receivedEvents);
        Assert.Equal(Event.Types.EventType.Put, receivedEvents[0].Type);
        Assert.Equal(testKey, receivedEvents[0].Kv.Key.ToStringUtf8());
        Assert.Equal(updatedValue, receivedEvents[0].Kv.Value.ToStringUtf8());
    }

    [Fact]
    public async Task Watch_ShouldReceiveEvents_WhenKeyIsDeleted()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var initialValue = "delete-test-value";

        var receivedEvents = new List<Event>();
        var eventReceived = new ManualResetEventSlim(false);

        // Put initial value
        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8(initialValue)
        });

        // Start watching the key
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey)
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events) receivedEvents.Add(evt);

                if (receivedEvents.Count > 0) eventReceived.Set();
            }));

        // Act - delete the key
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.DeleteAsync(new DeleteRangeRequest
        {
            Key = ByteString.CopyFromUtf8(testKey)
        });

        // Assert
        var eventWasReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));

        // Clean up
        _client.CancelWatch(watchId);

        // Verify results
        Assert.True(eventWasReceived, "Watch event was not received within timeout");
        Assert.NotEmpty(receivedEvents);
        Assert.Equal(Event.Types.EventType.Delete, receivedEvents[0].Type);
        Assert.Equal(testKey, receivedEvents[0].Kv.Key.ToStringUtf8());
    }

    [Fact]
    public async Task CancelWatch_ShouldStopReceivingEvents()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var initialValue = "cancel-test-value";
        var updatedValue = "updated-after-cancel";

        var receivedEvents = new List<Event>();
        var eventReceived = new ManualResetEventSlim(false);

        // Put initial value
        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8(initialValue)
        });

        // Start watching the key
        var watchId = await _client.WatchAsync(
            new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey)
                }
            },
            (Action<WatchResponse>)(response =>
            {
                foreach (var evt in response.Events) receivedEvents.Add(evt);

                eventReceived.Set();
            }));

        // Act - update the key to verify watch is working
        await Task.Delay(500); // Give some time for the watch to be established

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8("first-update")
        });

        // Wait for the event to be received
        var firstEventReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));

        // Cancel the watch
        _client.CancelWatch(watchId);

        // Reset for second update
        eventReceived.Reset();
        var eventCountBeforeSecondUpdate = receivedEvents.Count;

        // Update the key again after cancellation
        await Task.Delay(500); // Give some time for the cancellation to take effect

        await _client.PutAsync(new PutRequest
        {
            Key = ByteString.CopyFromUtf8(testKey),
            Value = ByteString.CopyFromUtf8(updatedValue)
        });

        // Wait to see if we receive any more events
        var moreEventsReceived = eventReceived.Wait(TimeSpan.FromSeconds(2));

        // Verify results
        Assert.True(firstEventReceived, "First watch event was not received within timeout");
        Assert.False(moreEventsReceived, "Events were received after watch was cancelled");
        Assert.Equal(eventCountBeforeSecondUpdate, receivedEvents.Count);
        Assert.True(eventCountBeforeSecondUpdate == receivedEvents.Count,
            "Event count should not have changed after cancellation");
    }
}