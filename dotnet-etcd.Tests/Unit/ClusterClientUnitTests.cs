using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

public class ClusterClientUnitTests
{
    [Fact]
    public void MemberAdd_ShouldAddMember()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();
        var expectedResponse = new MemberAddResponse
        {
            Member = new Member { ID = 1, Name = "test-member" }
        };

        mockClusterClient.Setup(x => x.MemberAdd(
                It.IsAny<MemberAddRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        var request = new MemberAddRequest
        {
            PeerURLs = { "http://localhost:2380" }
        };

        // Act
        var result = client.MemberAdd(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1UL, result.Member.ID);
        Assert.Equal("test-member", result.Member.Name);
    }

    [Fact]
    public async Task MemberAddAsync_ShouldAddMember()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();
        var expectedResponse = new MemberAddResponse
        {
            Member = new Member { ID = 1, Name = "test-member" }
        };

        mockClusterClient.Setup(x => x.MemberAddAsync(
                It.IsAny<MemberAddRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<MemberAddResponse>(
                Task.FromResult(expectedResponse),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        var request = new MemberAddRequest
        {
            PeerURLs = { "http://localhost:2380" }
        };

        // Act
        var result = await client.MemberAddAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1UL, result.Member.ID);
        Assert.Equal("test-member", result.Member.Name);
    }

    [Fact]
    public void MemberRemove_ShouldRemoveMember()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();
        var expectedResponse = new MemberRemoveResponse();

        mockClusterClient.Setup(x => x.MemberRemove(
                It.IsAny<MemberRemoveRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        var request = new MemberRemoveRequest { ID = 1 };

        // Act
        var result = client.MemberRemove(request);

        // Assert
        Assert.NotNull(result);
        mockClusterClient.Verify(x => x.MemberRemove(
            It.Is<MemberRemoveRequest>(r => r.ID == 1),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MemberList_ShouldListMembers()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();
        var expectedResponse = new MemberListResponse
        {
            Members = { new Member { ID = 1, Name = "member1" }, new Member { ID = 2, Name = "member2" } }
        };

        mockClusterClient.Setup(x => x.MemberList(
                It.IsAny<MemberListRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        // Act
        var result = client.MemberList(new MemberListRequest());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Members.Count);
        Assert.Equal("member1", result.Members[0].Name);
        Assert.Equal("member2", result.Members[1].Name);
    }

    [Fact]
    public void MemberUpdate_ShouldUpdateMember()
    {
        // Arrange
        var mockClusterClient = new Mock<Cluster.ClusterClient>();
        var expectedResponse = new MemberUpdateResponse();

        mockClusterClient.Setup(x => x.MemberUpdate(
                It.IsAny<MemberUpdateRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockClusterClient.Object, "_clusterClient");

        var request = new MemberUpdateRequest
        {
            ID = 1,
            PeerURLs = { "http://localhost:2380" }
        };

        // Act
        var result = client.MemberUpdate(request);

        // Assert
        Assert.NotNull(result);
        mockClusterClient.Verify(x => x.MemberUpdate(
            It.Is<MemberUpdateRequest>(r => r.ID == 1),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}