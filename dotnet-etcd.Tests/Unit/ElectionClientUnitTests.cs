using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Electionpb;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ElectionClientUnitTests
{
    [Fact]
    public void Campaign_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        string name = "test-election";
        long leaseId = 12345;
        string value = "test-value";
        byte[] expectedLeader = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CampaignResponse { Leader = new LeaderKey { Key = ByteString.CopyFrom(expectedLeader) } });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new CampaignRequest
        {
            Name = ByteString.CopyFromUtf8(name),
            Lease = leaseId,
            Value = ByteString.CopyFromUtf8(value)
        };
        var result = client.Campaign(request);

        // Assert
        mockElectionClient.Verify(x => x.Campaign(
            It.Is<CampaignRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name)) && 
                r.Lease == leaseId &&
                r.Value.Equals(ByteString.CopyFromUtf8(value))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedLeader, result.Leader.Key.ToByteArray());
    }

    [Fact]
    public async Task CampaignAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        string name = "test-election";
        long leaseId = 12345;
        string value = "test-value";
        byte[] expectedLeader = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        var expectedResponse = new CampaignResponse { Leader = new LeaderKey { Key = ByteString.CopyFrom(expectedLeader) } };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
            Name = ByteString.CopyFromUtf8(name),
            Lease = leaseId,
            Value = ByteString.CopyFromUtf8(value)
        };
        var result = await client.CampaignAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.CampaignAsync(
            It.Is<CampaignRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name)) && 
                r.Lease == leaseId &&
                r.Value.Equals(ByteString.CopyFromUtf8(value))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedLeader, result.Leader.Key.ToByteArray());
    }

    [Fact]
    public void Proclaim_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        byte[] leaderKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();
        string value = "test-value";

        mockElectionClient
            .Setup(x => x.Proclaim(It.IsAny<ProclaimRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ProclaimResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ProclaimRequest
        {
            Leader = new LeaderKey
            {
                Key = ByteString.CopyFrom(leaderKey)
            },
            Value = ByteString.CopyFromUtf8(value)
        };
        client.Proclaim(request);

        // Assert
        mockElectionClient.Verify(x => x.Proclaim(
            It.Is<ProclaimRequest>(r => 
                r.Leader.Key.Equals(ByteString.CopyFrom(leaderKey)) &&
                r.Value.Equals(ByteString.CopyFromUtf8(value))),
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
        byte[] leaderKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();
        string value = "test-value";
        
        var expectedResponse = new ProclaimResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
                Key = ByteString.CopyFrom(leaderKey)
            },
            Value = ByteString.CopyFromUtf8(value)
        };
        await client.ProclaimAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.ProclaimAsync(
            It.Is<ProclaimRequest>(r => 
                r.Leader.Key.Equals(ByteString.CopyFrom(leaderKey)) &&
                r.Value.Equals(ByteString.CopyFromUtf8(value))),
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
        string name = "test-election";
        byte[] expectedKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaderResponse 
            { 
                Kv = new KeyValue { Key = ByteString.CopyFrom(expectedKey) }
            });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new LeaderRequest
        {
            Name = ByteString.CopyFromUtf8(name)
        };
        var result = client.Leader(request);

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.Is<LeaderRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Kv.Key.ToByteArray());
    }

    [Fact]
    public async Task LeaderAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        string name = "test-election";
        byte[] expectedKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        var expectedResponse = new LeaderResponse 
        {
            Kv = new KeyValue { Key = ByteString.CopyFrom(expectedKey) }
        };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
            Name = ByteString.CopyFromUtf8(name)
        };
        var result = await client.LeaderAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.LeaderAsync(
            It.Is<LeaderRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Kv.Key.ToByteArray());
    }

    [Fact]
    public void Leader_Kv_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        string name = "test-election";
        byte[] expectedKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        mockElectionClient
            .Setup(x => x.Leader(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LeaderResponse 
            { 
                Kv = new KeyValue { Key = ByteString.CopyFrom(expectedKey) }
            });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new LeaderRequest
        {
            Name = ByteString.CopyFromUtf8(name)
        };
        var result = client.Leader(request);

        // Assert
        mockElectionClient.Verify(x => x.Leader(
            It.Is<LeaderRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Kv.Key.ToByteArray());
    }

    [Fact]
    public async Task LeaderAsync_Kv_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        string name = "test-election";
        byte[] expectedKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        var expectedResponse = new LeaderResponse 
        {
            Kv = new KeyValue { Key = ByteString.CopyFrom(expectedKey) }
        };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
            Name = ByteString.CopyFromUtf8(name)
        };
        var result = await client.LeaderAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.LeaderAsync(
            It.Is<LeaderRequest>(r => 
                r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(expectedKey, result.Kv.Key.ToByteArray());
    }

    [Fact]
    public void Resign_ShouldCallGrpcClient()
    {
        // Arrange
        var mockElectionClient = new Mock<Election.ElectionClient>();
        byte[] leaderKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();

        mockElectionClient
            .Setup(x => x.Resign(It.IsAny<ResignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ResignResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Election client
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        // Act
        var request = new ResignRequest
        {
            Leader = new LeaderKey
            {
                Key = ByteString.CopyFrom(leaderKey)
            }
        };
        client.Resign(request);

        // Assert
        mockElectionClient.Verify(x => x.Resign(
            It.Is<ResignRequest>(r => r.Leader.Key.Equals(ByteString.CopyFrom(leaderKey))),
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
        byte[] leaderKey = ByteString.CopyFromUtf8("test-leader").ToByteArray();
        
        var expectedResponse = new ResignResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

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
                Key = ByteString.CopyFrom(leaderKey)
            }
        };
        await client.ResignAsync(request);

        // Assert
        mockElectionClient.Verify(x => x.ResignAsync(
            It.Is<ResignRequest>(r => r.Leader.Key.Equals(ByteString.CopyFrom(leaderKey))),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
