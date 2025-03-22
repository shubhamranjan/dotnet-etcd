using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Lockpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class LockClientUnitTests
{
    [Fact]
    public void Lock_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<V3Lockpb.Lock.LockClient>();
        string name = "test-lock";
        long leaseId = 12345;
        byte[] expectedKey = ByteString.CopyFromUtf8("test-lock-key").ToByteArray();

        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse { Key = ByteString.CopyFrom(expectedKey) });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new LockRequest
        {
            Name = ByteString.CopyFromUtf8(name),
            Lease = leaseId
        };
        var result = client.Lock(request);

        // Assert
        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name)) && 
                r.Lease == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Key.ToByteArray());
    }

    [Fact]
    public async Task LockAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<V3Lockpb.Lock.LockClient>();
        string name = "test-lock";
        long leaseId = 12345;
        byte[] expectedKey = ByteString.CopyFromUtf8("test-lock-key").ToByteArray();
        
        var expectedResponse = new LockResponse { Key = ByteString.CopyFrom(expectedKey) };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
            Name = ByteString.CopyFromUtf8(name),
            Lease = leaseId
        };
        var result = await client.LockAsync(request);

        // Assert
        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name)) && 
                r.Lease == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Key.ToByteArray());
    }

    [Fact]
    public void Unlock_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<V3Lockpb.Lock.LockClient>();
        byte[] key = ByteString.CopyFromUtf8("test-lock-key").ToByteArray();

        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new UnlockResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lock client
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        // Act
        var request = new UnlockRequest
        {
            Key = ByteString.CopyFrom(key)
        };
        client.Unlock(request);

        // Assert
        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFrom(key))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLockClient = new Mock<V3Lockpb.Lock.LockClient>();
        byte[] key = ByteString.CopyFromUtf8("test-lock-key").ToByteArray();
        
        var expectedResponse = new UnlockResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
            Key = ByteString.CopyFrom(key)
        };
        await client.UnlockAsync(request);

        // Assert
        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFrom(key))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
