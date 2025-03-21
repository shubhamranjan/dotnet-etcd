using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientWatchTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IWatchManager> _mockWatchManager;
    private readonly EtcdClient _client;

    public EtcdClientWatchTests()
    {
        _mockConnection = MockConnection.Create();
        _mockWatchManager = new Mock<IWatchManager>();
        _client = new EtcdClient(_mockConnection.Object, watchManager: _mockWatchManager.Object);
    }

    [Fact]
    public void Watch_WithMultipleRequests_AndResponseActions_ShouldCallWatchManager()
    {
        // Arrange
        var requests = new WatchRequest[]
        {
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } },
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key2") } }
        };

        Action<WatchResponse>[] methods = new Action<WatchResponse>[]
        {
            _ => { },
            _ => { }
        };

        // Act
        _client.Watch(requests, methods);

        // Assert
        _mockWatchManager.Verify(x => x.Watch(
            It.Is<WatchRequest>(r => r == requests[0]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mockWatchManager.Verify(x => x.Watch(
            It.Is<WatchRequest>(r => r == requests[1]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Watch_WithMultipleRequests_AndEventActions_ShouldCallWatchManager()
    {
        // Arrange
        var requests = new WatchRequest[]
        {
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } },
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key2") } }
        };

        Action<WatchEvent[]>[] methods = new Action<WatchEvent[]>[]
        {
            _ => { },
            _ => { }
        };

        // Act
        _client.Watch(requests, methods);

        // Assert
        _mockWatchManager.Verify(x => x.Watch(
            It.Is<WatchRequest>(r => r == requests[0]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mockWatchManager.Verify(x => x.Watch(
            It.Is<WatchRequest>(r => r == requests[1]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Watch_WithDifferentArrayLengths_ShouldThrowArgumentException()
    {
        // Arrange
        var requests = new WatchRequest[]
        {
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } },
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key2") } }
        };

        Action<WatchResponse>[] methods = new Action<WatchResponse>[]
        {
            _ => { }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _client.Watch(requests, methods));
    }

    [Fact]
    public async Task WatchAsync_WithMultipleRequests_AndEventActions_ShouldCallWatchManagerAsync()
    {
        // Arrange
        var requests = new WatchRequest[]
        {
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } },
            new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key2") } }
        };

        Action<WatchEvent[]>[] methods = new Action<WatchEvent[]>[]
        {
            _ => { },
            _ => { }
        };

        long watchId1 = 1;
        long watchId2 = 2;

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r == requests[0]),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId1);

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r == requests[1]),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId2);

        // Act
        var result = await _client.WatchAsync(requests, methods);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(watchId1, result[0]);
        Assert.Equal(watchId2, result[1]);

        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r == requests[0]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r == requests[1]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WatchAsync_WithSingleRequest_AndSingleResponseAction_ShouldCallWatchManagerAsync()
    {
        // Arrange
        var request = new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } };
        Action<WatchResponse> method = _ => { };
        long watchId = 123;

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r == request),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId);

        // Act
        var result = await _client.WatchAsync(request, method);

        // Assert
        Assert.Equal(watchId, result);

        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r == request),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WatchAsync_WithSingleRequest_AndMultipleResponseActions_ShouldCallWatchManagerAsync()
    {
        // Arrange
        var request = new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } };
        Action<WatchResponse>[] methods = new Action<WatchResponse>[]
        {
            _ => { },
            _ => { }
        };
        long watchId = 123;

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r == request),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId);

        // Act
        var result = await _client.WatchAsync(request, methods);

        // Assert
        Assert.Equal(watchId, result);

        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r == request),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WatchAsync_WithSingleRequest_AndEventAction_ShouldCallWatchManagerAsync()
    {
        // Arrange
        var request = new WatchRequest { CreateRequest = new WatchCreateRequest { Key = ByteString.CopyFromUtf8("key1") } };
        Action<WatchEvent[]> method = _ => { };
        long watchId = 123;

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r == request),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId);

        // Act
        await _client.WatchAsync(request, method);

        // Assert
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r == request),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void CancelWatch_ShouldCallWatchManager()
    {
        // Arrange
        long watchId = 123;

        // Act
        _client.CancelWatch(watchId);

        // Assert
        _mockWatchManager.Verify(x => x.CancelWatch(watchId), Times.Once);
    }

    [Fact]
    public async Task WatchRangeAsync_WithSinglePath_ShouldCallWatchManagerAsync()
    {
        // Arrange
        string path = "test/path";
        Action<WatchResponse> method = _ => { };
        long watchId = 123;

        // Create expected request with range parameters
        var expectedRequest = new WatchRequest
        {
            CreateRequest = new WatchCreateRequest
            {
                Key = ByteString.CopyFromUtf8(path),
                RangeEnd = ByteString.CopyFromUtf8("\0"), // Range end for prefix
                ProgressNotify = true,
                PrevKv = true
            }
        };

        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId);

        // Act
        var result = await _client.WatchRangeAsync(path, method);

        // Assert
        Assert.Equal(watchId, result);
        
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => 
                r.CreateRequest.Key.ToStringUtf8() == path &&
                r.CreateRequest.ProgressNotify == true &&
                r.CreateRequest.PrevKv == true),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WatchRangeAsync_WithSinglePath_ShouldCallWatchManagerAsync_WithCorrectParameters()
    {
        // Arrange
        var path = "test-path";
        var watchId = 123L;
        Action<WatchResponse> callback = response => { };
        
        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchId);

        // Act
        var result = await _client.WatchRangeAsync(path, callback);

        // Assert
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => 
                r.CreateRequest.Key.ToStringUtf8() == path && 
                r.CreateRequest.RangeEnd != null),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(watchId, result);
    }

    [Fact]
    public async Task WatchRangeAsync_WithMultipleMethods_ShouldCallWatchManagerAsync_WithWrappedCallback()
    {
        // Arrange
        var path = "test-path";
        var watchId = 123L;
        var callbackCount = 0;
        
        Action<WatchResponse>[] callbacks = new Action<WatchResponse>[]
        {
            response => { callbackCount++; },
            response => { callbackCount++; }
        };
        
        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Callback<WatchRequest, Action<WatchResponse>, Metadata, DateTime?, CancellationToken>(
                (req, callback, headers, deadline, token) =>
                {
                    // Call the wrapper to verify it calls all methods
                    var response = new WatchResponse();
                    callback(response);
                })
            .ReturnsAsync(watchId);

        // Act
        var result = await _client.WatchRangeAsync(path, callbacks);

        // Assert
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => 
                r.CreateRequest.Key.ToStringUtf8() == path && 
                r.CreateRequest.RangeEnd != null),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(watchId, result);
        Assert.Equal(2, callbackCount); // Verify both callbacks were called
    }

    [Fact]
    public async Task WatchRangeAsync_WithMultiplePaths_ShouldCallWatchManagerAsync_ForEachPath()
    {
        // Arrange
        var paths = new[] { "test-path1", "test-path2" };
        var watchIds = new[] { 123L, 456L };
        Action<WatchResponse> callback = response => { };
        
        var callIndex = 0;
        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => watchIds[callIndex++]);

        // Act
        var result = await _client.WatchRangeAsync(paths, callback);

        // Assert
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.IsAny<WatchRequest>(),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        
        Assert.Equal(2, result.Length);
        Assert.Equal(watchIds[0], result[0]);
        Assert.Equal(watchIds[1], result[1]);
    }

    [Fact]
    public async Task WatchRangeAsync_WithMultiplePathsAndMethods_ShouldVerifyPathsAndMethodsCountMatch()
    {
        // Arrange
        var paths = new[] { "test-path1", "test-path2" };
        var callbacks = new Action<WatchResponse>[] 
        { 
            response => { }, 
            response => { } 
        };
        var watchIds = new[] { 123L, 456L };
        
        var callIndex = 0;
        _mockWatchManager
            .Setup(x => x.WatchAsync(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => watchIds[callIndex++]);

        // Act
        var result = await _client.WatchRangeAsync(paths, callbacks);

        // Assert
        _mockWatchManager.Verify(x => x.WatchAsync(
            It.Is<WatchRequest>(r => r.CreateRequest.Key.ToStringUtf8() == paths[0] || 
                                    r.CreateRequest.Key.ToStringUtf8() == paths[1]),
            It.IsAny<Action<WatchResponse>>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        
        Assert.Equal(2, result.Length);
        Assert.Equal(watchIds[0], result[0]);
        Assert.Equal(watchIds[1], result[1]);
    }

    [Fact]
    public async Task WatchRangeAsync_WithMismatchedPathsAndMethods_ShouldThrowArgumentException()
    {
        // Arrange
        var paths = new[] { "test-path1", "test-path2" };
        var callbacks = new Action<WatchResponse>[] { response => { } };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _client.WatchRangeAsync(paths, callbacks));
    }
}
