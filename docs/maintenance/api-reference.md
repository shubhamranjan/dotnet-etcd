# Maintenance API Reference

This page provides a complete reference of all maintenance-related methods available in the `dotnet-etcd` client.

## Status Methods

### Status

Gets the status of the etcd cluster.

#### Status Overloads

```csharp
// Get the status of the etcd cluster
public StatusResponse Status(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Status Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Status Returns

- `StatusResponse`: The etcd response containing the cluster status.

### StatusAsync

Gets the status of the etcd cluster asynchronously.

#### StatusAsync Overloads

```csharp
// Get the status of the etcd cluster asynchronously
public async Task<StatusResponse> StatusAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### StatusAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### StatusAsync Returns

- `Task<StatusResponse>`: The etcd response containing the cluster status.

## Defragment Methods

### Defragment

Defragments the etcd database to reclaim space.

#### Defragment Overloads

```csharp
// Defragment the etcd database
public DefragmentResponse Defragment(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Defragment Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Defragment Returns

- `DefragmentResponse`: The etcd response for the defragment operation.

### DefragmentAsync

Defragments the etcd database to reclaim space asynchronously.

#### DefragmentAsync Overloads

```csharp
// Defragment the etcd database asynchronously
public async Task<DefragmentResponse> DefragmentAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### DefragmentAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### DefragmentAsync Returns

- `Task<DefragmentResponse>`: The etcd response for the defragment operation.

## Snapshot Methods

### Snapshot

Takes a snapshot of the etcd database.

#### Snapshot Overloads

