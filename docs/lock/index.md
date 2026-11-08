# Lock Operations

This page documents how to use distributed locks in etcd using the `dotnet-etcd` client.

## Overview

etcd provides a distributed locking mechanism that allows multiple clients to coordinate access to shared resources. The lock service in etcd is built on top of the key-value store and lease system.

## Locks are held for as long as their lease

A lock in etcd is not held until you release it, it is held until its **lease** dies. That is what
makes the lock safe: if the process holding it crashes or is partitioned away, the lease stops being
renewed, it expires, and the lock is released for someone else. It is also what you have to take
responsibility for: the lease is yours to grant and yours to keep alive.

So every lock involves three things, not one:

1. grant a lease,
2. keep it alive for as long as you hold the lock,
3. pass its id to `Lock`.

> **Do not call `Lock(name)` without a lease.** That overload is obsolete. etcd answers an unset
> lease by creating a session of its own with a 60 second TTL and immediately orphaning it, so
> nothing renews it: the lock is released about 60 seconds later while your code is still running
> and still holds a `LockResponse` that looks perfectly valid. A second caller can then acquire the
> same lock. Callers waiting on that lock fail after the same interval with
> `mutex: session is expired`.

## Acquiring a Lock

```csharp
// 1. Grant a lease. Its TTL is how long the lock survives if this process stops responding.
var lease = client.LeaseGrant(new LeaseGrantRequest { TTL = 10 });

// 2. Keep it alive. The token is cancelled if the lease is lost, so it doubles as the signal
//    that you no longer hold the lock.
var lockLost = new CancellationTokenSource();
_ = client.LeaseKeepAlive(lockLost, lease.ID, keepAliveTimeout: 3000);

// 3. Acquire the lock with that lease.
var lockResponse = client.Lock("my-lock", lease.ID);

string lockKey = lockResponse.Key.ToStringUtf8();
Console.WriteLine($"Lock acquired with key: {lockKey}");
```

If the lock is already held by another client, `Lock` blocks until the lock is available or the
operation times out.

## Acquiring a Lock with Timeout

You can specify a timeout for acquiring a lock:

```csharp
try
{
    // Try to acquire a lock with a 5-second timeout
    var lockResponse = client.Lock(
        "my-lock",
        lease.ID,
        headers: null,
        deadline: DateTime.UtcNow.AddSeconds(5), // 5-second timeout
        cancellationToken: default
    );
    
    string lockKey = lockResponse.Key.ToStringUtf8();
    Console.WriteLine($"Lock acquired with key: {lockKey}");
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
{
    Console.WriteLine("Failed to acquire lock within the timeout period");
}
```

## Acquiring a Lock Asynchronously

You can acquire a lock asynchronously:

```csharp
// Acquire a lock asynchronously
var lockResponse = await client.LockAsync("my-lock", lease.ID);
string lockKey = lockResponse.Key.ToStringUtf8();
Console.WriteLine($"Lock acquired with key: {lockKey}");
```

## Releasing a Lock

To release a lock, use the `Unlock` method with the lock key:

```csharp
// Acquire a lock
var lockResponse = client.Lock("my-lock", lease.ID);
string lockKey = lockResponse.Key.ToStringUtf8();

try
{
    // Use the lock
    Console.WriteLine("Lock acquired, performing work...");
    
    // Simulate work
    Thread.Sleep(2000);
}
finally
{
    // Release the lock
    client.Unlock(lockKey);
    Console.WriteLine("Lock released");
}
```

## Releasing a Lock Asynchronously

You can release a lock asynchronously:

```csharp
// Acquire a lock asynchronously
var lockResponse = await client.LockAsync("my-lock", lease.ID);
string lockKey = lockResponse.Key.ToStringUtf8();

try
{
    // Use the lock
    Console.WriteLine("Lock acquired, performing work...");
    
    // Simulate work
    await Task.Delay(2000);
}
finally
{
    // Release the lock asynchronously
    await client.UnlockAsync(lockKey);
    Console.WriteLine("Lock released");
}
```

## Using Locks with a Using Statement

You can use the `using` statement to ensure that locks are properly released:

