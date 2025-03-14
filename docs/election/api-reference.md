# Election API Reference

This page provides a complete reference of all election-related methods available in the `dotnet-etcd` client.

## Campaign Methods

### Campaign

Creates a campaign to participate in leader election.

#### Campaign Overloads

```csharp
// Create a campaign with a lease ID and value
public CampaignResponse Campaign(
    string name,
    long leaseId,
    string value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Create a campaign with a lease ID and ByteString value
public CampaignResponse Campaign(
    string name,
    long leaseId,
    ByteString value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Campaign Parameters

- `name`: The name of the election.
- `leaseId`: The ID of the lease to associate with the campaign.
- `value`: The value to associate with the candidate.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Campaign Returns

- `CampaignResponse`: The etcd response containing the leader key-value.

### CampaignAsync

Creates a campaign to participate in leader election asynchronously.

#### CampaignAsync Overloads

```csharp
// Create a campaign with a lease ID and value asynchronously
public async Task<CampaignResponse> CampaignAsync(
    string name,
    long leaseId,
    string value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Create a campaign with a lease ID and ByteString value asynchronously
public async Task<CampaignResponse> CampaignAsync(
    string name,
    long leaseId,
    ByteString value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### CampaignAsync Parameters

- `name`: The name of the election.
- `leaseId`: The ID of the lease to associate with the campaign.
- `value`: The value to associate with the candidate.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### CampaignAsync Returns

- `Task<CampaignResponse>`: The etcd response containing the leader key-value.

## Proclaim Methods

### Proclaim

Updates the leader's value in an election.

#### Proclaim Overloads

```csharp
// Update the leader's value with a string
public ProclaimResponse Proclaim(
    KeyValue leader,
    string value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Update the leader's value with a ByteString
public ProclaimResponse Proclaim(
    KeyValue leader,
    ByteString value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Proclaim Parameters

- `leader`: The leader key-value from a campaign response.
- `value`: The new value to associate with the leader.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Proclaim Returns

- `ProclaimResponse`: The etcd response for the proclaim operation.

### ProclaimAsync

Updates the leader's value in an election asynchronously.

#### ProclaimAsync Overloads

```csharp
// Update the leader's value with a string asynchronously
public async Task<ProclaimResponse> ProclaimAsync(
    KeyValue leader,
    string value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Update the leader's value with a ByteString asynchronously
public async Task<ProclaimResponse> ProclaimAsync(
    KeyValue leader,
    ByteString value,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### ProclaimAsync Parameters

- `leader`: The leader key-value from a campaign response.
- `value`: The new value to associate with the leader.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### ProclaimAsync Returns

- `Task<ProclaimResponse>`: The etcd response for the proclaim operation.

## Leader Methods

### Leader

Gets the current leader of an election.

#### Leader Overloads

```csharp
// Get the current leader of an election
public LeaderResponse Leader(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Leader Parameters

- `name`: The name of the election.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Leader Returns

- `LeaderResponse`: The etcd response containing the leader key-value.

### LeaderAsync

Gets the current leader of an election asynchronously.

#### LeaderAsync Overloads

```csharp
// Get the current leader of an election asynchronously
public async Task<LeaderResponse> LeaderAsync(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### LeaderAsync Parameters

- `name`: The name of the election.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### LeaderAsync Returns

- `Task<LeaderResponse>`: The etcd response containing the leader key-value.

## Observe Methods

### Observe

Observes and streams election events.

#### Observe Overloads

```csharp
// Observe election events
public IAsyncStreamReader<LeaderResponse> Observe(
    string name,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Observe Parameters

- `name`: The name of the election.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Observe Returns

- `IAsyncStreamReader<LeaderResponse>`: A stream of leader responses.

### ObserveAsync

Observes and streams election events asynchronously.

#### ObserveAsync Overloads

```csharp
// Observe election events asynchronously
public async Task ObserveAsync(
    string name,
    Action<LeaderResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### ObserveAsync Parameters

- `name`: The name of the election.
- `callback`: The callback to invoke for each leader response.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

## Resign Methods

### Resign

Resigns leadership in an election.

#### Resign Overloads

```csharp
// Resign leadership
public ResignResponse Resign(
    KeyValue leader,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Resign Parameters

- `leader`: The leader key-value from a campaign response.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Resign Returns

- `ResignResponse`: The etcd response for the resign operation.

### ResignAsync

Resigns leadership in an election asynchronously.

#### ResignAsync Overloads

```csharp
// Resign leadership asynchronously
public async Task<ResignResponse> ResignAsync(
    KeyValue leader,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### ResignAsync Parameters

- `leader`: The leader key-value from a campaign response.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### ResignAsync Returns

- `Task<ResignResponse>`: The etcd response for the resign operation.

## Election Response Types

### CampaignResponse

The `CampaignResponse` class represents the response from a campaign operation.

#### CampaignResponse Properties

- `Header`: The response header.
- `Leader`: The key-value representing the leader.

### ProclaimResponse

The `ProclaimResponse` class represents the response from a proclaim operation.

#### ProclaimResponse Properties

- `Header`: The response header.

### LeaderResponse

The `LeaderResponse` class represents the response from a leader operation.

#### LeaderResponse Properties

- `Header`: The response header.
- `Kv`: The key-value representing the leader.

### ResignResponse

The `ResignResponse` class represents the response from a resign operation.

#### ResignResponse Properties

- `Header`: The response header.

## See Also

- [Election Operations](index.md) - Overview and examples of election operations
- [Lock API Reference](../lock/api-reference.md) - API reference for lock operations
- [Lease API Reference](../lease/api-reference.md) - API reference for lease operations
