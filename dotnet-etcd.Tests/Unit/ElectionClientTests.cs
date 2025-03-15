using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;
using V3Electionpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ElectionClientTests
{
    [Fact]
    public void Campaign_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new CampaignResponse
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            }
        };

        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new CampaignRequest
        {
            Name = ByteString.CopyFromUtf8("test-election"),
            Lease = 12345,
            Value = ByteString.CopyFromUtf8("test-value")
        };
        var result = client.Campaign(request);

        // Assert
        mockElectionClient.Verify(x => x.Campaign(
            It.Is<CampaignRequest>(r =>
                r.Name.ToStringUtf8() == "test-election" &&
                r.Lease == 12345 &&
                r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("test-election", result.Leader.Name.ToStringUtf8());
        Assert.Equal("election-key-123", result.Leader.Key.ToStringUtf8());
        Assert.Equal(100, result.Leader.Rev);
        Assert.Equal(12345, result.Leader.Lease);
    }

    [Fact]
    public void Campaign_WithStringName_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new CampaignResponse
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            }
        };

        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var result = client.Campaign("test-election", "test-value");

        // Assert
        mockElectionClient.Verify(x => x.Campaign(
            It.Is<CampaignRequest>(r =>
                r.Name.ToStringUtf8() == "test-election" &&
                r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("test-election", result.Leader.Name.ToStringUtf8());
        Assert.Equal("election-key-123", result.Leader.Key.ToStringUtf8());
        Assert.Equal(100, result.Leader.Rev);
        Assert.Equal(12345, result.Leader.Lease);
    }

    [Fact]
    public async Task CampaignAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new CampaignResponse
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            }
        };

        var asyncResponse = new AsyncUnaryCall<CampaignResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockElectionClient
            .Setup(x => x.CampaignAsync(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new CampaignRequest
        {
            Name = ByteString.CopyFromUtf8("test-election"),
            Lease = 12345,
            Value = ByteString.CopyFromUtf8("test-value")
        };
        var result = await client.CampaignAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.CampaignAsync(
            It.Is<CampaignRequest>(r =>
                r.Name.ToStringUtf8() == "test-election" &&
                r.Lease == 12345 &&
                r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("test-election", result.Leader.Name.ToStringUtf8());
        Assert.Equal("election-key-123", result.Leader.Key.ToStringUtf8());
        Assert.Equal(100, result.Leader.Rev);
        Assert.Equal(12345, result.Leader.Lease);
    }

    [Fact]
    public void Proclaim_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new ProclaimResponse();

        mockElectionClient
            .Setup(x => x.Proclaim(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ProclaimRequest
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            },
            Value = ByteString.CopyFromUtf8("new-value")
        };
        var result = client.Proclaim(request);

        // Assert
        mockElectionClient.Verify(x => x.Proclaim(
            It.Is<ProclaimRequest>(r =>
                r.Leader.Name.ToStringUtf8() == "test-election" &&
                r.Leader.Key.ToStringUtf8() == "election-key-123" &&
                r.Leader.Rev == 100 &&
                r.Leader.Lease == 12345 &&
                r.Value.ToStringUtf8() == "new-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ProclaimAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new ProclaimResponse();

        var asyncResponse = new AsyncUnaryCall<ProclaimResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockElectionClient
            .Setup(x => x.ProclaimAsync(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ProclaimRequest
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            },
            Value = ByteString.CopyFromUtf8("new-value")
        };
        var result = await client.ProclaimAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.ProclaimAsync(
            It.Is<ProclaimRequest>(r =>
                r.Leader.Name.ToStringUtf8() == "test-election" &&
                r.Leader.Key.ToStringUtf8() == "election-key-123" &&
                r.Leader.Rev == 100 &&
                r.Leader.Lease == 12345 &&
                r.Value.ToStringUtf8() == "new-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Leader_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new LeaderResponse
        {
            Kv = new KeyValue
            {
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Value = ByteString.CopyFromUtf8("test-value"),
                Lease = 12345,
                ModRevision = 100,
                CreateRevision = 50,
                Version = 1
            }
        };

        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new LeaderRequest
        {
            Name = ByteString.CopyFromUtf8("test-election")
        };
        var result = client.Leader(request);

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.Is<LeaderRequest>(r => r.Name.ToStringUtf8() == "test-election"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("election-key-123", result.Kv.Key.ToStringUtf8());
        Assert.Equal("test-value", result.Kv.Value.ToStringUtf8());
        Assert.Equal(12345, result.Kv.Lease);
        Assert.Equal(100, result.Kv.ModRevision);
    }

    [Fact]
    public void Leader_WithStringName_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new LeaderResponse
        {
            Kv = new KeyValue
            {
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Value = ByteString.CopyFromUtf8("test-value"),
                Lease = 12345,
                ModRevision = 100,
                CreateRevision = 50,
                Version = 1
            }
        };

        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var result = client.Leader("test-election");

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.Is<LeaderRequest>(r => r.Name.ToStringUtf8() == "test-election"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("election-key-123", result.Kv.Key.ToStringUtf8());
        Assert.Equal("test-value", result.Kv.Value.ToStringUtf8());
        Assert.Equal(12345, result.Kv.Lease);
        Assert.Equal(100, result.Kv.ModRevision);
    }

    [Fact]
    public async Task LeaderAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new LeaderResponse
        {
            Kv = new KeyValue
            {
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Value = ByteString.CopyFromUtf8("test-value"),
                Lease = 12345,
                ModRevision = 100,
                CreateRevision = 50,
                Version = 1
            }
        };

        var asyncResponse = new AsyncUnaryCall<LeaderResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockElectionClient
            .Setup(x => x.LeaderAsync(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new LeaderRequest
        {
            Name = ByteString.CopyFromUtf8("test-election")
        };
        var result = await client.LeaderAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.LeaderAsync(
            It.Is<LeaderRequest>(r => r.Name.ToStringUtf8() == "test-election"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("election-key-123", result.Kv.Key.ToStringUtf8());
        Assert.Equal("test-value", result.Kv.Value.ToStringUtf8());
        Assert.Equal(12345, result.Kv.Lease);
        Assert.Equal(100, result.Kv.ModRevision);
    }

    [Fact]
    public void Resign_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new ResignResponse();

        mockElectionClient
            .Setup(x => x.Resign(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ResignRequest
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            }
        };
        var result = client.Resign(request);

        // Assert
        mockElectionClient.Verify(x => x.Resign(
            It.Is<ResignRequest>(r =>
                r.Leader.Name.ToStringUtf8() == "test-election" &&
                r.Leader.Key.ToStringUtf8() == "election-key-123" &&
                r.Leader.Rev == 100 &&
                r.Leader.Lease == 12345),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ResignAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();

        var expectedResponse = new ResignResponse();

        var asyncResponse = new AsyncUnaryCall<ResignResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockElectionClient
            .Setup(x => x.ResignAsync(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ResignRequest
        {
            Leader = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("election-key-123"),
                Rev = 100,
                Lease = 12345
            }
        };
        var result = await client.ResignAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.ResignAsync(
            It.Is<ResignRequest>(r =>
                r.Leader.Name.ToStringUtf8() == "test-election" &&
                r.Leader.Key.ToStringUtf8() == "election-key-123" &&
                r.Leader.Rev == 100 &&
                r.Leader.Lease == 12345),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}