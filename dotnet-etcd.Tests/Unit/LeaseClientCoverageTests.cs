using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the Lease client unary methods.
///     Every public UNARY Lease method + overload is exercised in both sync and async form.
///     NOTE: LeaseKeepAlive is a duplex-streaming method and is intentionally NOT covered here.
/// </summary>
[Trait("Category", "Unit")]
public class LeaseClientCoverageTests
{
    // ---------------------------------------------------------------------
    // LeaseGrant (sync)
    // ---------------------------------------------------------------------

    [Fact]
    public void LeaseGrant_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseGrantResponse { ID = 100, TTL = 10 });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var result = client.LeaseGrant(new LeaseGrantRequest { TTL = 10 });

        Assert.Equal(100, result.ID);
        mockLeaseClient.Verify(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void LeaseGrant_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseGrantResponse { ID = 200 });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var result = client.LeaseGrant(new LeaseGrantRequest { TTL = 5 }, headers, deadline,
            CancellationToken.None);

        Assert.Equal(200, result.ID);
        mockLeaseClient.Verify(x => x.LeaseGrant(It.IsAny<LeaseGrantRequest>(), headers, deadline,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseGrantAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseGrantAsync_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseGrantResponse { ID = 300 }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var result = await client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 10 });

        Assert.Equal(300, result.ID);
        mockLeaseClient.Verify(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaseGrantAsync_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseGrantResponse { ID = 400 }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var result = await client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 5 }, headers, deadline,
            CancellationToken.None);

        Assert.Equal(400, result.ID);
        mockLeaseClient.Verify(x => x.LeaseGrantAsync(It.IsAny<LeaseGrantRequest>(), headers, deadline,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseRevoke (sync)
    // ---------------------------------------------------------------------

    [Fact]
    public void LeaseRevoke_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseRevoke(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseRevokeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        client.LeaseRevoke(new LeaseRevokeRequest { ID = 100 });

        mockLeaseClient.Verify(x => x.LeaseRevoke(It.Is<LeaseRevokeRequest>(r => r.ID == 100),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void LeaseRevoke_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseRevoke(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaseRevokeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        client.LeaseRevoke(new LeaseRevokeRequest { ID = 200 }, headers, deadline, CancellationToken.None);

        mockLeaseClient.Verify(x => x.LeaseRevoke(It.Is<LeaseRevokeRequest>(r => r.ID == 200),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseRevokeAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseRevokeAsync_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseRevokeAsync(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseRevokeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        await client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = 300 });

        mockLeaseClient.Verify(x => x.LeaseRevokeAsync(It.Is<LeaseRevokeRequest>(r => r.ID == 300),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaseRevokeAsync_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseRevokeAsync(It.IsAny<LeaseRevokeRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseRevokeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        await client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = 400 }, headers, deadline,
            CancellationToken.None);

        mockLeaseClient.Verify(x => x.LeaseRevokeAsync(It.Is<LeaseRevokeRequest>(r => r.ID == 400),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseTimeToLive (sync)
    // ---------------------------------------------------------------------

    [Fact]
    public void LeaseTimeToLive_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseTimeToLive(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new LeaseTimeToLiveResponse { ID = 100, TTL = 7 });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var result = client.LeaseTimeToLive(new LeaseTimeToLiveRequest { ID = 100 });

        Assert.Equal(100, result.ID);
        Assert.Equal(7, result.TTL);
        mockLeaseClient.Verify(x => x.LeaseTimeToLive(It.Is<LeaseTimeToLiveRequest>(r => r.ID == 100),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void LeaseTimeToLive_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseTimeToLive(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new LeaseTimeToLiveResponse { ID = 200, TTL = 9 });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var result = client.LeaseTimeToLive(new LeaseTimeToLiveRequest { ID = 200, Keys = true }, headers,
            deadline, CancellationToken.None);

        Assert.Equal(200, result.ID);
        Assert.Equal(9, result.TTL);
        mockLeaseClient.Verify(x => x.LeaseTimeToLive(It.Is<LeaseTimeToLiveRequest>(r => r.ID == 200),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseTimeToLiveAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseTimeToLiveAsync_WithRequestOnly_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseTimeToLiveAsync(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseTimeToLiveResponse { ID = 300, TTL = 4 }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var result = await client.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest { ID = 300 });

        Assert.Equal(300, result.ID);
        Assert.Equal(4, result.TTL);
        mockLeaseClient.Verify(x => x.LeaseTimeToLiveAsync(It.Is<LeaseTimeToLiveRequest>(r => r.ID == 300),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaseTimeToLiveAsync_WithAllParameters_ShouldCallGrpcClient()
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseTimeToLiveAsync(It.IsAny<LeaseTimeToLiveRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaseTimeToLiveResponse { ID = 400, TTL = 6 }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var result = await client.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest { ID = 400, Keys = true },
            headers, deadline, CancellationToken.None);

        Assert.Equal(400, result.ID);
        Assert.Equal(6, result.TTL);
        mockLeaseClient.Verify(x => x.LeaseTimeToLiveAsync(It.Is<LeaseTimeToLiveRequest>(r => r.ID == 400),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }
}
