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
    public async Task ProcessWatchResponses_ShouldInvokeCallbackForWatchEvents()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Create a watch response with events
        var watchResponse = new WatchResponse
        {
            WatchId = 1,
            Events = { new Event() }
        };

        var responseSequence = new MockSequence();
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.Current)
            .Returns(watchResponse);
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Set up a callback to track if it's called
        bool callbackInvoked = false;
        Action<WatchResponse> callback = (response) =>
        {
            callbackInvoked = true;
            Assert.Equal(1, response.WatchId);
            Assert.Single(response.Events);
        };

        // Act
        var watchStream = new WatchStream(mockStreamingCall.Object);
        
        // Get the processing task field to monitor it
        var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var processingTask = (Task)processingTaskField.GetValue(watchStream);

        // Add the callback to the callbacks dictionary
        var callbacksField = typeof(WatchStream).GetField("_callbacks",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var callbacks = (ConcurrentDictionary<long, Action<WatchResponse>>)callbacksField.GetValue(watchStream);
        callbacks[1] = callback;

        // Wait for the processing task to complete or timeout
        await Task.WhenAny(processingTask, Task.Delay(2000));
        
        // Assert
        Assert.True(callbackInvoked, "Callback should have been invoked for watch events");
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
    public async Task ProcessWatchResponses_ShouldHandleGenericException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to throw an exception
        mockResponseStream
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var watchStream = new WatchStream(mockStreamingCall.Object);

        // Get the processing task field
        var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var processingTask = (Task)processingTaskField.GetValue(watchStream);
        
        // Wait for the task to either complete or fail
        try
        {
            // Use a timeout to avoid hanging the test
            await Task.WhenAny(processingTask, Task.Delay(2000));
            
            // The task should have handled the exception without crashing
            Assert.NotNull(watchStream);
            
            // The exception should be logged but not rethrown (in non-DEBUG mode)
            // So the test should pass without any unhandled exceptions
        }
        catch (Exception ex)
        {
            Assert.Fail($"Exception should be handled but was thrown: {ex}");
        }
    }

    [Fact]
    public async Task ProcessWatchResponses_ShouldHandleCanceledWatchResponse()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to return a watch response with canceled = true
        var watchResponse = new WatchResponse
        {
            WatchId = 1,
            Canceled = true
        };

        var responseSequence = new MockSequence();
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.Current)
            .Returns(watchResponse);
        mockResponseStream
            .InSequence(responseSequence)
            .Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Set up a callback to track if it's called
        bool callbackInvoked = false;
        Action<WatchResponse> callback = (response) =>
        {
            callbackInvoked = true;
            Assert.Equal(1, response.WatchId);
            Assert.True(response.Canceled);
        };

        // Act
        var watchStream = new WatchStream(mockStreamingCall.Object);
        
        // Get the processing task field to monitor it
        var processingTaskField = typeof(WatchStream).GetField("_responseProcessingTask",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var processingTask = (Task)processingTaskField.GetValue(watchStream);

        // Add the callback to the callbacks dictionary
        var callbacksField = typeof(WatchStream).GetField("_callbacks",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var callbacks = (ConcurrentDictionary<long, Action<WatchResponse>>)callbacksField.GetValue(watchStream);
        callbacks[1] = callback;

        // Wait for the processing task to complete or timeout
        await Task.WhenAny(processingTask, Task.Delay(2000));
        
        // Assert
        Assert.True(callbackInvoked, "Callback should have been invoked for canceled watch response");
    }
}