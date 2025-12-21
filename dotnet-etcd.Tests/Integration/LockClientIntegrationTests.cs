using Etcdserverpb;
using Google.Protobuf;
using V3Lockpb;

namespace dotnet_etcd.Tests.Integration;

[Trait("Category", "Integration")]
public class LockClientIntegrationTests
{
    [Fact]
    public async Task LockUnlockLifecycleTest()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");

        var lockName = "test-lock";

        try
        {
            // 1. First, we need a lease for the lock
            var leaseGrantRequest = new LeaseGrantRequest
            {
                TTL = 30
            };
            var leaseResponse = await client.LeaseGrantAsync(leaseGrantRequest);
            Assert.NotNull(leaseResponse);
            Assert.True(leaseResponse.ID > 0);

            var leaseId = leaseResponse.ID;

            // 2. Acquire the lock
            var lockRequest = new LockRequest
            {
                Name = ByteString.CopyFromUtf8(lockName),
                Lease = leaseId
            };
            var lockResponse = await client.LockAsync(lockRequest);
            Assert.NotNull(lockResponse);
            Assert.NotEmpty(lockResponse.Key.ToByteArray());
            var lockKey = lockResponse.Key;

            // 3. Try to acquire the lock again (should wait)
            var lockTask = Task.Run(async () =>
            {
                // Use a different client instance
                using var client2 = new EtcdClient("127.0.0.1:2379");
                var lease2Request = new LeaseGrantRequest { TTL = 30 };
                var lease2Response = await client2.LeaseGrantAsync(lease2Request);

                var lock2Request = new LockRequest
                {
                    Name = ByteString.CopyFromUtf8(lockName),
                    Lease = lease2Response.ID
                };
                return await client2.LockAsync(lock2Request);
            });

            // The task should not complete immediately as the lock is held
            var completedInTime = await Task.WhenAny(lockTask, Task.Delay(TimeSpan.FromSeconds(1))) == lockTask;
            Assert.False(completedInTime);

            // 4. Release the lock
            var unlockRequest = new UnlockRequest
            {
                Key = lockKey
            };
            await client.UnlockAsync(unlockRequest);

            // 5. Now the second lock should be acquired
            var acquiredLock = await Task.WhenAny(lockTask, Task.Delay(TimeSpan.FromSeconds(5))) == lockTask;
            Assert.True(acquiredLock);
            var secondLockResponse = await lockTask;

            Assert.NotNull(secondLockResponse);
            Assert.NotEmpty(secondLockResponse.Key.ToByteArray());

            // Clean up the second lock
            using var client2 = new EtcdClient("127.0.0.1:2379");
            var unlockRequest2 = new UnlockRequest
            {
                Key = secondLockResponse.Key
            };
            await client2.UnlockAsync(unlockRequest2);

            // Clean up the lease
            var revokeRequest = new LeaseRevokeRequest
            {
                ID = leaseId
            };
            await client.LeaseRevokeAsync(revokeRequest);
        }
        finally
        {
            client.Dispose();
        }
    }

    [Fact]
    public async Task LockWithLeaseRevocationTest()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");

        var lockName = "test-lock-lease-revocation";

        try
        {
            // 1. Create a lease for the lock
            var leaseRequest = new LeaseGrantRequest { TTL = 30 };
            var leaseResponse = await client.LeaseGrantAsync(leaseRequest);
            Assert.NotNull(leaseResponse);
            var leaseId = leaseResponse.ID;

            // 2. Acquire the lock
            var lockRequest = new LockRequest
            {
                Name = ByteString.CopyFromUtf8(lockName),
                Lease = leaseId
            };
            var lockResponse = await client.LockAsync(lockRequest);
            Assert.NotNull(lockResponse);
            var lockKey = lockResponse.Key;

            // 3. Revoke the lease
            var revokeRequest = new LeaseRevokeRequest { ID = leaseId };
            await client.LeaseRevokeAsync(revokeRequest);

            // 4. Try to acquire the lock again with a new lease
            var newLeaseRequest = new LeaseGrantRequest { TTL = 30 };
            var newLeaseResponse = await client.LeaseGrantAsync(newLeaseRequest);

            var newLockRequest = new LockRequest
            {
                Name = ByteString.CopyFromUtf8(lockName),
                Lease = newLeaseResponse.ID
            };
            var newLockResponse = await client.LockAsync(newLockRequest);

            // The lock should be acquirable now that the lease is revoked
            Assert.NotNull(newLockResponse);

            // Clean up the new lock
            var unlockRequest = new UnlockRequest { Key = newLockResponse.Key };
            await client.UnlockAsync(unlockRequest);

            // Clean up the new lease
            var newRevokeRequest = new LeaseRevokeRequest { ID = newLeaseResponse.ID };
            await client.LeaseRevokeAsync(newRevokeRequest);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}