using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Mvccpb;
using Xunit;

namespace dotnet_etcd.Tests.Integration
{
    [Collection("EtcdCluster")]
    [Trait("Category", "Integration")]
    public class WatchIntegrationTests
    {
        private readonly EtcdClusterFixture _fixture;
        private readonly EtcdClient _client;

        public WatchIntegrationTests(EtcdClusterFixture fixture)
        {
            _fixture = fixture;
            Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");
            
            _client = new EtcdClient(_fixture.ConnectionString, configureChannelOptions: options => 
            {
                options.Credentials = ChannelCredentials.Insecure;
            });
        }

        [Fact]
        public async Task Watch_ShouldReceiveEvents_WhenKeyIsModified()
        {
            // Arrange
            string testKey = $"test-key-{Guid.NewGuid()}";
            string initialValue = "initial-value";
            string updatedValue = "updated-value";
            
            var receivedEvents = new List<Mvccpb.Event>();
            var eventReceived = new ManualResetEventSlim(false);
            
            // Put initial value
            await _client.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(initialValue)
            });
            
            // Start watching the key
            long watchId = await _client.WatchAsync(
                new WatchRequest
                {
                    CreateRequest = new WatchCreateRequest
                    {
                        Key = ByteString.CopyFromUtf8(testKey)
                    }
                },
                (Action<WatchResponse>)(response =>
                {
                    foreach (var evt in response.Events)
                    {
                        receivedEvents.Add(evt);
                    }
                    
                    if (receivedEvents.Count > 0)
                    {
                        eventReceived.Set();
                    }
                }));
            
            // Act - update the key
            await Task.Delay(500); // Give some time for the watch to be established
            
            await _client.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(updatedValue)
            });
            
            // Assert
            bool eventWasReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));
            
            // Clean up
            _client.CancelWatch(watchId);
            
            // Verify results
            Assert.True(eventWasReceived, "Watch event was not received within timeout");
            Assert.NotEmpty(receivedEvents);
            Assert.Equal(Mvccpb.Event.Types.EventType.Put, receivedEvents[0].Type);
            Assert.Equal(testKey, receivedEvents[0].Kv.Key.ToStringUtf8());
            Assert.Equal(updatedValue, receivedEvents[0].Kv.Value.ToStringUtf8());
        }

        [Fact]
        public async Task Watch_ShouldReceiveEvents_WhenKeyIsDeleted()
        {
            // Arrange
            string testKey = $"test-key-{Guid.NewGuid()}";
            string initialValue = "delete-test-value";
            
            var receivedEvents = new List<Mvccpb.Event>();
            var eventReceived = new ManualResetEventSlim(false);
            
            // Put initial value
            await _client.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(initialValue)
            });
            
            // Start watching the key
            long watchId = await _client.WatchAsync(
                new WatchRequest
                {
                    CreateRequest = new WatchCreateRequest
                    {
                        Key = ByteString.CopyFromUtf8(testKey)
                    }
                },
                (Action<WatchResponse>)(response =>
                {
                    foreach (var evt in response.Events)
                    {
                        receivedEvents.Add(evt);
                    }
                    
                    if (receivedEvents.Count > 0)
                    {
                        eventReceived.Set();
                    }
                }));
            
            // Act - delete the key
            await Task.Delay(500); // Give some time for the watch to be established
            
            await _client.DeleteAsync(new DeleteRangeRequest
            {
                Key = ByteString.CopyFromUtf8(testKey)
            });
            
            // Assert
            bool eventWasReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));
            
            // Clean up
            _client.CancelWatch(watchId);
            
            // Verify results
            Assert.True(eventWasReceived, "Watch event was not received within timeout");
            Assert.NotEmpty(receivedEvents);
            Assert.Equal(Mvccpb.Event.Types.EventType.Delete, receivedEvents[0].Type);
            Assert.Equal(testKey, receivedEvents[0].Kv.Key.ToStringUtf8());
        }

        [Fact]
        public async Task CancelWatch_ShouldStopReceivingEvents()
        {
            // Arrange
            string testKey = $"test-key-{Guid.NewGuid()}";
            string initialValue = "cancel-test-value";
            string updatedValue = "updated-after-cancel";
            
            var receivedEvents = new List<Mvccpb.Event>();
            var eventReceived = new ManualResetEventSlim(false);
            
            // Put initial value
            await _client.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(initialValue)
            });
            
            // Start watching the key
            long watchId = await _client.WatchAsync(
                new WatchRequest
                {
                    CreateRequest = new WatchCreateRequest
                    {
                        Key = ByteString.CopyFromUtf8(testKey)
                    }
                },
                (Action<WatchResponse>)(response =>
                {
                    foreach (var evt in response.Events)
                    {
                        receivedEvents.Add(evt);
                    }
                    
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
            bool firstEventReceived = eventReceived.Wait(TimeSpan.FromSeconds(5));
            
            // Cancel the watch
            _client.CancelWatch(watchId);
            
            // Reset for second update
            eventReceived.Reset();
            int eventCountBeforeSecondUpdate = receivedEvents.Count;
            
            // Update the key again after cancellation
            await Task.Delay(500); // Give some time for the cancellation to take effect
            
            await _client.PutAsync(new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(updatedValue)
            });
            
            // Wait to see if we receive any more events
            bool moreEventsReceived = eventReceived.Wait(TimeSpan.FromSeconds(2));
            
            // Verify results
            Assert.True(firstEventReceived, "First watch event was not received within timeout");
            Assert.False(moreEventsReceived, "Events were received after watch was cancelled");
            Assert.Equal(eventCountBeforeSecondUpdate, receivedEvents.Count);
            Assert.True(eventCountBeforeSecondUpdate == receivedEvents.Count, "Event count should not have changed after cancellation");
        }
    }
} 