using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class LeaseClientUnitTests
{
    [Fact]
    public void LeaseGrant_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long ttl = 10;
        long expectedLeaseId = 12345;

        mockLeaseClient
            .Setup(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseGrantResponse { ID = expectedLeaseId });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseGrantRequest { TTL = ttl };
        var result = client.LeaseGrant(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseGrant(
            It.Is<LeaseGrantRequest>(r => r.TTL == ttl),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedLeaseId, result.ID);
    }

    [Fact]
    public async Task LeaseGrantAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long ttl = 10;
        long expectedLeaseId = 12345;

        var expectedResponse = new LeaseGrantResponse { ID = expectedLeaseId };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockLeaseClient
            .Setup(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseGrantRequest { TTL = ttl };
        var result = await client.LeaseGrantAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseGrantAsync(
            It.Is<LeaseGrantRequest>(r => r.TTL == ttl),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedLeaseId, result.ID);
    }

    [Fact]
    public void LeaseRevoke_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long leaseId = 12345;

        mockLeaseClient
            .Setup(x => x.LeaseRevoke(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseRevokeResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseRevokeRequest { ID = leaseId };
        client.LeaseRevoke(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseRevoke(
            It.Is<LeaseRevokeRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task LeaseRevokeAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long leaseId = 12345;

        var expectedResponse = new LeaseRevokeResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockLeaseClient
            .Setup(x => x.LeaseRevokeAsync(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseRevokeRequest { ID = leaseId };
        await client.LeaseRevokeAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseRevokeAsync(
            It.Is<LeaseRevokeRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void LeaseTimeToLive_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long leaseId = 12345;
        long expectedTtl = 8;

        mockLeaseClient
            .Setup(x => x.LeaseTimeToLive(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseTimeToLiveResponse { ID = leaseId, TTL = expectedTtl });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseTimeToLiveRequest { ID = leaseId };
        var result = client.LeaseTimeToLive(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseTimeToLive(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(leaseId, result.ID);
        Assert.Equal(expectedTtl, result.TTL);
    }

    [Fact]
    public async Task LeaseTimeToLiveAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        long leaseId = 12345;
        long expectedTtl = 8;

        var expectedResponse = new LeaseTimeToLiveResponse { ID = leaseId, TTL = expectedTtl };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockLeaseClient
            .Setup(x => x.LeaseTimeToLiveAsync(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseTimeToLiveRequest { ID = leaseId };
        var result = await client.LeaseTimeToLiveAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseTimeToLiveAsync(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(leaseId, result.ID);
        Assert.Equal(expectedTtl, result.TTL);
    }
}