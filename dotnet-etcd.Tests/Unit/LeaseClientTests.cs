using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class LeaseClientTests
{
    [Fact]
    public void LeaseGrant_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();

        var expectedResponse = new LeaseGrantResponse
        {
            ID = 123,
            TTL = 60
        };

        mockLeaseClient
            .Setup(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseGrantRequest { ID = 123, TTL = 60 };
        var result = client.LeaseGrant(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseGrant(
            It.Is<LeaseGrantRequest>(r => r.ID == 123 && r.TTL == 60),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123, result.ID);
        Assert.Equal(60, result.TTL);
    }

    [Fact]
    public async Task LeaseGrantAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();

        var expectedResponse = new LeaseGrantResponse
        {
            ID = 123,
            TTL = 60
        };

        var asyncResponse = new AsyncUnaryCall<LeaseGrantResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockLeaseClient
            .Setup(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseGrantRequest { ID = 123, TTL = 60 };
        var result = await client.LeaseGrantAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseGrantAsync(
            It.Is<LeaseGrantRequest>(r => r.ID == 123 && r.TTL == 60),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123, result.ID);
        Assert.Equal(60, result.TTL);
    }

    [Fact]
    public void LeaseRevoke_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();

        var expectedResponse = new LeaseRevokeResponse();

        mockLeaseClient
            .Setup(x => x.LeaseRevoke(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseRevokeRequest { ID = 123 };
        var result = client.LeaseRevoke(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseRevoke(
            It.Is<LeaseRevokeRequest>(r => r.ID == 123),
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

        var expectedResponse = new LeaseRevokeResponse();

        var asyncResponse = new AsyncUnaryCall<LeaseRevokeResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockLeaseClient
            .Setup(x => x.LeaseRevokeAsync(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseRevokeRequest { ID = 123 };
        var result = await client.LeaseRevokeAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseRevokeAsync(
            It.Is<LeaseRevokeRequest>(r => r.ID == 123),
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

        var expectedResponse = new LeaseTimeToLiveResponse
        {
            ID = 123,
            TTL = 30,
            GrantedTTL = 60
        };

        mockLeaseClient
            .Setup(x => x.LeaseTimeToLive(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseTimeToLiveRequest { ID = 123, Keys = true };
        var result = client.LeaseTimeToLive(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseTimeToLive(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == 123 && r.Keys == true),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123, result.ID);
        Assert.Equal(30, result.TTL);
        Assert.Equal(60, result.GrantedTTL);
    }

    [Fact]
    public async Task LeaseTimeToLiveAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockLeaseClient = new Mock<Lease.LeaseClient>();

        var expectedResponse = new LeaseTimeToLiveResponse
        {
            ID = 123,
            TTL = 30,
            GrantedTTL = 60
        };

        var asyncResponse = new AsyncUnaryCall<LeaseTimeToLiveResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockLeaseClient
            .Setup(x => x.LeaseTimeToLiveAsync(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Lease client
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        // Act
        var request = new LeaseTimeToLiveRequest { ID = 123, Keys = true };
        var result = await client.LeaseTimeToLiveAsync(request);

        // Assert
        mockLeaseClient.Verify(x => x.LeaseTimeToLiveAsync(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == 123 && r.Keys == true),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123, result.ID);
        Assert.Equal(30, result.TTL);
        Assert.Equal(60, result.GrantedTTL);
    }
}