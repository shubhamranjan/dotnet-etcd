# Lease Operations

This page documents how to work with leases in etcd using the `dotnet-etcd` client.

## Overview

Leases in etcd are a way to associate a time-to-live (TTL) with keys. When a lease expires, all keys attached to it are automatically deleted. This is useful for implementing features like service discovery, distributed locks, and leader election.

## Creating a Lease

To create a lease, use the `LeaseGrant` method:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);

// Get the lease ID
long leaseId = leaseResponse.ID;
```

## Attaching Keys to a Lease

To attach a key to a lease, use the `Put` method with the lease ID:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Attach a key to the lease
var putRequest = new PutRequest
{
    Key = ByteString.CopyFromUtf8("my-key"),
    Value = ByteString.CopyFromUtf8("my-value"),
    Lease = leaseId
};

client.Put(putRequest);

// Alternative simplified approach
client.PutWithLease("my-key", "my-value", leaseId);
```

When the lease expires, the key will be automatically deleted.

## Keeping a Lease Alive

To prevent a lease from expiring, you can periodically send keep-alive requests:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Keep the lease alive
var keepAliveResponse = client.LeaseKeepAlive(leaseId);
```

For continuous keep-alive, you can use a background task:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Start a background task to keep the lease alive
var cancellationTokenSource = new CancellationTokenSource();
var keepAliveTask = Task.Run(async () =>
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        try
        {
            await client.LeaseKeepAliveAsync(leaseId);
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error keeping lease alive: {ex.Message}");
            break;
        }
    }
}, cancellationTokenSource.Token);

// Later, when you want to stop keeping the lease alive
cancellationTokenSource.Cancel();
```

## Retrieving Lease Information

To get information about a lease, use the `LeaseTimeToLive` method:

```csharp
// Get lease information
var leaseInfo = client.LeaseTimeToLive(leaseId);

// Access lease properties
Console.WriteLine($"Lease ID: {leaseInfo.ID}");
Console.WriteLine($"TTL: {leaseInfo.TTL} seconds");
Console.WriteLine($"Granted TTL: {leaseInfo.GrantedTTL} seconds");

// Get keys attached to the lease
if (leaseInfo.Keys.Count > 0)
{
    Console.WriteLine("Keys attached to this lease:");
    foreach (var key in leaseInfo.Keys)
    {
        Console.WriteLine(key.ToStringUtf8());
    }
}
```

## Revoking a Lease

To manually revoke a lease, use the `LeaseRevoke` method:

```csharp
// Revoke a lease
client.LeaseRevoke(leaseId);
```

This will immediately delete all keys attached to the lease.

## Asynchronous Lease Operations

All lease operations have asynchronous counterparts:

```csharp
// Create a lease asynchronously
var leaseResponse = await client.LeaseGrantAsync(10);
long leaseId = leaseResponse.ID;

// Keep the lease alive asynchronously
await client.LeaseKeepAliveAsync(leaseId);

// Get lease information asynchronously
var leaseInfo = await client.LeaseTimeToLiveAsync(leaseId);

// Revoke a lease asynchronously
await client.LeaseRevokeAsync(leaseId);
```

## Common Use Cases

### Service Discovery

Leases can be used for service discovery by having services register themselves with a lease:

```csharp
// Service registration with lease
var leaseResponse = client.LeaseGrant(30); // 30-second TTL
long leaseId = leaseResponse.ID;

// Register service
string serviceKey = $"services/{serviceId}";
string serviceInfo = JsonSerializer.Serialize(new
{
    Host = "localhost",
    Port = 8080,
    Version = "1.0.0"
});

client.PutWithLease(serviceKey, serviceInfo, leaseId);

// Keep the lease alive in the background
StartKeepAliveTask(client, leaseId);

// Service discovery
var services = client.GetRange("services/");
foreach (var kvp in services.Kvs)
{
    string serviceId = kvp.Key.ToStringUtf8().Replace("services/", "");
    string serviceInfo = kvp.Value.ToStringUtf8();
    Console.WriteLine($"Found service: {serviceId} - {serviceInfo}");
}
```

### Distributed Locks with Leases

Leases can be used to implement distributed locks with automatic release:

```csharp
// Acquire lock with lease
var leaseResponse = client.LeaseGrant(10); // 10-second TTL
long leaseId = leaseResponse.ID;

string lockKey = "locks/my-resource";
bool lockAcquired = false;

try
{
    // Try to acquire the lock
    var txnResponse = client.Transaction(
        new[] { Compare.Create(lockKey, CompareTarget.Version, CompareResult.Equal, 0) },
        new[] { Op.Put(lockKey, "locked", leaseId) },
        new[] { Op.Get(lockKey) }
    );

    lockAcquired = txnResponse.Succeeded;

    if (lockAcquired)
    {
        Console.WriteLine("Lock acquired");
        
        // Start keep-alive to maintain the lock
        StartKeepAliveTask(client, leaseId);
        
        // Do work with the lock
        // ...
    }
    else
    {
        Console.WriteLine("Failed to acquire lock");
    }
}
finally
{
    if (lockAcquired)
    {
        // Release the lock by revoking the lease
        client.LeaseRevoke(leaseId);
        Console.WriteLine("Lock released");
    }
}
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for lease operations
- [Lock Operations](../lock/index.md) - Using etcd's built-in lock service
- [Election Operations](../election/index.md) - Using etcd for leader election
