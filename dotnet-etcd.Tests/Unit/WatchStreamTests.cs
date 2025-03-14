using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using Xunit;
using System.Collections.Concurrent;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class WatchStreamTests
    {
        [Fact]
        public async Task CreateWatchAsync_ShouldWriteRequestToStream()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var watchStream = new WatchStream(mockStreamingCall.Object);

            var watchRequest = new WatchRequest
            {
                CreateRequest = new WatchCreateRequest
                {
                    Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key")
                }
            };

            // Act
            await watchStream.CreateWatchAsync(watchRequest, response => { });

            // Assert
            mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r => 
                r.CreateRequest.Key.ToStringUtf8() == "test-key")), Times.Once);
        }

        [Fact]
        public async Task CancelWatchAsync_ShouldWriteCancelRequestToStream()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var watchStream = new WatchStream(mockStreamingCall.Object);

            // Act
            await watchStream.CancelWatchAsync(123);

            // Assert
            mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r => 
                r.CancelRequest != null && r.CancelRequest.WatchId == 123)), Times.Once);
        }

        [Fact]
        public void ProcessWatchResponses_ShouldInvokeCallbackForWatchEvents()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            // Set up the response stream to return a sequence of responses
            var responses = new Queue<(bool, WatchResponse)>();
            responses.Enqueue((true, new WatchResponse { WatchId = 123, Created = true }));
            responses.Enqueue((true, new WatchResponse { WatchId = 123, Events = { new Mvccpb.Event() } }));
            responses.Enqueue((false, null)); // End of stream

            int callCount = 0;
            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => 
                {
                    if (responses.Count == 0)
                        return false;
                    
                    var (hasNext, _) = responses.Peek();
                    if (!hasNext)
                    {
                        responses.Dequeue();
                        return false;
                    }
                    
                    return true;
                });

            mockResponseStream
                .Setup(x => x.Current)
                .Returns(() => 
                {
                    if (responses.Count == 0)
                        return null;
                    
                    var (_, response) = responses.Dequeue();
                    return response;
                });

            // Create a watch stream and set up a callback
            var callbackInvoked = false;
            Action<WatchResponse> callback = response => 
            {
                if (response.Events.Count > 0)
                    callbackInvoked = true;
            };

            // Act - create the watch stream which will start processing responses
            var watchStream = new WatchStream(mockStreamingCall.Object);
            
            // Register the callback for watch ID 123
            var field = typeof(WatchStream).GetField("_callbacks", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var callbacks = (ConcurrentDictionary<long, Action<WatchResponse>>)field.GetValue(watchStream);
            callbacks.TryAdd(123, callback);

            // Wait for the processing task to complete
            var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var processingTask = (Task)processingTaskField.GetValue(watchStream);
            
            // Use Task.Run to avoid blocking the test thread
            Task.Run(() => processingTask.Wait()).Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(callbackInvoked, "Callback should have been invoked for the watch event");
        }

        [Fact]
        public void Dispose_ShouldCompleteRequestStreamAndDisposeResources()
        {
            // Arrange
            var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

            mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
            mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

            mockResponseStream
                .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var watchStream = new WatchStream(mockStreamingCall.Object);

            // Act
            watchStream.Dispose();

            // Assert
            mockRequestStream.Verify(x => x.CompleteAsync(), Times.Once);
            mockStreamingCall.Verify(x => x.Dispose(), Times.Once);
        }
    }
} 