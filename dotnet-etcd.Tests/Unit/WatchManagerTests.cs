using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using Xunit;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class WatchManagerTests
    {
        [Fact]
        public async Task WatchAsync_ShouldReturnWatchId()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            // Setup the response stream to return a response with a watch ID
            var watchResponse = new WatchResponse
            {
                Created = true,
                WatchId = 123
            };

            var responseSequence = new Queue<WatchResponse>();
            responseSequence.Enqueue(watchResponse);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => responseSequence.Count > 0)
                .Callback(() => 
                {
                    if (responseSequence.Count > 0)
                    {
                        var response = responseSequence.Dequeue();
                        mockResponseStream.Setup(x => x.Current).Returns(response);
                    }
                });

            // Create a factory that returns our mock
            Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory = 
                (headers, deadline, token) => mockStreamingCall.Object;

            var watchManager = new WatchManager(factory);

            // Act
            var watchId = await watchManager.WatchAsync(
                new WatchRequest { CreateRequest = new WatchCreateRequest { Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key") } },
                response => { });

            // Assert
            Assert.NotEqual(0, watchId);
            mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r => 
                r.CreateRequest.Key.ToStringUtf8() == "test-key")), Times.Once);
        }

        [Fact]
        public void CancelWatch_ShouldCancelWatchAndRemoveFromDictionary()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            // Setup the response stream to return a response with a watch ID
            var watchResponse = new WatchResponse
            {
                Created = true,
                WatchId = 123
            };

            var responseSequence = new Queue<WatchResponse>();
            responseSequence.Enqueue(watchResponse);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => responseSequence.Count > 0)
                .Callback(() => 
                {
                    if (responseSequence.Count > 0)
                    {
                        var response = responseSequence.Dequeue();
                        mockResponseStream.Setup(x => x.Current).Returns(response);
                    }
                });

            // Create a factory that returns our mock
            Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory = 
                (headers, deadline, token) => mockStreamingCall.Object;

            var watchManager = new WatchManager(factory);

            // Create a watch first
            var watchId = watchManager.Watch(
                new WatchRequest { CreateRequest = new WatchCreateRequest { Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key") } },
                response => { });

            // Set up the watch ID mapping using reflection
            var watchIdMappingField = typeof(WatchManager).GetField("_watchIdMapping", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var watchIdMapping = (System.Collections.Concurrent.ConcurrentDictionary<long, long>)watchIdMappingField.GetValue(watchManager);
            watchIdMapping[123] = watchId;

            // Act
            watchManager.CancelWatch(watchId);

            // Assert
            mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r => 
                r.CancelRequest != null && r.CancelRequest.WatchId == 123)), Times.Once);
            
            // Verify the watch was removed from the dictionary
            var watchesField = typeof(WatchManager).GetField("_watches", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var watches = (System.Collections.Concurrent.ConcurrentDictionary<long, WatchManager.WatchCancellation>)watchesField.GetValue(watchManager);
            Assert.False(watches.ContainsKey(watchId));
        }

        [Fact]
        public void GetServerWatchId_ShouldReturnCorrectId()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            // Setup the response stream to return a response with a watch ID
            var watchResponse = new WatchResponse
            {
                Created = true,
                WatchId = 123
            };

            var responseSequence = new Queue<WatchResponse>();
            responseSequence.Enqueue(watchResponse);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => responseSequence.Count > 0)
                .Callback(() => 
                {
                    if (responseSequence.Count > 0)
                    {
                        var response = responseSequence.Dequeue();
                        mockResponseStream.Setup(x => x.Current).Returns(response);
                    }
                });

            // Create a factory that returns our mock
            Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory = 
                (headers, deadline, token) => mockStreamingCall.Object;

            var watchManager = new WatchManager(factory);

            // Create a watch first
            var clientWatchId = watchManager.Watch(
                new WatchRequest { CreateRequest = new WatchCreateRequest { Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key") } },
                response => { });

            // Set up the watch ID mapping using reflection
            var watchIdMappingField = typeof(WatchManager).GetField("_watchIdMapping", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var watchIdMapping = (System.Collections.Concurrent.ConcurrentDictionary<long, long>)watchIdMappingField.GetValue(watchManager);
            watchIdMapping[123] = clientWatchId;

            // Act - use reflection to call the private method
            var method = typeof(WatchManager).GetMethod("GetServerWatchId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var serverWatchId = (long)method.Invoke(watchManager, new object[] { clientWatchId });

            // Assert
            Assert.Equal(123, serverWatchId);
        }
    }
} 