```csharp
// Take a snapshot of the etcd database
public Stream Snapshot(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Snapshot Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Snapshot Returns

- `Stream`: A stream containing the snapshot data.

### SnapshotAsync

Takes a snapshot of the etcd database asynchronously.

#### SnapshotAsync Overloads

```csharp
// Take a snapshot of the etcd database asynchronously
public async Task<Stream> SnapshotAsync(
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### SnapshotAsync Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### SnapshotAsync Returns

- `Task<Stream>`: A stream containing the snapshot data.

## MoveLeader Methods

### MoveLeader

Transfers leadership from the current leader to another member.

#### MoveLeader Overloads

```csharp
// Transfer leadership to another member
public MoveLeaderResponse MoveLeader(
    ulong targetId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MoveLeader Parameters

- `targetId`: The ID of the member to transfer leadership to.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MoveLeader Returns

- `MoveLeaderResponse`: The etcd response for the move leader operation.

### MoveLeaderAsync

Transfers leadership from the current leader to another member asynchronously.

#### MoveLeaderAsync Overloads

```csharp
// Transfer leadership to another member asynchronously
public async Task<MoveLeaderResponse> MoveLeaderAsync(
    ulong targetId,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### MoveLeaderAsync Parameters

- `targetId`: The ID of the member to transfer leadership to.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### MoveLeaderAsync Returns

- `Task<MoveLeaderResponse>`: The etcd response for the move leader operation.

## Alarm Methods

### Alarm

Manages alarms in the etcd cluster.

#### Alarm Overloads

```csharp
// Manage alarms in the etcd cluster
public AlarmResponse Alarm(
    AlarmAction action,
    AlarmType alarm,
    ulong memberID = 0,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Alarm Parameters

- `action`: The action to perform on the alarm (Get, Activate, or Deactivate).
- `alarm`: The type of alarm to manage (None, Nospace, or Corrupt).
- `memberID`: The ID of the member to manage alarms for. Use 0 for all members.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Alarm Returns

- `AlarmResponse`: The etcd response for the alarm operation.

### AlarmAsync

Manages alarms in the etcd cluster asynchronously.

#### AlarmAsync Overloads

```csharp
// Manage alarms in the etcd cluster asynchronously
public async Task<AlarmResponse> AlarmAsync(
    AlarmAction action,
    AlarmType alarm,
    ulong memberID = 0,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### AlarmAsync Parameters

- `action`: The action to perform on the alarm (Get, Activate, or Deactivate).
- `alarm`: The type of alarm to manage (None, Nospace, or Corrupt).
- `memberID`: The ID of the member to manage alarms for. Use 0 for all members.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### AlarmAsync Returns

- `Task<AlarmResponse>`: The etcd response for the alarm operation.

## Maintenance Response Types

### AlarmResponse

The `AlarmResponse` class represents the response from an alarm operation.

#### AlarmResponse Properties

- `Header`: The response header.
- `Alarms`: The list of alarms.

### StatusResponse

The `StatusResponse` class represents the response from a status operation.

#### StatusResponse Properties

- `Header`: The response header.
- `Version`: The version of the etcd server.
- `DbSize`: The size of the etcd database in bytes.
- `Leader`: The ID of the leader node.
- `RaftIndex`: The current raft index.
- `RaftTerm`: The current raft term.

### HashResponse

The `HashResponse` class represents the response from a hash operation.

#### HashResponse Properties

- `Header`: The response header.
- `Hash`: The hash of the etcd database.

### HashKVResponse

The `HashKVResponse` class represents the response from a hash KV operation.

#### HashKVResponse Properties

- `Header`: The response header.
- `Hash`: The hash of the KV store.
- `CompactRevision`: The compact revision of the KV store.
- `HashRevision`: The revision used to generate the hash.

### SnapshotResponse

The `SnapshotResponse` class represents the response from a snapshot operation.

#### SnapshotResponse Properties

- `Header`: The response header.
- `RemainingBytes`: The remaining bytes in the snapshot.
- `Blob`: The snapshot data.

## Enums

### AlarmAction

The `AlarmAction` enum represents the action to perform on an alarm.

#### AlarmAction Values

- `Get`: Get the current alarms.
- `Activate`: Activate an alarm.
- `Deactivate`: Deactivate an alarm.

### AlarmType

The `AlarmType` enum represents the type of alarm.

#### AlarmType Values

- `None`: No specific alarm type.
- `Nospace`: No space left on the device.
- `Corrupt`: Database corruption detected.

## Examples

### Getting Cluster Status

```csharp
// Get the status of the etcd cluster
var statusResponse = client.Status();

// Display status information
Console.WriteLine($"Leader: {statusResponse.Leader}");
Console.WriteLine($"Version: {statusResponse.Version}");
Console.WriteLine($"DB Size: {statusResponse.DbSize}");
Console.WriteLine($"Current Revision: {statusResponse.Header.Revision}");
Console.WriteLine($"Raft Term: {statusResponse.Header.RaftTerm}");
Console.WriteLine($"Raft Index: {statusResponse.RaftIndex}");
```

### Defragmenting the Database

```csharp
// Defragment the etcd database
var defragResponse = client.Defragment();

// Display defragmentation result
Console.WriteLine($"Defragmentation completed with header revision: {defragResponse.Header.Revision}");
```

### Taking a Snapshot

```csharp
// Take a snapshot of the etcd database
using (var snapshotStream = client.Snapshot())
{
    // Save the snapshot to a file
    using (var fileStream = File.Create("etcd-snapshot.db"))
    {
        snapshotStream.CopyTo(fileStream);
    }
}

Console.WriteLine("Snapshot saved to etcd-snapshot.db");
```

### Moving Leadership

```csharp
// Get the ID of the target member to transfer leadership to
ulong targetMemberId = 12345;

// Transfer leadership
var moveLeaderResponse = client.MoveLeader(targetMemberId);

// Display move leader result
Console.WriteLine($"Leadership transferred with header revision: {moveLeaderResponse.Header.Revision}");
```

### Getting Alarms

```csharp
// Get all alarms
var alarmsResponse = client.Alarm(AlarmAction.Get, AlarmType.None);

// Display alarms
foreach (var alarm in alarmsResponse.Alarms)
{
    Console.WriteLine($"Alarm Type: {alarm.Type}");
    Console.WriteLine($"Alarm Member ID: {alarm.MemberId}");
    Console.WriteLine();
}
```

### Disarming Alarms

```csharp
// Disarm all alarms
var disarmResponse = client.Alarm(AlarmAction.Deactivate, AlarmType.None);

// Display disarm result
Console.WriteLine($"Disarmed alarms with header revision: {disarmResponse.Header.Revision}");
Console.WriteLine($"Number of alarms disarmed: {disarmResponse.Alarms.Count}");
```

## See Also

- [Maintenance Operations](index.md) - Overview and examples of maintenance operations
- [Cluster API Reference](../cluster/api-reference.md) - API reference for cluster operations
- [Authentication API Reference](../authentication/api-reference.md) - API reference for authentication operations
