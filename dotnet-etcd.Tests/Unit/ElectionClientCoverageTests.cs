using dotnet_etcd.Tests.Infrastructure;
using Grpc.Core;
using Moq;
using V3Electionpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ElectionClientCoverageTests
{
    // ---------------------------------------------------------------------
    // Campaign
    // ---------------------------------------------------------------------

    [Fact]
    public void Campaign_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CampaignResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Campaign(new CampaignRequest());

        // Assert
        mockElectionClient.Verify(x => x.Campaign(
            It.IsAny<CampaignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Campaign_WithNameAndValue_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CampaignResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Campaign("test-election", "test-value");

        // Assert
        mockElectionClient.Verify(x => x.Campaign(
            It.IsAny<CampaignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CampaignAsync_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.CampaignAsync(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new CampaignResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.CampaignAsync(new CampaignRequest());

        // Assert
        mockElectionClient.Verify(x => x.CampaignAsync(
            It.IsAny<CampaignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CampaignAsync_WithNameAndValue_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.CampaignAsync(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new CampaignResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.CampaignAsync("test-election", "test-value");

        // Assert
        mockElectionClient.Verify(x => x.CampaignAsync(
            It.IsAny<CampaignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Proclaim
    // ---------------------------------------------------------------------

    [Fact]
    public void Proclaim_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Proclaim(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ProclaimResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Proclaim(new ProclaimRequest { Leader = new LeaderKey() });

        // Assert
        mockElectionClient.Verify(x => x.Proclaim(
            It.IsAny<ProclaimRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Proclaim_WithLeaderAndValue_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Proclaim(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ProclaimResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Proclaim(new LeaderKey(), "test-value");

        // Assert
        mockElectionClient.Verify(x => x.Proclaim(
            It.IsAny<ProclaimRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ProclaimAsync_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.ProclaimAsync(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new ProclaimResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.ProclaimAsync(new ProclaimRequest { Leader = new LeaderKey() });

        // Assert
        mockElectionClient.Verify(x => x.ProclaimAsync(
            It.IsAny<ProclaimRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ProclaimAsync_WithLeaderAndValue_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.ProclaimAsync(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new ProclaimResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.ProclaimAsync(new LeaderKey(), "test-value");

        // Assert
        mockElectionClient.Verify(x => x.ProclaimAsync(
            It.IsAny<ProclaimRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Leader
    // ---------------------------------------------------------------------

    [Fact]
    public void Leader_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaderResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Leader(new LeaderRequest());

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.IsAny<LeaderRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Leader_WithName_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaderResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Leader("test-election");

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.IsAny<LeaderRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task LeaderAsync_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.LeaderAsync(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaderResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.LeaderAsync(new LeaderRequest());

        // Assert
        mockElectionClient.Verify(x => x.LeaderAsync(
            It.IsAny<LeaderRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task LeaderAsync_WithName_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.LeaderAsync(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LeaderResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.LeaderAsync("test-election");

        // Assert
        mockElectionClient.Verify(x => x.LeaderAsync(
            It.IsAny<LeaderRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Resign
    // ---------------------------------------------------------------------

    [Fact]
    public void Resign_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Resign(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ResignResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Resign(new ResignRequest { Leader = new LeaderKey() });

        // Assert
        mockElectionClient.Verify(x => x.Resign(
            It.IsAny<ResignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Resign_WithLeader_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Resign(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ResignResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        client.Resign(new LeaderKey());

        // Assert
        mockElectionClient.Verify(x => x.Resign(
            It.IsAny<ResignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ResignAsync_WithRequest_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.ResignAsync(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new ResignResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.ResignAsync(new ResignRequest { Leader = new LeaderKey() });

        // Assert
        mockElectionClient.Verify(x => x.ResignAsync(
            It.IsAny<ResignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ResignAsync_WithLeader_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.ResignAsync(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new ResignResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        await client.ResignAsync(new LeaderKey());

        // Assert
        mockElectionClient.Verify(x => x.ResignAsync(
            It.IsAny<ResignRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
