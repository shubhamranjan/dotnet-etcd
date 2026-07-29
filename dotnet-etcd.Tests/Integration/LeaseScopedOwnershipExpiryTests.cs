using System.Diagnostics;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using V3Lockpb;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
///     The wall-clock proof that a lock taken without a lease is released while its holder is still
///     running. It has to sit through etcd's full 60 second default session TTL, so it carries its
///     own category and is excluded from both CI filters ("Category=Unit" and
///     "Category=Integration"). Run it deliberately:
///     <code>dotnet test --filter "Category=Slow"</code>
/// </summary>
[Collection("EtcdCluster")]
[Trait("Category", "Slow")]
public class LeaseScopedOwnershipExpiryTests
{
    private readonly EtcdClient _client;

    public LeaseScopedOwnershipExpiryTests(EtcdClusterFixture fixture) =>
        _client = new EtcdClient(fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });

    [Fact(Timeout = 180_000)]
    public async Task LockAsync_WithoutLease_ShouldReleaseTheLockWhileTheHolderIsStillRunning()
    {
        var name = $"lock-expiry-{Guid.NewGuid()}";

#pragma warning disable CS0618 // the point of the test is what the obsolete overload does
        var held = await _client.LockAsync(name);
#pragma warning restore CS0618

        // The holder does everything right: it never unlocks, never disposes, and its connection
        // stays open for the whole test. Nothing about it stops being alive.
        var stillHeld = await _client.GetAsync(new RangeRequest { Key = held.Key });
        Assert.Single(stillHeld.Kvs);

        await Task.Delay(TimeSpan.FromSeconds(DefaultSessionTtlSeconds + 5));

        // The lock key is gone even though the holder never released it.
        var afterTtl = await _client.GetAsync(new RangeRequest { Key = held.Key });
        Assert.Empty(afterTtl.Kvs);

        // And another caller can take the same lock: mutual exclusion is gone, not merely stale.
        var contenderLease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        try
        {
            var sw = Stopwatch.StartNew();
            var contender = await _client.LockAsync(name, contenderLease.ID,
                deadline: DateTime.UtcNow.AddSeconds(5));
            sw.Stop();

            Assert.NotEmpty(contender.Key.ToByteArray());
            Assert.NotEqual(held.Key, contender.Key);

            await _client.UnlockAsync(new UnlockRequest { Key = contender.Key });
        }
        finally
        {
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = contenderLease.ID });
        }
    }

    [Fact(Timeout = 180_000)]
    public async Task LockAsync_WithKeptAliveLease_ShouldStillHoldTheLockPastTheDefaultTtl()
    {
        // The control case: same elapsed time, same server, the only difference is that the caller
        // supplies a lease and keeps it alive.
        var name = $"lock-keepalive-{Guid.NewGuid()}";
        var lease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 10 });

        using var keepAlive = new CancellationTokenSource();
        _ = _client.LeaseKeepAlive(keepAlive, lease.ID, 3000);

        var held = await _client.LockAsync(name, lease.ID);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(DefaultSessionTtlSeconds + 5));

            // A 10 second lease has outlived a 60 second one, because this one is being refreshed.
            var afterTtl = await _client.GetAsync(new RangeRequest { Key = held.Key });
            Assert.Single(afterTtl.Kvs);

            var ttl = await _client.LeaseTimeToLiveAsync(new LeaseTimeToLiveRequest { ID = lease.ID });
            Assert.True(ttl.TTL > 0, $"lease should still be alive, TTL was {ttl.TTL}");

            // And the lock is still exclusive.
            var contenderLease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
            try
            {
                var blocked = _client.LockAsync(name, contenderLease.ID);
                Assert.NotSame(blocked, await Task.WhenAny(blocked, Task.Delay(TimeSpan.FromSeconds(3))));
            }
            finally
            {
                await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = contenderLease.ID });
            }
        }
        finally
        {
            await keepAlive.CancelAsync();
            await _client.UnlockAsync(new UnlockRequest { Key = held.Key });
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = lease.ID });
        }
    }

    /// <summary>concurrency.defaultSessionTTL in etcd's client/v3/concurrency package.</summary>
    private const int DefaultSessionTtlSeconds = 60;
}
