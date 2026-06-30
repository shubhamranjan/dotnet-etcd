using dotnet_etcd.Tests.Infrastructure;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the duplex-streaming LeaseKeepAlive overloads.
///     A real <see cref="AsyncDuplexStreamingCall{TRequest,TResponse}" /> is built over a recording
///     request writer and a finite response reader, mirroring how TestHelper builds AsyncUnaryCall.
/// </summary>
[Trait("Category", "Unit")]
public class LeaseKeepAliveCoverageTests
{
    private static AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> BuildCall(
        RecordingClientStreamWriter<LeaseKeepAliveRequest> writer,
        IEnumerable<LeaseKeepAliveResponse> responses) =>
        new(
            writer,
            new TestAsyncStreamReader<LeaseKeepAliveResponse>(responses),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    private static EtcdClient CreateClient(
        AsyncDuplexStreamingCall<LeaseKeepAliveRequest, LeaseKeepAliveResponse> call,
        out Mock<Lease.LeaseClient> mockLeaseClient)
    {
        mockLeaseClient = new Mock<Lease.LeaseClient>();
        mockLeaseClient
            .Setup(x => x.LeaseKeepAlive(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(call);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLeaseClient.Object, "_leaseClient");
        return client;
    }

    private static LeaseKeepAliveResponse Response(long id, long ttl) =>
        new() { ID = id, TTL = ttl };

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(request, method, ct)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_Request_SingleMethod_InvokesMethodAndWritesRequest()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, new[] { Response(1, 5) });
        var client = CreateClient(call, out var mock);

        var received = new List<LeaseKeepAliveResponse>();
        await client.LeaseKeepAlive(new LeaseKeepAliveRequest { ID = 1 }, received.Add, CancellationToken.None);

        Assert.Single(received);
        Assert.Equal(1, received[0].ID);
        Assert.Contains(writer.Written, r => r.ID == 1);
        Assert.True(writer.IsCompleted);
        mock.Verify(x => x.LeaseKeepAlive(It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(request, methods[], ct)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_Request_MultipleMethods_InvokesAll()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, new[] { Response(2, 5) });
        var client = CreateClient(call, out _);

        int a = 0, b = 0;
        await client.LeaseKeepAlive(new LeaseKeepAliveRequest { ID = 2 },
            new Action<LeaseKeepAliveResponse>[] { _ => a++, _ => b++ }, CancellationToken.None);

        Assert.Equal(1, a);
        Assert.Equal(1, b);
        Assert.True(writer.IsCompleted);
    }

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(requests[], method, ct)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_RequestArray_SingleMethod_WritesAllRequests()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, new[] { Response(3, 5) });
        var client = CreateClient(call, out _);

        var received = new List<LeaseKeepAliveResponse>();
        await client.LeaseKeepAlive(
            new[] { new LeaseKeepAliveRequest { ID = 3 }, new LeaseKeepAliveRequest { ID = 4 } },
            received.Add, CancellationToken.None);

        Assert.Single(received);
        Assert.Equal(2, writer.Written.Count);
        Assert.True(writer.IsCompleted);
    }

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(requests[], methods[], ct, headers, deadline)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_RequestArray_MultipleMethods_WritesAllAndInvokesAll()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, new[] { Response(5, 5) });
        var client = CreateClient(call, out _);

        int a = 0, b = 0;
        await client.LeaseKeepAlive(
            new[] { new LeaseKeepAliveRequest { ID = 5 }, new LeaseKeepAliveRequest { ID = 6 } },
            new Action<LeaseKeepAliveResponse>[] { _ => a++, _ => b++ },
            CancellationToken.None);

        Assert.Equal(1, a);
        Assert.Equal(1, b);
        Assert.Equal(2, writer.Written.Count);
    }

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(leaseId, ct)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_LeaseId_ReturnsWhenLeaseExpired()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        // TTL == 0 means the lease is expired, so the loop completes and returns.
        var call = BuildCall(writer, new[] { Response(10, 0) });
        var client = CreateClient(call, out _);

        await client.LeaseKeepAlive(10, CancellationToken.None);

        Assert.Contains(writer.Written, r => r.ID == 10);
        Assert.True(writer.IsCompleted);
    }

    [Fact]
    public async Task LeaseKeepAlive_LeaseId_StreamEnds_ThrowsEndOfStream()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        // Empty response stream -> MoveNext returns false immediately -> EndOfStreamException.
        var call = BuildCall(writer, Array.Empty<LeaseKeepAliveResponse>());
        var client = CreateClient(call, out _);

        await Assert.ThrowsAsync<EndOfStreamException>(() => client.LeaseKeepAlive(11, CancellationToken.None));
        Assert.True(writer.IsCompleted);
    }

    // ---------------------------------------------------------------------
    // LeaseKeepAlive(cancellationTokenSource, leaseId, ...) - timer based
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LeaseKeepAlive_WithCts_ReturnsWhenLeaseExpired()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, new[] { Response(20, 0) });
        var client = CreateClient(call, out _);

        using var cts = new CancellationTokenSource();
        await client.LeaseKeepAlive(cts, 20);

        Assert.Contains(writer.Written, r => r.ID == 20);
        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public async Task LeaseKeepAlive_WithCts_StreamEnds_CompletesGracefully()
    {
        var writer = new RecordingClientStreamWriter<LeaseKeepAliveRequest>();
        var call = BuildCall(writer, Array.Empty<LeaseKeepAliveResponse>());
        var client = CreateClient(call, out _);

        using var cts = new CancellationTokenSource();
        await client.LeaseKeepAlive(cts, 21);

        Assert.True(writer.IsCompleted);
    }

    [Fact]
    public async Task LeaseKeepAlive_WithCts_NullSource_Throws()
    {
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.LeaseKeepAlive(null!, 1));
    }

    [Fact]
    public async Task LeaseKeepAlive_WithCts_NonPositiveKeepAliveTimeout_Throws()
    {
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        using var cts = new CancellationTokenSource();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.LeaseKeepAlive(cts, 1, 0));
    }

    [Fact]
    public async Task LeaseKeepAlive_WithCts_NonPositiveCommunicationTimeout_Throws()
    {
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        using var cts = new CancellationTokenSource();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.LeaseKeepAlive(cts, 1, 1000, 0));
    }
}
