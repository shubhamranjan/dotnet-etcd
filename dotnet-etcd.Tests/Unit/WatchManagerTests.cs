using System.Collections.Concurrent;
using System.Reflection;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

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
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("test-key") } },
            response => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "test-key")), Times.Once);
    }

    [Fact]
    public void Watch_ShouldReturnWatchId()
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
        var watchId = watchManager.Watch(
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("test-key") } },
            response => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "test-key")), Times.Once);
    }

    [Fact]
    public void WatchRange_ShouldCreateWatchForRange()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream to return a single response
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                // Set up the current response
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 123
                });
            });

        // Create a factory that returns our mock
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act
        // Explicitly specify the type of the callback parameter to resolve ambiguity
        Action<WatchResponse> callback = response => { };
        var watchId = watchManager.WatchRange("test-prefix", callback);

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "test-prefix" &&
            r.CreateRequest.RangeEnd.ToStringUtf8() == "test-prefiy")), Times.Once);
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
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("test-key") } },
            response => { });

        // Set up the watch ID mapping using reflection
        var watchIdMappingField = typeof(WatchManager).GetField("_watchIdMapping",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (watchIdMappingField != null)
        {
            var watchIdMapping = (ConcurrentDictionary<long, long>)watchIdMappingField.GetValue(watchManager)!;
            watchIdMapping[123] = watchId;
        }

        // Act
        watchManager.CancelWatch(watchId);

        // Assert
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CancelRequest != null && r.CancelRequest.WatchId == 123)), Times.Once);

        // Verify the watch was removed from the dictionary
        var watchesField = typeof(WatchManager).GetField("_watches",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (watchesField != null)
        {
            var watches =
                (ConcurrentDictionary<long, WatchManager.WatchCancellation>)watchesField.GetValue(watchManager)!;
            Assert.False(watches.ContainsKey(watchId));
        }
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
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("test-key") } },
            response => { });

        // Set up the watch ID mapping using reflection
        var watchIdMappingField = typeof(WatchManager).GetField("_watchIdMapping",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (watchIdMappingField != null)
        {
            var watchIdMapping = (ConcurrentDictionary<long, long>)watchIdMappingField.GetValue(watchManager)!;
            watchIdMapping[123] = clientWatchId;
        }

        // Act - use reflection to call the private method
        var method = typeof(WatchManager).GetMethod("GetServerWatchId",
            BindingFlags.NonPublic | BindingFlags.Instance);

        long serverWatchId = -1;
        if (method != null) serverWatchId = (long)method.Invoke(watchManager, new object[] { clientWatchId })!;

        // Assert
        Assert.Equal(123, serverWatchId);
    }

    [Fact]
    public void GetServerWatchId_WhenMappingNotFound_ShouldReturnMinusOne()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Create a factory that returns our mock
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act - use reflection to call the private method
        var method = typeof(WatchManager).GetMethod("GetServerWatchId",
            BindingFlags.NonPublic | BindingFlags.Instance);

        long serverWatchId = 0;
        if (method != null) serverWatchId = (long)method.Invoke(watchManager, new object[] { 999 })!;

        // Assert
        Assert.Equal(-1, serverWatchId);
    }

    [Fact]
    public void Dispose_ShouldCancelAllWatchesAndDisposeWatchStream()
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
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("test-key") } },
            response => { });

        // Act
        watchManager.Dispose();

        // Assert
        // Verify the watches dictionary is empty
        var watchesField = typeof(WatchManager).GetField("_watches",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (watchesField != null)
        {
            var watches =
                (ConcurrentDictionary<long, WatchManager.WatchCancellation>)watchesField.GetValue(watchManager)!;
            Assert.Empty(watches);
        }

        // Verify the watch stream is null
        var watchStreamField = typeof(WatchManager).GetField("_watchStream",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (watchStreamField != null)
        {
            var watchStream = watchStreamField.GetValue(watchManager);
            Assert.Null(watchStream);
        }

        // Verify the disposed flag is set
        var disposedField = typeof(WatchManager).GetField("_disposed",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (disposedField != null)
        {
            var disposed = (bool)disposedField.GetValue(watchManager)!;
            Assert.True(disposed);
        }
    }

    [Fact]
    public async Task WatchAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var factory =
            new Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>(
                (headers, deadline, token) => mockStreamingCall.Object);

        var watchManager = new WatchManager(factory);
        watchManager.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => watchManager.WatchAsync(
            new WatchRequest(), response => { }));
    }

    [Fact]
    public void WatchRange_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var factory =
            new Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>(
                (headers, deadline, token) => mockStreamingCall.Object);

        var watchManager = new WatchManager(factory);
        watchManager.Dispose();

        // Act & Assert
        // Explicitly specify the type of the callback parameter to resolve ambiguity
        Action<WatchResponse> callback = response => { };
        Assert.Throws<ObjectDisposedException>(() => watchManager.WatchRange("test", callback));
    }

    [Fact]
    public void CancelWatch_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var factory =
            new Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>(
                (headers, deadline, token) => mockStreamingCall.Object);

        var watchManager = new WatchManager(factory);
        watchManager.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => watchManager.CancelWatch(1));
    }

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WatchManager(null!));
    }

    [Fact]
    public void Watch_WithKey_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockCall.Setup(c => c.RequestStream).Returns(mockRequestStream.Object);
        mockCall.Setup(c => c.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 2
                });
            });

        var watchManager = new WatchManager((headers, deadline, cancellationToken) => mockCall.Object);

        // Act
        var watchId = watchManager.Watch("test-key", response => { });

        // Assert
        Assert.Equal(2, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(
            It.Is<WatchRequest>(r =>
                r.CreateRequest.Key.ToStringUtf8() == "test-key" &&
                r.CreateRequest.ProgressNotify == true &&
                r.CreateRequest.PrevKv == true)
        ), Times.Once);
    }

    [Fact]
    public void Watch_WithKeyAndStartRevision_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockCall.Setup(c => c.RequestStream).Returns(mockRequestStream.Object);
        mockCall.Setup(c => c.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 2
                });
            });

        var watchManager = new WatchManager((headers, deadline, cancellationToken) => mockCall.Object);

        // Act
        var watchId = watchManager.Watch("test-key", 100, response => { });

        // Assert
        Assert.Equal(2, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(
            It.Is<WatchRequest>(r =>
                r.CreateRequest.Key.ToStringUtf8() == "test-key" &&
                r.CreateRequest.StartRevision == 100 &&
                r.CreateRequest.ProgressNotify == true &&
                r.CreateRequest.PrevKv == true)
        ), Times.Once);
    }

    [Fact]
    public void WatchRange_WithPrefix_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 321
                });
            });

        // Create a factory
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act
        var watchId = watchManager.WatchRange("prefix/", (WatchEvent _) => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "prefix/" &&
            r.CreateRequest.RangeEnd.ToStringUtf8() == "prefix0")), Times.Once);
    }

    [Fact]
    public void WatchRange_WithPrefixAndStartRevision_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 654
                });
            });

        // Create a factory
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act
        var watchId = watchManager.WatchRange("prefix/", 200, _ => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "prefix/" &&
            r.CreateRequest.StartRevision == 200 &&
            r.CreateRequest.RangeEnd.ToStringUtf8() == "prefix0")), Times.Once);
    }

    [Fact]
    public async Task WatchAsync_WithKey_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockCall.Setup(c => c.RequestStream).Returns(mockRequestStream.Object);
        mockCall.Setup(c => c.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 2
                });
            });

        var watchManager = new WatchManager((headers, deadline, cancellationToken) => mockCall.Object);

        // Act
        var watchId = await watchManager.WatchAsync("async-key", response => { });

        // Assert
        Assert.Equal(2, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(
            It.Is<WatchRequest>(r =>
                r.CreateRequest.Key.ToStringUtf8() == "async-key" &&
                r.CreateRequest.ProgressNotify == true &&
                r.CreateRequest.PrevKv == true)
        ), Times.Once);
    }

    [Fact]
    public async Task WatchRangeAsync_WithPrefix_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 753
                });
            });

        // Create a factory
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act
        var watchId = await watchManager.WatchRangeAsync("async-prefix/", _ => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "async-prefix/" &&
            r.CreateRequest.RangeEnd.ToStringUtf8() == "async-prefix0")), Times.Once);
    }

    [Fact]
    public async Task WatchAsync_WithKeyAndStartRevision_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockCall.Setup(c => c.RequestStream).Returns(mockRequestStream.Object);
        mockCall.Setup(c => c.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 2
                });
            });

        var watchManager = new WatchManager((headers, deadline, cancellationToken) => mockCall.Object);

        // Act
        var watchId = await watchManager.WatchAsync("async-key-rev", 300, response => { });

        // Assert
        Assert.Equal(2, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(
            It.Is<WatchRequest>(r =>
                r.CreateRequest.Key.ToStringUtf8() == "async-key-rev" &&
                r.CreateRequest.StartRevision == 300 &&
                r.CreateRequest.ProgressNotify == true &&
                r.CreateRequest.PrevKv == true)
        ), Times.Once);
    }

    [Fact]
    public async Task WatchRangeAsync_WithPrefixAndStartRevision_ShouldCreateProperWatchRequest()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        mockStreamingCall.Setup(x => x.RequestStream).Returns(mockRequestStream.Object);
        mockStreamingCall.Setup(x => x.ResponseStream).Returns(mockResponseStream.Object);

        // Set up the response stream
        mockResponseStream.Setup(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback(() =>
            {
                mockResponseStream.Setup(x => x.Current).Returns(new WatchResponse
                {
                    Created = true,
                    WatchId = 357
                });
            });

        // Create a factory
        Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>> factory =
            (headers, deadline, token) => mockStreamingCall.Object;

        var watchManager = new WatchManager(factory);

        // Act
        var watchId = await watchManager.WatchRangeAsync("async-prefix-rev/", 400, _ => { });

        // Assert
        Assert.NotEqual(0, watchId);
        mockRequestStream.Verify(x => x.WriteAsync(It.Is<WatchRequest>(r =>
            r.CreateRequest.Key.ToStringUtf8() == "async-prefix-rev/" &&
            r.CreateRequest.StartRevision == 400 &&
            r.CreateRequest.RangeEnd.ToStringUtf8() == "async-prefix-rev0")), Times.Once);
    }

    [Fact]
    public void GetRangeEnd_PrivateMethod_ShouldCalculateCorrectRangeEnd()
    {
        // Arrange
        var mockStreamingCall = new Mock<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>();
        var factory =
            new Func<Metadata, DateTime?, CancellationToken, IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>(
                (headers, deadline, token) => mockStreamingCall.Object);

        var watchManager = new WatchManager(factory);

        // Act - use reflection to call the private method
        var method = typeof(WatchManager).GetMethod("GetRangeEnd",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = string.Empty;
        if (method != null) result = (string)method.Invoke(null, new object[] { "test" });

        // Assert
        Assert.Equal("tesu", result);
    }
}