```csharp
// Helper method to create a disposable lock
public static IDisposable AcquireLock(EtcdClient client, string lockName, long leaseId)
{
    var lockResponse = client.Lock(lockName, leaseId);
    string lockKey = lockResponse.Key.ToStringUtf8();
    
    return new DisposableLock(client, lockKey);
}

// Disposable lock class
public class DisposableLock : IDisposable
{
    private readonly EtcdClient _client;
    private readonly string _lockKey;
    
    public DisposableLock(EtcdClient client, string lockKey)
    {
        _client = client;
        _lockKey = lockKey;
    }
    
    public void Dispose()
    {
        _client.Unlock(_lockKey);
    }
}

// Usage
using (var lockObj = AcquireLock(client, "my-lock", lease.ID))
{
    // The lock is held here
    Console.WriteLine("Lock acquired, performing work...");
    
    // Simulate work
    Thread.Sleep(2000);
    
    // The lock will be automatically released when the using block exits
}
Console.WriteLine("Lock released");
```

## Implementing a Mutex Pattern

You can implement a mutex pattern to coordinate access to a shared resource:

```csharp
public class EtcdMutex
{
    private readonly EtcdClient _client;
    private readonly string _mutexName;
    private readonly long _leaseId;
    private string _lockKey;
    
    public EtcdMutex(EtcdClient client, string mutexName, long leaseId)
    {
        _client = client;
        _mutexName = mutexName;
        _leaseId = leaseId;
    }
    
    public void Lock()
    {
        var lockResponse = _client.Lock(_mutexName, _leaseId);
        _lockKey = lockResponse.Key.ToStringUtf8();
    }
    
    public async Task LockAsync()
    {
        var lockResponse = await _client.LockAsync(_mutexName, _leaseId);
        _lockKey = lockResponse.Key.ToStringUtf8();
    }
    
    public void Unlock()
    {
        if (_lockKey != null)
        {
            _client.Unlock(_lockKey);
            _lockKey = null;
        }
    }
    
    public async Task UnlockAsync()
    {
        if (_lockKey != null)
        {
            await _client.UnlockAsync(_lockKey);
            _lockKey = null;
        }
    }
}

// Usage
var mutex = new EtcdMutex(client, "my-mutex", lease.ID);

// Acquire the lock
mutex.Lock();

try
{
    // Use the shared resource
    Console.WriteLine("Lock acquired, performing work...");
    
    // Simulate work
    Thread.Sleep(2000);
}
finally
{
    // Release the lock
    mutex.Unlock();
    Console.WriteLine("Lock released");
}
```

## Implementing a Read-Write Lock Pattern

You can implement a read-write lock pattern to allow multiple readers but only one writer:

```csharp
public class EtcdReadWriteLock
{
    private readonly EtcdClient _client;
    private readonly string _lockName;
    private readonly long _leaseId;
    
    public EtcdReadWriteLock(EtcdClient client, string lockName, long leaseId)
    {
        _client = client;
        _lockName = lockName;
        _leaseId = leaseId;
    }
    
    public string AcquireReadLock()
    {
        // Use a prefix for read locks
        var lockResponse = _client.Lock($"{_lockName}/read", _leaseId);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public async Task<string> AcquireReadLockAsync()
    {
        var lockResponse = await _client.LockAsync($"{_lockName}/read", _leaseId);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public string AcquireWriteLock()
    {
        // Use a prefix for write locks
        var lockResponse = _client.Lock($"{_lockName}/write", _leaseId);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public async Task<string> AcquireWriteLockAsync()
    {
        var lockResponse = await _client.LockAsync($"{_lockName}/write", _leaseId);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public void ReleaseLock(string lockKey)
    {
        _client.Unlock(lockKey);
    }
    
    public async Task ReleaseLockAsync(string lockKey)
    {
        await _client.UnlockAsync(lockKey);
    }
}

// Usage
var rwLock = new EtcdReadWriteLock(client, "my-rw-lock", lease.ID);

// Acquire a read lock
string readLockKey = rwLock.AcquireReadLock();
try
{
    // Read from the shared resource
    Console.WriteLine("Read lock acquired, reading data...");
}
finally
{
    // Release the read lock
    rwLock.ReleaseLock(readLockKey);
}

// Acquire a write lock
string writeLockKey = rwLock.AcquireWriteLock();
try
{
    // Write to the shared resource
    Console.WriteLine("Write lock acquired, writing data...");
}
finally
{
    // Release the write lock
    rwLock.ReleaseLock(writeLockKey);
}
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for lock operations
- [Lease Operations](../lease/index.md) - Working with leases
- [Election Operations](../election/index.md) - Using etcd for leader election
