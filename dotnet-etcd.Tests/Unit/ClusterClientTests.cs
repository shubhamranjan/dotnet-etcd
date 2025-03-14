using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ClusterClientTests
{
    [Fact]
    public void MemberAdd_ShouldCallGrpcClient()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();

        var expectedResponse = new MemberAddResponse
        {
            Member = new Member
            {
                ID = 123UL,
                Name = "new-member"
            }
        };

        mockClusterClient
            .Setup(x => x.MemberAdd(It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Cluster client
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var request = new MemberAddRequest();
        request.PeerURLs.Add("http://localhost:2380");
        var result = client.MemberAdd(request);

        // Assert
        mockClusterClient.Verify(x => x.MemberAdd(
            It.Is<MemberAddRequest>(r => r.PeerURLs.Contains("http://localhost:2380")),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Member.ID);
        Assert.Equal("new-member", result.Member.Name);
    }

    [Fact]
    public async Task MemberAddAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();

        var expectedResponse = new MemberAddResponse
        {
            Member = new Member
            {
                ID = 123UL,
                Name = "new-member"
            }
        };

        var asyncResponse = new AsyncUnaryCall<MemberAddResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockClusterClient
            .Setup(x => x.MemberAddAsync(It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Cluster client
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var request = new MemberAddRequest();
        request.PeerURLs.Add("http://localhost:2380");
        var result = await client.MemberAddAsync(request);

        // Assert
        mockClusterClient.Verify(x => x.MemberAddAsync(
            It.Is<MemberAddRequest>(r => r.PeerURLs.Contains("http://localhost:2380")),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Member.ID);
        Assert.Equal("new-member", result.Member.Name);
    }

    [Fact]
    public void MemberList_ShouldCallGrpcClient()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();

        var expectedResponse = new MemberListResponse();
        expectedResponse.Members.Add(new Member
        {
            ID = 123UL,
            Name = "member1"
        });
        expectedResponse.Members.Add(new Member
        {
            ID = 456UL,
            Name = "member2"
        });

        mockClusterClient
            .Setup(x => x.MemberList(It.IsAny<MemberListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Cluster client
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var request = new MemberListRequest();
        var result = client.MemberList(request);

        // Assert
        mockClusterClient.Verify(x => x.MemberList(
            It.IsAny<MemberListRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(2, result.Members.Count);
        Assert.Equal(123UL, result.Members[0].ID);
        Assert.Equal("member1", result.Members[0].Name);
        Assert.Equal(456UL, result.Members[1].ID);
        Assert.Equal("member2", result.Members[1].Name);
    }

    [Fact]
    public void MemberRemove_ShouldCallGrpcClient()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();

        var expectedResponse = new MemberRemoveResponse();
        expectedResponse.Members.Add(new Member
        {
            ID = 456UL,
            Name = "member2"
        });

        mockClusterClient
            .Setup(x => x.MemberRemove(It.IsAny<MemberRemoveRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Cluster client
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var request = new MemberRemoveRequest { ID = 123UL };
        var result = client.MemberRemove(request);

        // Assert
        mockClusterClient.Verify(x => x.MemberRemove(
            It.Is<MemberRemoveRequest>(r => r.ID == 123UL),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Single(result.Members);
        Assert.Equal(456UL, result.Members[0].ID);
        Assert.Equal("member2", result.Members[0].Name);
    }

    [Fact]
    public void MemberUpdate_ShouldCallGrpcClient()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();

        var expectedResponse = new MemberUpdateResponse();
        expectedResponse.Members.Add(new Member
        {
            ID = 123UL,
            Name = "member1",
            PeerURLs = { "http://localhost:2380" }
        });

        mockClusterClient
            .Setup(x => x.MemberUpdate(It.IsAny<MemberUpdateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Cluster client
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var request = new MemberUpdateRequest { ID = 123UL };
        request.PeerURLs.Add("http://localhost:2380");
        var result = client.MemberUpdate(request);

        // Assert
        mockClusterClient.Verify(x => x.MemberUpdate(
            It.Is<MemberUpdateRequest>(r => r.ID == 123UL && r.PeerURLs.Contains("http://localhost:2380")),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Single(result.Members);
        Assert.Equal(123UL, result.Members[0].ID);
        Assert.Equal("member1", result.Members[0].Name);
        Assert.Contains("http://localhost:2380", result.Members[0].PeerURLs);
    }
}