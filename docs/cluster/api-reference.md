# Cluster API Reference

This page provides a complete reference of all cluster-related methods available in the `dotnet-etcd` client.

## Member List Methods

### MemberList

Gets a list of all members in the etcd cluster.

#### MemberList Overloads

```csharp
// Get all members in the cluster
public MemberListResponse MemberList(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberList Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberList Returns

- `MemberListResponse`: The etcd response containing the list of members.

### MemberListAsync

Gets a list of all members in the etcd cluster asynchronously.

#### MemberListAsync Overloads

```csharp
// Get all members in the cluster asynchronously
public async Task<MemberListResponse> MemberListAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberListAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberListAsync Returns

- `Task<MemberListResponse>`: The etcd response containing the list of members.

## Member Add Methods

### MemberAdd

Adds a new member to the etcd cluster.

#### MemberAdd Overloads

```csharp
// Add a new member to the cluster
public MemberAddResponse MemberAdd(
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberAdd Parameters

- `peerURLs`: The array of peer URLs for the new member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberAdd Returns

- `MemberAddResponse`: The etcd response containing the new member and updated list of members.

### MemberAddAsync

Adds a new member to the etcd cluster asynchronously.

#### MemberAddAsync Overloads

```csharp
// Add a new member to the cluster asynchronously
public async Task<MemberAddResponse> MemberAddAsync(
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberAddAsync Parameters

- `peerURLs`: The array of peer URLs for the new member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberAddAsync Returns

- `Task<MemberAddResponse>`: The etcd response containing the new member and updated list of members.

### MemberAddAsLearner

Adds a new member to the etcd cluster as a learner (non-voting member).

#### MemberAddAsLearner Overloads

```csharp
// Add a new learner member to the cluster
public MemberAddResponse MemberAddAsLearner(
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberAddAsLearner Parameters

- `peerURLs`: The array of peer URLs for the new learner member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberAddAsLearner Returns

- `MemberAddResponse`: The etcd response containing the new learner member and updated list of members.

### MemberAddAsLearnerAsync

Adds a new member to the etcd cluster as a learner (non-voting member) asynchronously.

#### MemberAddAsLearnerAsync Overloads

```csharp
// Add a new learner member to the cluster asynchronously
public async Task<MemberAddResponse> MemberAddAsLearnerAsync(
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberAddAsLearnerAsync Parameters

- `peerURLs`: The array of peer URLs for the new learner member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberAddAsLearnerAsync Returns

- `Task<MemberAddResponse>`: The etcd response containing the new learner member and updated list of members.

## Member Update Methods

### MemberUpdate

Updates an existing member in the etcd cluster.

#### MemberUpdate Overloads

```csharp
// Update an existing member in the cluster
public MemberUpdateResponse MemberUpdate(
    ulong memberId,
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberUpdate Parameters

- `memberId`: The ID of the member to update.
- `peerURLs`: The new array of peer URLs for the member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberUpdate Returns

- `MemberUpdateResponse`: The etcd response containing the updated list of members.

### MemberUpdateAsync

Updates an existing member in the etcd cluster asynchronously.

#### MemberUpdateAsync Overloads

```csharp
// Update an existing member in the cluster asynchronously
public async Task<MemberUpdateResponse> MemberUpdateAsync(
    ulong memberId,
    string[] peerURLs,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberUpdateAsync Parameters

- `memberId`: The ID of the member to update.
- `peerURLs`: The new array of peer URLs for the member.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberUpdateAsync Returns

- `Task<MemberUpdateResponse>`: The etcd response containing the updated list of members.

## Member Remove Methods

### MemberRemove

Removes a member from the etcd cluster.

#### MemberRemove Overloads

```csharp
// Remove a member from the cluster
public MemberRemoveResponse MemberRemove(
    ulong memberId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberRemove Parameters

- `memberId`: The ID of the member to remove.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberRemove Returns

- `MemberRemoveResponse`: The etcd response containing the updated list of members.

### MemberRemoveAsync

Removes a member from the etcd cluster asynchronously.

#### MemberRemoveAsync Overloads

```csharp
// Remove a member from the cluster asynchronously
public async Task<MemberRemoveResponse> MemberRemoveAsync(
    ulong memberId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberRemoveAsync Parameters

- `memberId`: The ID of the member to remove.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberRemoveAsync Returns

- `Task<MemberRemoveResponse>`: The etcd response containing the updated list of members.

## Member Promote Methods

### MemberPromote

Promotes a learner member to a voting member in the etcd cluster.

#### MemberPromote Overloads

```csharp
// Promote a learner member to a voting member
public MemberPromoteResponse MemberPromote(
    ulong memberId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberPromote Parameters

- `memberId`: The ID of the learner member to promote.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberPromote Returns

- `MemberPromoteResponse`: The etcd response containing the updated list of members.

### MemberPromoteAsync

Promotes a learner member to a voting member in the etcd cluster asynchronously.

#### MemberPromoteAsync Overloads

```csharp
// Promote a learner member to a voting member asynchronously
public async Task<MemberPromoteResponse> MemberPromoteAsync(
    ulong memberId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MemberPromoteAsync Parameters

- `memberId`: The ID of the learner member to promote.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MemberPromoteAsync Returns

- `Task<MemberPromoteResponse>`: The etcd response containing the updated list of members.

## Cluster Response Types

### MemberAddResponse

The `MemberAddResponse` class represents the response from a member add operation.

#### MemberAddResponse Properties

- `Header`: The response header.
- `Member`: The added member.
- `Members`: The list of all members after the add operation.

### MemberRemoveResponse

The `MemberRemoveResponse` class represents the response from a member remove operation.

#### MemberRemoveResponse Properties

- `Header`: The response header.
- `Members`: The list of all members after the remove operation.

### MemberUpdateResponse

The `MemberUpdateResponse` class represents the response from a member update operation.

#### MemberUpdateResponse Properties

- `Header`: The response header.
- `Members`: The list of all members after the update operation.

### MemberListResponse

The `MemberListResponse` class represents the response from a member list operation.

#### MemberListResponse Properties

- `Header`: The response header.
- `Members`: The list of all members.

### MemberPromoteResponse

The `MemberPromoteResponse` class represents the response from a member promote operation.

#### MemberPromoteResponse Properties

- `Header`: The response header.
- `Members`: The list of all members after the promote operation.

### ClusterStatusResponse

The `ClusterStatusResponse` class represents the response from a cluster status operation.

#### ClusterStatusResponse Properties

- `Header`: The response header.
- `Members`: The list of all members with their status.

### Member

The `Member` class represents a member in the etcd cluster.

#### Member Properties

- `ID`: The unique ID of the member.
- `Name`: The name of the member.
- `PeerURLs`: The list of peer URLs for the member.
- `ClientURLs`: The list of client URLs for the member.
- `IsLearner`: Whether the member is a learner (non-voting) member.

## Examples

### Listing Members

```csharp
// Get all members in the cluster
var membersResponse = client.MemberList();

// Display member information
foreach (var member in membersResponse.Members)
{
    Console.WriteLine($"Member ID: {member.ID}");
    Console.WriteLine($"Member Name: {member.Name}");
    Console.WriteLine($"Peer URLs: {string.Join(", ", member.PeerURLs)}");
    Console.WriteLine($"Client URLs: {string.Join(", ", member.ClientURLs)}");
    Console.WriteLine($"Is Learner: {member.IsLearner}");
    Console.WriteLine();
}
```

### Adding a Member

```csharp
// Define the peer URLs for the new member
string[] peerURLs = { "http://example.com:2380" };

// Add the member to the cluster
var addResponse = client.MemberAdd(peerURLs);

// Display the new member information
Console.WriteLine($"Added member with ID: {addResponse.Member.ID}");
Console.WriteLine($"Peer URLs: {string.Join(", ", addResponse.Member.PeerURLs)}");
```

### Updating a Member

```csharp
// Get the member ID to update
ulong memberId = 12345;

// Define the new peer URLs
string[] newPeerURLs = { "http://new-example.com:2380" };

// Update the member
var updateResponse = client.MemberUpdate(memberId, newPeerURLs);

// Display updated cluster information
Console.WriteLine("Updated cluster members:");
foreach (var member in updateResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
    Console.WriteLine($"  Peer URLs: {string.Join(", ", member.PeerURLs)}");
}
```

### Removing a Member

```csharp
// Get the member ID to remove
ulong memberId = 12345;

// Remove the member
var removeResponse = client.MemberRemove(memberId);

// Display updated cluster information
Console.WriteLine("Remaining cluster members:");
foreach (var member in removeResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
}
```

### Promoting a Learner Member

```csharp
// Get the learner member ID to promote
ulong learnerId = 12345;

// Promote the learner member
var promoteResponse = client.MemberPromote(learnerId);

// Display updated cluster information
Console.WriteLine("Updated cluster members:");
foreach (var member in promoteResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
    Console.WriteLine($"  Is Learner: {member.IsLearner}");
}
```

## See Also

- [Cluster Operations](index.md) - Overview and examples of cluster operations
- [Maintenance API Reference](../maintenance/api-reference.md) - API reference for maintenance operations
- [Authentication API Reference](../authentication/api-reference.md) - API reference for authentication operations
