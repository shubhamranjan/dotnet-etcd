# Lock API Reference

This page provides a complete reference of all lock-related methods available in the `dotnet-etcd` client.

## Lock Methods

### Lock

Acquires a lock with a lease.

#### Lock Overloads

```csharp
// Acquire a lock with a lease TTL
public LockResponse Lock(
    string name,
    int leaseTtl,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Acquire a lock with a lease ID
public LockResponse Lock(
    string name,
    long leaseId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Lock Parameters

- `name`: The name of the lock to acquire.
- `leaseTtl`: The time-to-live in seconds for the lease associated with the lock.
- `leaseId`: The ID of an existing lease to associate with the lock.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Lock Returns

- `LockResponse`: The etcd response containing the lock key.

### LockAsync

Acquires a lock with a lease asynchronously.

#### LockAsync Overloads

```csharp
// Acquire a lock with a lease TTL asynchronously
public async Task<LockResponse> LockAsync(
    string name,
    int leaseTtl,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Acquire a lock with a lease ID asynchronously
public async Task<LockResponse> LockAsync(
    string name,
    long leaseId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LockAsync Parameters

- `name`: The name of the lock to acquire.
- `leaseTtl`: The time-to-live in seconds for the lease associated with the lock.
- `leaseId`: The ID of an existing lease to associate with the lock.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LockAsync Returns

- `Task<LockResponse>`: The etcd response containing the lock key.

## Unlock Methods

### Unlock

Releases a lock.

#### Unlock Overloads

```csharp
// Release a lock
public UnlockResponse Unlock(
    string key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Release a lock with a ByteString key
public UnlockResponse Unlock(
    ByteString key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Unlock Parameters

- `key`: The key of the lock to release.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Unlock Returns

- `UnlockResponse`: The etcd response for the unlock operation.

### UnlockAsync

Releases a lock asynchronously.

#### UnlockAsync Overloads

```csharp
// Release a lock asynchronously
public async Task<UnlockResponse> UnlockAsync(
    string key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Release a lock with a ByteString key asynchronously
public async Task<UnlockResponse> UnlockAsync(
    ByteString key,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### UnlockAsync Parameters

- `key`: The key of the lock to release.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### UnlockAsync Returns

- `Task<UnlockResponse>`: The etcd response for the unlock operation.

## Lock Response

The `LockResponse` class represents the response from a lock operation.

### LockResponse Properties

- `Header`: The response header.
- `Key`: The key that represents the lock.

### LockResponse Example

```csharp
// Acquire a lock
var lockResponse = client.Lock("my-lock", 10);

// Get the lock key
string lockKey = lockResponse.Key.ToStringUtf8();
Console.WriteLine($"Lock acquired with key: {lockKey}");

// Use the lock
// ...

// Release the lock
client.Unlock(lockKey);
```

## Unlock Response

The `UnlockResponse` class represents the response from an unlock operation.

### UnlockResponse Properties

- `Header`: The response header.

### UnlockResponse Example

```csharp
// Acquire a lock
var lockResponse = client.Lock("my-lock", 10);
string lockKey = lockResponse.Key.ToStringUtf8();

// Use the lock
// ...

// Release the lock
var unlockResponse = client.Unlock(lockKey);
Console.WriteLine("Lock released");
```

## See Also

- [Lock Operations](index.md) - Overview and examples of lock operations
- [Lease API Reference](../lease/api-reference.md) - API reference for lease operations
- [Election API Reference](../election/api-reference.md) - API reference for election operations
