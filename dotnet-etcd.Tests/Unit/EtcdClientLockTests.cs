using System;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Lockpb;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientLockTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly EtcdClient _client;
    private readonly Mock<V3Lockpb.Lock.LockClient> _mockLockClient;

    public EtcdClientLockTests()
    {
        _mockConnection = MockConnection.Create();
        _mockLockClient = new Mock<V3Lockpb.Lock.LockClient>();
        _mockConnection.SetupGet(x => x.LockClient).Returns(_mockLockClient.Object);
        _client = new EtcdClient(_mockConnection.Object);
    }

    [Fact]
    public void Lock_WithStringName_ShouldCallLock_WithCorrectParameters()
    {
        // Arrange
        var name = "test-lock";
        var lockResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        _mockLockClient
            .Setup(x => x.Lock(
                It.IsAny<LockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(lockResponse);

        // Act
        var result = _client.Lock(name);

        // Assert
        _mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Name.ToStringUtf8() == name),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(lockResponse, result);
    }

    [Fact]
    public void Lock_WithRequest_ShouldCallLock_WithCorrectParameters()
    {
        // Arrange
        var request = new LockRequest
        {
            Name = ByteString.CopyFromUtf8("test-lock"),
            Lease = 12345
        };
        var lockResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        _mockLockClient
            .Setup(x => x.Lock(
                It.IsAny<LockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(lockResponse);

        // Act
        var result = _client.Lock(request);

        // Assert
        _mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r == request),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(lockResponse, result);
    }

    [Fact]
    public async Task LockAsync_WithStringName_ShouldCallLockAsync_WithCorrectParameters()
    {
        // Arrange
        var name = "test-lock";
        var lockResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        var asyncCall = AsyncUnaryCallFactory.Create(lockResponse);
        _mockLockClient
            .Setup(x => x.LockAsync(
                It.IsAny<LockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.LockAsync(name);

        // Assert
        _mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Name.ToStringUtf8() == name),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(lockResponse, result);
    }

    [Fact]
    public async Task LockAsync_WithRequest_ShouldCallLockAsync_WithCorrectParameters()
    {
        // Arrange
        var request = new LockRequest
        {
            Name = ByteString.CopyFromUtf8("test-lock"),
            Lease = 12345
        };
        var lockResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        var asyncCall = AsyncUnaryCallFactory.Create(lockResponse);
        _mockLockClient
            .Setup(x => x.LockAsync(
                It.IsAny<LockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.LockAsync(request);

        // Assert
        _mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r == request),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(lockResponse, result);
    }

    [Fact]
    public void Unlock_WithKey_ShouldCallUnlock_WithCorrectParameters()
    {
        // Arrange
        var key = "test-key";
        var unlockResponse = new UnlockResponse();
        _mockLockClient
            .Setup(x => x.Unlock(
                It.IsAny<UnlockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(unlockResponse);

        // Act
        var result = _client.Unlock(key);

        // Assert
        _mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.ToStringUtf8() == key),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(unlockResponse, result);
    }

    [Fact]
    public void Unlock_WithRequest_ShouldCallUnlock_WithCorrectParameters()
    {
        // Arrange
        var request = new UnlockRequest
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        var unlockResponse = new UnlockResponse();
        _mockLockClient
            .Setup(x => x.Unlock(
                It.IsAny<UnlockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(unlockResponse);

        // Act
        var result = _client.Unlock(request);

        // Assert
        _mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r == request),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(unlockResponse, result);
    }

    [Fact]
    public async Task UnlockAsync_WithKey_ShouldCallUnlockAsync_WithCorrectParameters()
    {
        // Arrange
        var key = "test-key";
        var unlockResponse = new UnlockResponse();
        var asyncCall = AsyncUnaryCallFactory.Create(unlockResponse);
        _mockLockClient
            .Setup(x => x.UnlockAsync(
                It.IsAny<UnlockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.UnlockAsync(key);

        // Assert
        _mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.ToStringUtf8() == key),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(unlockResponse, result);
    }

    [Fact]
    public async Task UnlockAsync_WithRequest_ShouldCallUnlockAsync_WithCorrectParameters()
    {
        // Arrange
        var request = new UnlockRequest
        {
            Key = ByteString.CopyFromUtf8("test-key")
        };
        var unlockResponse = new UnlockResponse();
        var asyncCall = AsyncUnaryCallFactory.Create(unlockResponse);
        _mockLockClient
            .Setup(x => x.UnlockAsync(
                It.IsAny<UnlockRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.UnlockAsync(request);

        // Assert
        _mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r == request),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(unlockResponse, result);
    }
}
