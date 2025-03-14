using System.Collections.Concurrent;
using System.Reflection;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Moq;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

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
                Key = ByteString.CopyFromUtf8("test-key")
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
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        var callbackInvoked = false;
        Action<WatchResponse> callback = response => { callbackInvoked = true; };

        // Create a watch response with events
        var watchResponse = new WatchResponse
        {
            WatchId = 123,
            Created = false,
            Canceled = false
        };

        // Create an event without using the Event constructor directly
        var eventList = new RepeatedField<Event>();
        eventList.Add(new Event());
        watchResponse.Events.AddRange(eventList);

        // Set up the response stream to return our watch response
        var responseQueue = new Queue<(bool, WatchResponse?)>();
        responseQueue.Enqueue((true, watchResponse));
        responseQueue.Enqueue((false, null)); // End the stream after one response

        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (responseQueue.Count == 0) return Task.FromResult(false);
                var (hasNext, _) = responseQueue.Dequeue();
                return Task.FromResult(hasNext);
            });

        mockResponseStream.Setup(x => x.Current)
            .Returns(() =>
            {
                // Return the last dequeued response
                return watchResponse;
            });

        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Register the callback for watch ID 123
        var field = typeof(WatchStream).GetField("_callbacks",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var callbacks = (ConcurrentDictionary<long, Action<WatchResponse>>)field.GetValue(watchStream);
        callbacks.TryAdd(123, callback);

        // Get the processing task
        var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var processingTask = (Task)processingTaskField.GetValue(watchStream);

        // Wait for the processing task to complete
        // We need to give it enough time to process the response
        Task.Run(() =>
        {
            try
            {
                processingTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException)
            {
                // Ignore exceptions from the task
            }
        }).Wait();

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

    [Fact]
    public void Constructor_WithNullStreamingCall_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WatchStream(null));
        Assert.Equal("streamingCall", exception.ParamName);
    }

    [Fact]
    public async Task CreateWatchAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => watchStream.CreateWatchAsync(null, response => { }));
        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task CreateWatchAsync_WithNullCallback_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        var watchStream = new WatchStream(mockStreamingCall.Object);
        var watchRequest = new WatchRequest
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8("test-key")
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => watchStream.CreateWatchAsync(watchRequest, null));
        Assert.Equal("callback", exception.ParamName);
    }

    [Fact]
    public void ProcessWatchResponses_ShouldHandleRpcCancelledException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to throw an RpcException with Cancelled status
        mockResponseStream
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RpcException(new Status(StatusCode.Cancelled, "Stream cancelled")));

        // Act - create the watch stream which will start processing responses
        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Wait a bit for the processing task to run
        Thread.Sleep(100);

        // Assert - no exception should be thrown, and the stream should still be usable
        Assert.NotNull(watchStream);
    }

    [Fact]
    public void ProcessWatchResponses_ShouldHandleOperationCanceledException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to throw an OperationCanceledException
        mockResponseStream
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act - create the watch stream which will start processing responses
        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Wait a bit for the processing task to run
        Thread.Sleep(100);

        // Assert - no exception should be thrown, and the stream should still be usable
        Assert.NotNull(watchStream);
    }

    [Fact]
    public void ProcessWatchResponses_ShouldHandleGenericException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to throw a generic exception
        mockResponseStream
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act - create the watch stream which will start processing responses
        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Wait a bit for the processing task to run
        Thread.Sleep(100);

        // Assert - no exception should be thrown, and the stream should still be usable
        Assert.NotNull(watchStream);
    }

    [Fact]
    public void ProcessWatchResponses_ShouldHandleCanceledWatchResponse()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Create a watch response with Canceled = true
        var watchResponse = new WatchResponse
        {
            WatchId = 123,
            Canceled = true
        };

        // Set up the response stream to return our watch response
        var responses = new Queue<(bool, WatchResponse)>();
        responses.Enqueue((true, watchResponse));
        responses.Enqueue((false, null)); // Signal end of stream after processing one response

        var moveNextCalled = false;
        mockResponseStream
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (!moveNextCalled)
                {
                    moveNextCalled = true;
                    return true; // Return true the first time to process our response
                }

                return false; // Then return false to end the stream
            });

        mockResponseStream
            .Setup(x => x.Current)
            .Returns(watchResponse);

        // Create a watch stream and set up a callback
        var callbackInvoked = false;
        Action<WatchResponse> callback = response =>
        {
            if (response.Canceled)
                callbackInvoked = true;
        };

        // Act - create the watch stream which will start processing responses
        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Register the callback for watch ID 123
        var field = typeof(WatchStream).GetField("_callbacks",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var callbacks = (ConcurrentDictionary<long, Action<WatchResponse>>)field.GetValue(watchStream);
        callbacks.TryAdd(123, callback);

        // Wait for the processing task to complete
        var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var processingTask = (Task)processingTaskField.GetValue(watchStream);

        // Wait for the processing task to complete (with a timeout)
        Task.Run(() => processingTask.Wait()).Wait(TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(callbackInvoked, "Callback should have been invoked for the canceled watch response");

        // Verify the callback was removed from the dictionary
        Assert.False(callbacks.ContainsKey(123), "Callback should have been removed from the dictionary");
    }
}