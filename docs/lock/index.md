# Lock Operations

This page documents how to use distributed locks in etcd using the `dotnet-etcd` client.

## Overview

etcd provides a distributed locking mechanism that allows multiple clients to coordinate access to shared resources. The lock service in etcd is built on top of the key-value store and lease system.

## Acquiring a Lock

To acquire a lock, use the `Lock` method:

```csharp
// Acquire a lock
var lockResponse = client.Lock("my-lock", 10); // 10-second lease

// Get the key that represents the lock
string lockKey = lockResponse.Key.ToStringUtf8();
Console.WriteLine($"Lock acquired with key: {lockKey}");
```

The `Lock` method creates a lease with the specified TTL and uses it to acquire the lock. If the lock is already held by another client, the method will block until the lock is available or the operation times out.

## Acquiring a Lock with Timeout

You can specify a timeout for acquiring a lock:

```csharp
try
{
    // Try to acquire a lock with a 5-second timeout
    var lockResponse = client.Lock(
        "my-lock",
        10, // 10-second lease
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
var lockResponse = await client.LockAsync("my-lock", 10);
string lockKey = lockResponse.Key.ToStringUtf8();
Console.WriteLine($"Lock acquired with key: {lockKey}");
```

## Releasing a Lock

To release a lock, use the `Unlock` method with the lock key:

```csharp
// Acquire a lock
var lockResponse = client.Lock("my-lock", 10);
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
var lockResponse = await client.LockAsync("my-lock", 10);
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
public static IDisposable AcquireLock(EtcdClient client, string lockName, int leaseTtl)
{
    var lockResponse = client.Lock(lockName, leaseTtl);
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
using (var lockObj = AcquireLock(client, "my-lock", 10))
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
    private readonly int _leaseTtl;
    private string _lockKey;
    
    public EtcdMutex(EtcdClient client, string mutexName, int leaseTtl = 10)
    {
        _client = client;
        _mutexName = mutexName;
        _leaseTtl = leaseTtl;
    }
    
    public void Lock()
    {
        var lockResponse = _client.Lock(_mutexName, _leaseTtl);
        _lockKey = lockResponse.Key.ToStringUtf8();
    }
    
    public async Task LockAsync()
    {
        var lockResponse = await _client.LockAsync(_mutexName, _leaseTtl);
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
var mutex = new EtcdMutex(client, "my-mutex");

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
    private readonly int _leaseTtl;
    
    public EtcdReadWriteLock(EtcdClient client, string lockName, int leaseTtl = 10)
    {
        _client = client;
        _lockName = lockName;
        _leaseTtl = leaseTtl;
    }
    
    public string AcquireReadLock()
    {
        // Use a prefix for read locks
        var lockResponse = _client.Lock($"{_lockName}/read", _leaseTtl);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public async Task<string> AcquireReadLockAsync()
    {
        var lockResponse = await _client.LockAsync($"{_lockName}/read", _leaseTtl);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public string AcquireWriteLock()
    {
        // Use a prefix for write locks
        var lockResponse = _client.Lock($"{_lockName}/write", _leaseTtl);
        return lockResponse.Key.ToStringUtf8();
    }
    
    public async Task<string> AcquireWriteLockAsync()
    {
        var lockResponse = await _client.LockAsync($"{_lockName}/write", _leaseTtl);
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
var rwLock = new EtcdReadWriteLock(client, "my-rw-lock");

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
