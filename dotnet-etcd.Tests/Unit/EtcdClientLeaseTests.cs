using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientLeaseTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly EtcdClient _client;
    private readonly Mock<Lease.LeaseClient> _mockLeaseClient;

    public EtcdClientLeaseTests()
    {
        _mockConnection = MockConnection.Create();
        _mockLeaseClient = new Mock<Lease.LeaseClient>();
        _mockConnection.SetupGet(x => x.LeaseClient).Returns(_mockLeaseClient.Object);
        _client = new EtcdClient(_mockConnection.Object);
    }

    [Fact]
    public void LeaseGrant_WithTTL_ShouldCallLeaseGrant_WithCorrectParameters()
    {
        // Arrange
        var ttl = 10;
        var leaseGrantResponse = new LeaseGrantResponse { ID = 12345, TTL = ttl };
        _mockLeaseClient
            .Setup(x => x.LeaseGrant(
                It.IsAny<LeaseGrantRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(leaseGrantResponse);

        // Act
        var result = _client.LeaseGrant(new LeaseGrantRequest { TTL = ttl });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseGrant(
            It.Is<LeaseGrantRequest>(r => r.TTL == ttl),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseGrantResponse, result);
    }

    [Fact]
    public async Task LeaseGrantAsync_ShouldCallLeaseGrantAsync_WithCorrectParameters()
    {
        // Arrange
        var ttl = 10;
        var leaseGrantResponse = new LeaseGrantResponse { ID = 12345, TTL = ttl };
        var asyncCall = AsyncUnaryCallFactory.Create(leaseGrantResponse);
        _mockLeaseClient
            .Setup(x => x.LeaseGrantAsync(
                It.IsAny<LeaseGrantRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = ttl });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseGrantAsync(
            It.Is<LeaseGrantRequest>(r => r.TTL == ttl),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseGrantResponse, result);
    }

    [Fact]
    public void LeaseRevoke_ShouldCallLeaseRevoke_WithCorrectParameters()
    {
        // Arrange
        var leaseId = 12345L;
        var leaseRevokeResponse = new LeaseRevokeResponse();
        _mockLeaseClient
            .Setup(x => x.LeaseRevoke(
                It.IsAny<LeaseRevokeRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(leaseRevokeResponse);

        // Act
        var result = _client.LeaseRevoke(new LeaseRevokeRequest { ID = leaseId });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseRevoke(
            It.Is<LeaseRevokeRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseRevokeResponse, result);
    }

    [Fact]
    public async Task LeaseRevokeAsync_ShouldCallLeaseRevokeAsync_WithCorrectParameters()
    {
        // Arrange
        var leaseId = 12345L;
        var leaseRevokeResponse = new LeaseRevokeResponse();
        var asyncCall = AsyncUnaryCallFactory.Create(leaseRevokeResponse);
        _mockLeaseClient
            .Setup(x => x.LeaseRevokeAsync(
                It.IsAny<LeaseRevokeRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = leaseId });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseRevokeAsync(
            It.Is<LeaseRevokeRequest>(r => r.ID == leaseId),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseRevokeResponse, result);
    }

    [Fact]
    public void LeaseTimeToLive_ShouldCallLeaseTimeToLive_WithCorrectParameters()
    {
        // Arrange
        var leaseId = 12345L;
        var withKeys = true;
        var leaseTimeToLiveResponse = new LeaseTimeToLiveResponse { ID = leaseId, TTL = 10 };
        _mockLeaseClient
            .Setup(x => x.LeaseTimeToLive(
                It.IsAny<LeaseTimeToLiveRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(leaseTimeToLiveResponse);

        // Act
        var result = _client.LeaseTimeToLive(new LeaseTimeToLiveRequest { ID = leaseId, Keys = withKeys });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseTimeToLive(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == leaseId && r.Keys == withKeys),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseTimeToLiveResponse, result);
    }

    [Fact]
    public async Task LeaseTimeToLiveAsync_ShouldCallLeaseTimeToLiveAsync_WithCorrectParameters()
    {
        // Arrange
        var leaseId = 12345L;
        var withKeys = true;
        var leaseTimeToLiveResponse = new LeaseTimeToLiveResponse { ID = leaseId, TTL = 10 };
        var asyncCall = AsyncUnaryCallFactory.Create(leaseTimeToLiveResponse);
        _mockLeaseClient
            .Setup(x => x.LeaseTimeToLiveAsync(
                It.IsAny<LeaseTimeToLiveRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncCall);

        // Act
        var result = await _client.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest { ID = leaseId, Keys = withKeys });

        // Assert
        _mockLeaseClient.Verify(x => x.LeaseTimeToLiveAsync(
            It.Is<LeaseTimeToLiveRequest>(r => r.ID == leaseId && r.Keys == withKeys),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(leaseTimeToLiveResponse, result);
    }

    [Fact]
    public async Task LeaseKeepAlive_ShouldCallLeaseKeepAlive_WithCorrectParameters()
    {
        // Arrange
        var leaseId = 12345L;
        
        // Simply verify the correct method is called
        var leaseKeepAliveCalled = false;
        
        _mockLeaseClient
            .Setup(x => x.LeaseKeepAlive(
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => { leaseKeepAliveCalled = true; })
            .Returns(() => null);
        
        // Act - We expect an exception since we're returning null
        var cts = new CancellationTokenSource(50); // Very short timeout for testing
        await Assert.ThrowsAsync<NullReferenceException>(() => {
            return Task.Run(() => _client.LeaseKeepAlive(cts, leaseId));
        });
        
        // Assert the method was called
        Assert.True(leaseKeepAliveCalled, "LeaseKeepAlive method was not called");
    }
}
