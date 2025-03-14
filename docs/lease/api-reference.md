# Lease API Reference

This page provides a complete reference of all lease-related methods available in the `dotnet-etcd` client.

## Lease Grant Methods

### LeaseGrant

Creates a lease with a specified TTL.

#### LeaseGrant Overloads

```csharp
// Create a lease with a TTL
public LeaseGrantResponse LeaseGrant(
    long ttl,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Create a lease with a TTL and ID
public LeaseGrantResponse LeaseGrant(
    long ttl,
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseGrant Parameters

- `ttl`: The time-to-live in seconds for the lease.
- `id`: The requested ID for the lease. If not provided, etcd will assign an ID.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseGrant Returns

- `LeaseGrantResponse`: The etcd response containing the lease ID and TTL.

### LeaseGrantAsync

Creates a lease with a specified TTL asynchronously.

#### LeaseGrantAsync Overloads

```csharp
// Create a lease with a TTL asynchronously
public async Task<LeaseGrantResponse> LeaseGrantAsync(
    long ttl,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Create a lease with a TTL and ID asynchronously
public async Task<LeaseGrantResponse> LeaseGrantAsync(
    long ttl,
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseGrantAsync Parameters

- `ttl`: The time-to-live in seconds for the lease.
- `id`: The requested ID for the lease. If not provided, etcd will assign an ID.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseGrantAsync Returns

- `Task<LeaseGrantResponse>`: The etcd response containing the lease ID and TTL.

## Lease Revoke Methods

### LeaseRevoke

Revokes a lease.

#### LeaseRevoke Overloads

```csharp
// Revoke a lease
public LeaseRevokeResponse LeaseRevoke(
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseRevoke Parameters

- `id`: The ID of the lease to revoke.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseRevoke Returns

- `LeaseRevokeResponse`: The etcd response for the lease revoke operation.

### LeaseRevokeAsync

Revokes a lease asynchronously.

#### LeaseRevokeAsync Overloads

```csharp
// Revoke a lease asynchronously
public async Task<LeaseRevokeResponse> LeaseRevokeAsync(
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseRevokeAsync Parameters

- `id`: The ID of the lease to revoke.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseRevokeAsync Returns

- `Task<LeaseRevokeResponse>`: The etcd response for the lease revoke operation.

## Lease Keep-Alive Methods

### LeaseKeepAlive

Keeps a lease alive.

#### LeaseKeepAlive Overloads

```csharp
// Keep a lease alive
public LeaseKeepAliveResponse LeaseKeepAlive(
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseKeepAlive Parameters

- `id`: The ID of the lease to keep alive.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseKeepAlive Returns

- `LeaseKeepAliveResponse`: The etcd response for the lease keep-alive operation.

### LeaseKeepAliveAsync

Keeps a lease alive asynchronously.

#### LeaseKeepAliveAsync Overloads

```csharp
// Keep a lease alive asynchronously
public async Task<LeaseKeepAliveResponse> LeaseKeepAliveAsync(
    long id,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseKeepAliveAsync Parameters

- `id`: The ID of the lease to keep alive.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseKeepAliveAsync Returns

- `Task<LeaseKeepAliveResponse>`: The etcd response for the lease keep-alive operation.

## Lease Time-to-Live Methods

### LeaseTimeToLive

Gets information about a lease.

#### LeaseTimeToLive Overloads

```csharp
// Get lease information
public LeaseTimeToLiveResponse LeaseTimeToLive(
    long id,
    bool keys = false,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseTimeToLive Parameters

- `id`: The ID of the lease to get information about.
- `keys`: Whether to return the list of keys attached to the lease.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseTimeToLive Returns

- `LeaseTimeToLiveResponse`: The etcd response containing lease information.

### LeaseTimeToLiveAsync

Gets information about a lease asynchronously.

#### LeaseTimeToLiveAsync Overloads

```csharp
// Get lease information asynchronously
public async Task<LeaseTimeToLiveResponse> LeaseTimeToLiveAsync(
    long id,
    bool keys = false,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseTimeToLiveAsync Parameters

- `id`: The ID of the lease to get information about.
- `keys`: Whether to return the list of keys attached to the lease.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseTimeToLiveAsync Returns

- `Task<LeaseTimeToLiveResponse>`: The etcd response containing lease information.

## Lease List Methods

### LeaseList

Lists all active leases.

#### LeaseList Overloads

```csharp
// List all active leases
public LeaseListResponse LeaseList(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseList Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseList Returns

- `LeaseListResponse`: The etcd response containing the list of active lease IDs.

### LeaseListAsync

Lists all active leases asynchronously.

#### LeaseListAsync Overloads

```csharp
// List all active leases asynchronously
public async Task<LeaseListResponse> LeaseListAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaseListAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaseListAsync Returns

- `Task<LeaseListResponse>`: The etcd response containing the list of active lease IDs.

## Key-Value with Lease Methods

### PutWithLease

Puts a key-value pair with a lease.

#### PutWithLease Overloads

```csharp
// Put a key-value pair with a lease
public PutResponse PutWithLease(
    string key,
    string val,
    long leaseId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### PutWithLease Parameters

- `key`: The key to put.
- `val`: The value to put.
- `leaseId`: The ID of the lease to attach to the key.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### PutWithLease Returns

- `PutResponse`: The etcd response for the put operation.

### PutWithLeaseAsync

Puts a key-value pair with a lease asynchronously.

#### PutWithLeaseAsync Overloads

```csharp
// Put a key-value pair with a lease asynchronously
public async Task<PutResponse> PutWithLeaseAsync(
    string key,
    string val,
    long leaseId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### PutWithLeaseAsync Parameters

- `key`: The key to put.
- `val`: The value to put.
- `leaseId`: The ID of the lease to attach to the key.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### PutWithLeaseAsync Returns

- `Task<PutResponse>`: The etcd response for the put operation.

## Lease Response Types

### LeaseGrantResponse

The `LeaseGrantResponse` class represents the response from a lease grant operation.

#### LeaseGrantResponse Properties

- `Header`: The response header.
- `ID`: The lease ID.
- `TTL`: The time-to-live in seconds for the lease.
- `Error`: The error message, if any.

### LeaseRevokeResponse

The `LeaseRevokeResponse` class represents the response from a lease revoke operation.

#### LeaseRevokeResponse Properties

- `Header`: The response header.

### LeaseKeepAliveResponse

The `LeaseKeepAliveResponse` class represents the response from a lease keep-alive operation.

#### LeaseKeepAliveResponse Properties

- `Header`: The response header.
- `ID`: The lease ID.
- `TTL`: The new time-to-live in seconds for the lease.

### LeaseTimeToLiveResponse

The `LeaseTimeToLiveResponse` class represents the response from a lease time-to-live operation.

#### LeaseTimeToLiveResponse Properties

- `Header`: The response header.
- `ID`: The lease ID.
- `TTL`: The remaining time-to-live in seconds for the lease.
- `GrantedTTL`: The initial granted time-to-live in seconds for the lease.
- `Keys`: The list of keys attached to the lease.

### LeaseListResponse

The `LeaseListResponse` class represents the response from a lease list operation.

#### LeaseListResponse Properties

- `Header`: The response header.
- `Leases`: The list of leases.

## See Also

- [Lease Operations](index.md) - Overview and examples of lease operations
- [Key-Value API Reference](../key-value/api-reference.md) - API reference for key-value operations
- [Lock Operations](../lock/index.md) - Using etcd's built-in lock service
