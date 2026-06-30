using dotnet_etcd.Tests.Infrastructure;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using V3Electionpb;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the server-streaming wrappers: election Observe / ObserveAsync
///     and maintenance Snapshot. Canned responses are supplied via
///     <see cref="AsyncStreamingCallFactory.Create{T}" />.
/// </summary>
[Trait("Category", "Unit")]
public class StreamingServerCoverageTests
{
    private static EtcdClient CreateElectionClient(IEnumerable<LeaderResponse> responses,
        out Mock<Election.ElectionClient> mock)
    {
        mock = new Mock<Election.ElectionClient>();
        mock.Setup(x => x.Observe(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(AsyncStreamingCallFactory.Create(responses));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_electionClient");
        return client;
    }

    private static EtcdClient CreateMaintenanceClient(IEnumerable<SnapshotResponse> responses,
        out Mock<Maintenance.MaintenanceClient> mock)
    {
        mock = new Mock<Maintenance.MaintenanceClient>();
        mock.Setup(x => x.Snapshot(It.IsAny<SnapshotRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(AsyncStreamingCallFactory.Create(responses));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_maintenanceClient");
        return client;
    }

    // ---------------------------------------------------------------------
    // Observe (server-streaming, returns the call)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Observe_Request_ReturnsStreamingCall()
    {
        var client = CreateElectionClient(new[] { new LeaderResponse(), new LeaderResponse() }, out var mock);

        using var call = client.Observe(new LeaderRequest());
        var collected = new List<LeaderResponse>();
        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            collected.Add(call.ResponseStream.Current);
        }

        Assert.Equal(2, collected.Count);
        mock.Verify(x => x.Observe(It.IsAny<LeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Observe_Name_ReturnsStreamingCall()
    {
        var client = CreateElectionClient(new[] { new LeaderResponse() }, out _);

        using var call = client.Observe("my-election");
        var collected = new List<LeaderResponse>();
        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            collected.Add(call.ResponseStream.Current);
        }

        Assert.Single(collected);
    }

    [Fact]
    public void Observe_NullRequest_Throws()
    {
        var client = CreateElectionClient(Array.Empty<LeaderResponse>(), out _);
        Assert.Throws<ArgumentNullException>(() => client.Observe((LeaderRequest)null!));
    }

    [Fact]
    public void Observe_NullName_Throws()
    {
        var client = CreateElectionClient(Array.Empty<LeaderResponse>(), out _);
        Assert.Throws<ArgumentNullException>(() => client.Observe((string)null!));
    }

    // ---------------------------------------------------------------------
    // ObserveAsync (IAsyncEnumerable)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task ObserveAsync_Request_YieldsAllResponses()
    {
        var client = CreateElectionClient(new[] { new LeaderResponse(), new LeaderResponse(), new LeaderResponse() },
            out _);

        var collected = new List<LeaderResponse>();
        await foreach (var response in client.ObserveAsync(new LeaderRequest()))
        {
            collected.Add(response);
        }

        Assert.Equal(3, collected.Count);
    }

    [Fact]
    public async Task ObserveAsync_Name_YieldsAllResponses()
    {
        var client = CreateElectionClient(new[] { new LeaderResponse() }, out _);

        var collected = new List<LeaderResponse>();
        await foreach (var response in client.ObserveAsync("name"))
        {
            collected.Add(response);
        }

        Assert.Single(collected);
    }

    [Fact]
    public async Task ObserveAsync_NullRequest_Throws()
    {
        var client = CreateElectionClient(Array.Empty<LeaderResponse>(), out _);
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.ObserveAsync((LeaderRequest)null!))
            {
            }
        });
    }

    [Fact]
    public async Task ObserveAsync_NullName_Throws()
    {
        var client = CreateElectionClient(Array.Empty<LeaderResponse>(), out _);
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.ObserveAsync((string)null!))
            {
            }
        });
    }

    // ---------------------------------------------------------------------
    // Snapshot (server-streaming)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Snapshot_SingleMethod_InvokesForEachResponse()
    {
        var client = CreateMaintenanceClient(
            new[] { new SnapshotResponse { RemainingBytes = 2 }, new SnapshotResponse { RemainingBytes = 1 } },
            out var mock);

        var received = new List<SnapshotResponse>();
        await client.Snapshot(new SnapshotRequest(), received.Add, CancellationToken.None);

        Assert.Equal(2, received.Count);
        mock.Verify(x => x.Snapshot(It.IsAny<SnapshotRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Snapshot_MultipleMethods_InvokesAllForEachResponse()
    {
        var client = CreateMaintenanceClient(
            new[] { new SnapshotResponse { RemainingBytes = 1 } }, out _);

        int a = 0, b = 0;
        await client.Snapshot(new SnapshotRequest(),
            new Action<SnapshotResponse>[] { _ => a++, _ => b++ }, CancellationToken.None);

        Assert.Equal(1, a);
        Assert.Equal(1, b);
    }
}
