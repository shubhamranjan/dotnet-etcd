using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Lockpb;
using Lock = V3Lockpb.Lock;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class LockClientTests
{
    [Fact]
    public void Lock_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("lock-key-123")
        };

        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new LockRequest
        {
            Name = ByteString.CopyFromUtf8("test-lock")
        };
        var result = client.Lock(request);

        // Assert
        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Name.ToStringUtf8() == "test-lock"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("lock-key-123", result.Key.ToStringUtf8());
    }

    [Fact]
    public void Lock_WithStringName_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("lock-key-123")
        };

        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var result = client.Lock("test-lock");

        // Assert
        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Name.ToStringUtf8() == "test-lock"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("lock-key-123", result.Key.ToStringUtf8());
    }

    [Fact]
    public async Task LockAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new LockResponse
        {
            Key = ByteString.CopyFromUtf8("lock-key-123")
        };

        var asyncResponse = new AsyncUnaryCall<LockResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new LockRequest
        {
            Name = ByteString.CopyFromUtf8("test-lock")
        };
        var result = await client.LockAsync(request);

        // Assert
        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Name.ToStringUtf8() == "test-lock"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("lock-key-123", result.Key.ToStringUtf8());
    }

    [Fact]
    public void Unlock_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new UnlockResponse();

        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new UnlockRequest
        {
            Key = ByteString.CopyFromUtf8("lock-key-123")
        };
        var result = client.Unlock(request);

        // Assert
        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.ToStringUtf8() == "lock-key-123"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Unlock_WithStringKey_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new UnlockResponse();

        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var result = client.Unlock("lock-key-123");

        // Assert
        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.ToStringUtf8() == "lock-key-123"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<Lock.LockClient>();

        var expectedResponse = new UnlockResponse();

        var asyncResponse = new AsyncUnaryCall<UnlockResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockLockClient
            .Setup(x => x.UnlockAsync(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new UnlockRequest
        {
            Key = ByteString.CopyFromUtf8("lock-key-123")
        };
        var result = await client.UnlockAsync(request);

        // Assert
        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.ToStringUtf8() == "lock-key-123"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}