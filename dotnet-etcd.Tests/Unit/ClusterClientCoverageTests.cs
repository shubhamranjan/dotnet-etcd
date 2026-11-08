using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ClusterClientCoverageTests
{
    private const string ClusterClientField = "_clusterClient";

    private static EtcdClient CreateClientWith(Mock<Cluster.ClusterClient> mock)
    {
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, ClusterClientField);
        return client;
    }

    // ----- MemberAdd -----

    [Fact]
    public void MemberAdd_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberAdd(
                It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberAddResponse { Member = new Member { ID = 5, Name = "added" } });

        var client = CreateClientWith(mock);
        var request = new MemberAddRequest { PeerURLs = { "http://localhost:2380" } };

        var result = client.MemberAdd(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5UL, result.Member.ID);
        mock.Verify(x => x.MemberAdd(
            It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberAddAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberAddAsync(
                It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(
                new MemberAddResponse { Member = new Member { ID = 5, Name = "added" } }));

        var client = CreateClientWith(mock);
        var request = new MemberAddRequest { PeerURLs = { "http://localhost:2380" } };

        var result = await client.MemberAddAsync(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5UL, result.Member.ID);
        mock.Verify(x => x.MemberAddAsync(
            It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MemberRemove -----

    [Fact]
    public void MemberRemove_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberRemove(
                It.IsAny<MemberRemoveRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberRemoveResponse());

        var client = CreateClientWith(mock);
        var request = new MemberRemoveRequest { ID = 7 };

        var result = client.MemberRemove(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MemberRemove(
            It.Is<MemberRemoveRequest>(r => r.ID == 7), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberRemoveAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberRemoveAsync(
                It.IsAny<MemberRemoveRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberRemoveResponse()));

        var client = CreateClientWith(mock);
        var request = new MemberRemoveRequest { ID = 7 };

        var result = await client.MemberRemoveAsync(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MemberRemoveAsync(
            It.Is<MemberRemoveRequest>(r => r.ID == 7), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MemberUpdate -----

    [Fact]
    public void MemberUpdate_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberUpdate(
                It.IsAny<MemberUpdateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberUpdateResponse());

        var client = CreateClientWith(mock);
        var request = new MemberUpdateRequest { ID = 3, PeerURLs = { "http://localhost:2380" } };

        var result = client.MemberUpdate(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MemberUpdate(
            It.Is<MemberUpdateRequest>(r => r.ID == 3), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberUpdateAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberUpdateAsync(
                It.IsAny<MemberUpdateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberUpdateResponse()));

        var client = CreateClientWith(mock);
        var request = new MemberUpdateRequest { ID = 3, PeerURLs = { "http://localhost:2380" } };

        var result = await client.MemberUpdateAsync(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MemberUpdateAsync(
            It.Is<MemberUpdateRequest>(r => r.ID == 3), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MemberList -----

    [Fact]
    public void MemberList_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberList(
                It.IsAny<MemberListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberListResponse
            {
                Members = { new Member { ID = 1, Name = "m1" }, new Member { ID = 2, Name = "m2" } }
            });

        var client = CreateClientWith(mock);

        var result = client.MemberList(new MemberListRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Members.Count);
        mock.Verify(x => x.MemberList(
            It.IsAny<MemberListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberListAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberListAsync(
                It.IsAny<MemberListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberListResponse
            {
                Members = { new Member { ID = 1, Name = "m1" }, new Member { ID = 2, Name = "m2" } }
            }));

        var client = CreateClientWith(mock);

        var result = await client.MemberListAsync(new MemberListRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Members.Count);
        mock.Verify(x => x.MemberListAsync(
            It.IsAny<MemberListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MemberPromote -----

    [Fact]
    public void MemberPromote_WithRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberPromote(It.IsAny<MemberPromoteRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberPromoteResponse { Members = { new Member { ID = 9, IsLearner = false } } });

        var client = CreateClientWith(mock);

        var result = client.MemberPromote(new MemberPromoteRequest { ID = 9 });

        Assert.False(Assert.Single(result.Members).IsLearner);
        mock.Verify(x => x.MemberPromote(It.Is<MemberPromoteRequest>(r => r.ID == 9UL), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MemberPromote_WithMemberId_ShouldBuildRequest()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberPromote(It.IsAny<MemberPromoteRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberPromoteResponse());

        var client = CreateClientWith(mock);

        client.MemberPromote(42UL);

        mock.Verify(x => x.MemberPromote(It.Is<MemberPromoteRequest>(r => r.ID == 42UL), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberPromoteAsync_WithRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberPromoteAsync(It.IsAny<MemberPromoteRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberPromoteResponse()));

        var client = CreateClientWith(mock);

        await client.MemberPromoteAsync(new MemberPromoteRequest { ID = 9 });

        mock.Verify(x => x.MemberPromoteAsync(It.Is<MemberPromoteRequest>(r => r.ID == 9UL), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberPromoteAsync_WithMemberId_ShouldBuildRequest()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberPromoteAsync(It.IsAny<MemberPromoteRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberPromoteResponse()));

        var client = CreateClientWith(mock);

        await client.MemberPromoteAsync(42UL);

        mock.Verify(x => x.MemberPromoteAsync(It.Is<MemberPromoteRequest>(r => r.ID == 42UL), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MemberAddAsLearner -----

    [Fact]
    public void MemberAddAsLearner_ShouldSendLearnerFlagAndPeerUrls()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberAdd(It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MemberAddResponse { Member = new Member { ID = 3, IsLearner = true } });

        var client = CreateClientWith(mock);

        var result = client.MemberAddAsLearner(new[] { "http://learner:2380" });

        Assert.True(result.Member.IsLearner);
        mock.Verify(x => x.MemberAdd(
            It.Is<MemberAddRequest>(r => r.IsLearner && r.PeerURLs.Count == 1 && r.PeerURLs[0] == "http://learner:2380"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MemberAddAsLearnerAsync_ShouldSendLearnerFlagAndPeerUrls()
    {
        var mock = new Mock<Cluster.ClusterClient>();
        mock.Setup(x => x.MemberAddAsync(It.IsAny<MemberAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MemberAddResponse()));

        var client = CreateClientWith(mock);

        await client.MemberAddAsLearnerAsync(new[] { "http://a:2380", "http://b:2380" });

        mock.Verify(x => x.MemberAddAsync(
            It.Is<MemberAddRequest>(r => r.IsLearner && r.PeerURLs.Count == 2),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MemberAddAsLearner_WithNullPeerUrls_ShouldThrow()
    {
        var client = CreateClientWith(new Mock<Cluster.ClusterClient>());

        Assert.Throws<ArgumentNullException>(() => client.MemberAddAsLearner(null!));
    }
}
