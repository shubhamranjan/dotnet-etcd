using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Electionpb;
using V3Lockpb;
using Lock = V3Lockpb.Lock;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Unit tests for the Lock / Campaign overloads that take an explicit lease.
///     They assert that the lease the caller supplies actually reaches the wire, which is the
///     whole point of the overloads: without it etcd substitutes a session of its own.
/// </summary>
[Trait("Category", "Unit")]
public class LeaseScopedLockElectionUnitTests
{
    private const long Lease = 7788991122334455;

    [Fact]
    public void Lock_WithLease_ShouldSendLeaseInRequest()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        client.Lock("my-lock", Lease);

        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r =>
                r.Name.Equals(ByteString.CopyFromUtf8("my-lock")) && r.Lease == Lease),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LockAsync_WithLease_ShouldSendLeaseInRequest()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        await client.LockAsync("my-lock", Lease);

        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r =>
                r.Name.Equals(ByteString.CopyFromUtf8("my-lock")) && r.Lease == Lease),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Lock_WithLeaseAndAllParameters_ShouldForwardCallOptions()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        client.Lock("my-lock", Lease, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Lease == Lease),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Campaign_WithLease_ShouldSendLeaseInRequest()
    {
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.Campaign(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CampaignResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        client.Campaign("my-election", Lease, "my-value");

        mockElectionClient.Verify(x => x.Campaign(
            It.Is<CampaignRequest>(r =>
                r.Name.Equals(ByteString.CopyFromUtf8("my-election")) &&
                r.Value.Equals(ByteString.CopyFromUtf8("my-value")) &&
                r.Lease == Lease),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CampaignAsync_WithLease_ShouldSendLeaseInRequest()
    {
        var mockElectionClient = new Mock<Election.ElectionClient>();
        mockElectionClient
            .Setup(x => x.CampaignAsync(It.IsAny<CampaignRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new CampaignResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockElectionClient.Object, "_electionClient");

        await client.CampaignAsync("my-election", Lease, "my-value");

        mockElectionClient.Verify(x => x.CampaignAsync(
            It.Is<CampaignRequest>(r =>
                r.Name.Equals(ByteString.CopyFromUtf8("my-election")) &&
                r.Value.Equals(ByteString.CopyFromUtf8("my-value")) &&
                r.Lease == Lease),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Campaign_WithLeaseAndNullName_ShouldThrowArgumentNullException() =>
        Assert.Throws<ArgumentNullException>(() =>
            TestHelper.CreateEtcdClientWithMockCallInvoker().Campaign(null, Lease, "value"));
}
