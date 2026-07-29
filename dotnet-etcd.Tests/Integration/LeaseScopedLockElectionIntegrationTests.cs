using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using V3Electionpb;
using V3Lockpb;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
///     Documents what etcd does when Lock / Campaign are called without a lease, and asserts that
///     the lease-taking overloads bind ownership to the caller's own lease instead.
///     <para>
///         etcd's v3lock and v3election servers both react to an unset lease by calling
///         <c>concurrency.NewSession</c>, which grants a lease of its own with the package default
///         TTL of 60 seconds, and then immediately calling <c>Orphan()</c> on the session. Orphan
///         stops the keep-alive, so nothing ever refreshes that lease: ownership is dropped roughly
///         60 seconds after it is taken, while the caller is still running and still holding what
///         looks like a valid LockResponse / CampaignResponse.
///     </para>
///     <para>
///         These tests observe the granted lease directly rather than waiting for it to expire, so
///         they finish in milliseconds. The wall-clock behaviour is covered separately by
///         <see cref="LeaseScopedOwnershipExpiryTests" />, which is excluded from the default test
///         run because it has to sit through the full 60 second TTL.
///     </para>
/// </summary>
[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class LeaseScopedLockElectionIntegrationTests
{
    /// <summary>concurrency.defaultSessionTTL in etcd's client/v3/concurrency package.</summary>
    private const long DefaultSessionTtlSeconds = 60;

    private readonly EtcdClient _client;

    public LeaseScopedLockElectionIntegrationTests(EtcdClusterFixture fixture) =>
        _client = new EtcdClient(fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });

    // ---------------------------------------------------------------------
    // Lock
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LockAsync_WithoutLease_ShouldBindLockToAnUnrequestedSixtySecondLease()
    {
        var name = $"lock-no-lease-{Guid.NewGuid()}";

#pragma warning disable CS0618 // the point of the test is what the obsolete overload does
        var lockResponse = await _client.LockAsync(name);
#pragma warning restore CS0618

        try
        {
            var kvs = await _client.GetAsync(new RangeRequest { Key = lockResponse.Key });
            var lockKey = Assert.Single(kvs.Kvs);

            // The caller never granted a lease, yet the lock key is attached to one: the server
            // made it up. Ownership therefore has a lifetime the caller never asked for.
            Assert.NotEqual(0, lockKey.Lease);

            var ttl = await _client.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest { ID = lockKey.Lease });
            Assert.Equal(DefaultSessionTtlSeconds, ttl.GrantedTTL);
        }
        finally
        {
            await _client.UnlockAsync(new UnlockRequest { Key = lockResponse.Key });
        }
    }

    [Fact]
    public async Task LockAsync_WithLease_ShouldBindLockToTheCallersLease()
    {
        var name = $"lock-with-lease-{Guid.NewGuid()}";
        var lease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });

        var lockResponse = await _client.LockAsync(name, lease.ID);

        try
        {
            var kvs = await _client.GetAsync(new RangeRequest { Key = lockResponse.Key });
            var lockKey = Assert.Single(kvs.Kvs);

            // Ownership now lives and dies with a lease the caller controls and can keep alive.
            Assert.Equal(lease.ID, lockKey.Lease);
        }
        finally
        {
            await _client.UnlockAsync(new UnlockRequest { Key = lockResponse.Key });
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = lease.ID });
        }
    }

    [Fact]
    public async Task LockAsync_WithRevokedLease_ShouldReleaseTheLockImmediately()
    {
        var name = $"lock-revoked-lease-{Guid.NewGuid()}";
        var lease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        var lockResponse = await _client.LockAsync(name, lease.ID);

        // Revoking the lease is the fast, deterministic stand-in for it expiring.
        await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = lease.ID });

        var kvs = await _client.GetAsync(new RangeRequest { Key = lockResponse.Key });
        Assert.Empty(kvs.Kvs);

        // And the lock is genuinely free again, not merely missing a key.
        var contender = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        try
        {
            var second = await _client.LockAsync(name, contender.ID,
                deadline: DateTime.UtcNow.AddSeconds(5));
            Assert.NotEmpty(second.Key.ToByteArray());
            await _client.UnlockAsync(new UnlockRequest { Key = second.Key });
        }
        finally
        {
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = contender.ID });
        }
    }

    // ---------------------------------------------------------------------
    // Campaign
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CampaignAsync_WithoutLease_ShouldBindLeadershipToAnUnrequestedSixtySecondLease()
    {
        var name = $"election-no-lease-{Guid.NewGuid()}";

#pragma warning disable CS0618 // the point of the test is what the obsolete overload does
        var campaign = await _client.CampaignAsync(name, "candidate-a");
#pragma warning restore CS0618

        try
        {
            // Same story as Lock: leadership is attached to a lease the caller never asked for,
            // and nothing is keeping it alive.
            Assert.NotEqual(0, campaign.Leader.Lease);

            var ttl = await _client.LeaseTimeToLiveAsync(
                new LeaseTimeToLiveRequest { ID = campaign.Leader.Lease });
            Assert.Equal(DefaultSessionTtlSeconds, ttl.GrantedTTL);
        }
        finally
        {
            await _client.ResignAsync(new ResignRequest { Leader = campaign.Leader });
        }
    }

    [Fact]
    public async Task CampaignAsync_WithLease_ShouldBindLeadershipToTheCallersLease()
    {
        var name = $"election-with-lease-{Guid.NewGuid()}";
        var lease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });

        var campaign = await _client.CampaignAsync(name, lease.ID, "candidate-a");

        try
        {
            Assert.Equal(lease.ID, campaign.Leader.Lease);

            var leader = await _client.LeaderAsync(name);
            Assert.Equal("candidate-a", leader.Kv.Value.ToStringUtf8());
        }
        finally
        {
            await _client.ResignAsync(new ResignRequest { Leader = campaign.Leader });
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = lease.ID });
        }
    }

    [Fact]
    public async Task CampaignAsync_WithRevokedLease_ShouldHandLeadershipToTheNextCandidate()
    {
        var name = $"election-revoked-lease-{Guid.NewGuid()}";
        var leaderLease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        var campaign = await _client.CampaignAsync(name, leaderLease.ID, "candidate-a");

        var challengerLease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        var challenger = _client.CampaignAsync(name, challengerLease.ID, "candidate-b");

        // While the incumbent's lease is alive the challenger must not be promoted.
        Assert.NotSame(challenger, await Task.WhenAny(challenger, Task.Delay(TimeSpan.FromSeconds(1))));

        await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = leaderLease.ID });

        var promoted = await challenger.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(challengerLease.ID, promoted.Leader.Lease);

        var leader = await _client.LeaderAsync(name);
        Assert.Equal("candidate-b", leader.Kv.Value.ToStringUtf8());

        await _client.ResignAsync(new ResignRequest { Leader = promoted.Leader });
        await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = challengerLease.ID });
        _ = campaign; // the incumbent's LeaderKey is already dead with its lease
    }
}
