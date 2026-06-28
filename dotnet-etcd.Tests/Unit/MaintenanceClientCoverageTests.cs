using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class MaintenanceClientCoverageTests
{
    private const string MaintenanceClientField = "_maintenanceClient";

    private static EtcdClient CreateClientWith(Mock<Maintenance.MaintenanceClient> mock)
    {
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, MaintenanceClientField);
        return client;
    }

    // ----- Alarm -----

    [Fact]
    public void Alarm_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.Alarm(
                It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AlarmResponse());

        var client = CreateClientWith(mock);
        var request = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.Nospace
        };

        var result = client.Alarm(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.Alarm(
            It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlarmAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.AlarmAsync(
                It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AlarmResponse()));

        var client = CreateClientWith(mock);
        var request = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.Nospace
        };

        var result = await client.AlarmAsync(request, new Metadata(), DateTime.UtcNow.AddSeconds(10),
            CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.AlarmAsync(
            It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Status -----

    [Fact]
    public void Status_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.Status(
                It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new StatusResponse { Version = "3.5.0" });

        var client = CreateClientWith(mock);

        var result = client.Status(new StatusRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal("3.5.0", result.Version);
        mock.Verify(x => x.Status(
            It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StatusAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.StatusAsync(
                It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new StatusResponse { Version = "3.5.0" }));

        var client = CreateClientWith(mock);

        var result = await client.StatusAsync(new StatusRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal("3.5.0", result.Version);
        mock.Verify(x => x.StatusAsync(
            It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Defragment -----

    [Fact]
    public void Defragment_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.Defragment(
                It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DefragmentResponse());

        var client = CreateClientWith(mock);

        var result = client.Defragment(new DefragmentRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.Defragment(
            It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DefragmentAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.DefragmentAsync(
                It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new DefragmentResponse()));

        var client = CreateClientWith(mock);

        var result = await client.DefragmentAsync(new DefragmentRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.DefragmentAsync(
            It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- Hash -----

    [Fact]
    public void Hash_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.Hash(
                It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new HashResponse { Hash = 999 });

        var client = CreateClientWith(mock);

        var result = client.Hash(new HashRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal(999u, result.Hash);
        mock.Verify(x => x.Hash(
            It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HashAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.HashAsync(
                It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new HashResponse { Hash = 999 }));

        var client = CreateClientWith(mock);

        var result = await client.HashAsync(new HashRequest(), new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal(999u, result.Hash);
        mock.Verify(x => x.HashAsync(
            It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- HashKV -----

    [Fact]
    public void HashKV_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.HashKV(
                It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new HashKVResponse { Hash = 4242 });

        var client = CreateClientWith(mock);

        var result = client.HashKV(new HashKVRequest { Revision = 10 }, new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal(4242u, result.Hash);
        mock.Verify(x => x.HashKV(
            It.Is<HashKVRequest>(r => r.Revision == 10), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HashKVAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.HashKVAsync(
                It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new HashKVResponse { Hash = 4242 }));

        var client = CreateClientWith(mock);

        var result = await client.HashKVAsync(new HashKVRequest { Revision = 10 }, new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.Equal(4242u, result.Hash);
        mock.Verify(x => x.HashKVAsync(
            It.Is<HashKVRequest>(r => r.Revision == 10), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- MoveLeader -----

    [Fact]
    public void MoveLeader_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.MoveLeader(
                It.IsAny<MoveLeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new MoveLeaderResponse());

        var client = CreateClientWith(mock);

        var result = client.MoveLeader(new MoveLeaderRequest { TargetID = 8 }, new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MoveLeader(
            It.Is<MoveLeaderRequest>(r => r.TargetID == 8), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveLeaderAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.MoveLeaderAsync(
                It.IsAny<MoveLeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new MoveLeaderResponse()));

        var client = CreateClientWith(mock);

        var result = await client.MoveLeaderAsync(new MoveLeaderRequest { TargetID = 8 }, new Metadata(),
            DateTime.UtcNow.AddSeconds(10), CancellationToken.None);

        Assert.NotNull(result);
        mock.Verify(x => x.MoveLeaderAsync(
            It.Is<MoveLeaderRequest>(r => r.TargetID == 8), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
