# Maintenance Operations

This page documents how to use maintenance operations in etcd using the `dotnet-etcd` client.

## Overview

etcd provides maintenance operations to manage the health and performance of the etcd cluster. These operations allow you to:

- Get the status of the etcd cluster
- Defragment the etcd database
- Take snapshots of the etcd database
- Move the leadership to another member
- Enable or disable alarms

## Getting Cluster Status

To get the status of the etcd cluster:

```csharp
// Get the status of the etcd cluster
var statusResponse = client.Status();

// Display status information
Console.WriteLine($"Leader: {statusResponse.Leader}");
Console.WriteLine($"Version: {statusResponse.Version}");
Console.WriteLine($"DB Size: {statusResponse.DbSize}");
Console.WriteLine($"DB Size in Use: {statusResponse.DbSizeInUse}");
Console.WriteLine($"Current Revision: {statusResponse.Header.Revision}");
Console.WriteLine($"Raft Term: {statusResponse.Header.RaftTerm}");
Console.WriteLine($"Raft Index: {statusResponse.RaftIndex}");
Console.WriteLine($"Raft Applied Index: {statusResponse.RaftAppliedIndex}");
```

## Getting Cluster Status Asynchronously

You can get the cluster status asynchronously:

```csharp
// Get the status of the etcd cluster asynchronously
var statusResponse = await client.StatusAsync();

// Display status information
Console.WriteLine($"Leader: {statusResponse.Leader}");
Console.WriteLine($"Version: {statusResponse.Version}");
Console.WriteLine($"DB Size: {statusResponse.DbSize}");
Console.WriteLine($"DB Size in Use: {statusResponse.DbSizeInUse}");
```

## Defragmenting the Database

Over time, the etcd database can become fragmented, which can impact performance. You can defragment the database to reclaim space:

```csharp
// Defragment the etcd database
var defragResponse = client.Defragment();

// Display defragmentation result
Console.WriteLine($"Defragmentation completed with header revision: {defragResponse.Header.Revision}");
```

## Defragmenting the Database Asynchronously

You can defragment the database asynchronously:

```csharp
// Defragment the etcd database asynchronously
var defragResponse = await client.DefragmentAsync();

// Display defragmentation result
Console.WriteLine($"Defragmentation completed with header revision: {defragResponse.Header.Revision}");
```

## Taking a Snapshot

You can take a snapshot of the etcd database for backup purposes:

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

## Taking a Snapshot Asynchronously

You can take a snapshot asynchronously:

```csharp
// Take a snapshot of the etcd database asynchronously
using (var snapshotStream = await client.SnapshotAsync())
{
    // Save the snapshot to a file
    using (var fileStream = File.Create("etcd-snapshot.db"))
    {
        await snapshotStream.CopyToAsync(fileStream);
    }
}

Console.WriteLine("Snapshot saved to etcd-snapshot.db");
```

## Moving Leadership

You can transfer leadership from the current leader to another member:

```csharp
// Get the ID of the target member to transfer leadership to
ulong targetMemberId = 12345;

// Transfer leadership
var moveLeaderResponse = client.MoveLeader(targetMemberId);

// Display move leader result
Console.WriteLine($"Leadership transferred with header revision: {moveLeaderResponse.Header.Revision}");
```

## Moving Leadership Asynchronously

You can transfer leadership asynchronously:

```csharp
// Get the ID of the target member to transfer leadership to
ulong targetMemberId = 12345;

// Transfer leadership asynchronously
var moveLeaderResponse = await client.MoveLeaderAsync(targetMemberId);

// Display move leader result
Console.WriteLine($"Leadership transferred with header revision: {moveLeaderResponse.Header.Revision}");
```

## Getting Alarms

You can get the current alarms in the etcd cluster:

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

## Getting Alarms Asynchronously

You can get alarms asynchronously:

```csharp
// Get all alarms asynchronously
var alarmsResponse = await client.AlarmAsync(AlarmAction.Get, AlarmType.None);

// Display alarms
foreach (var alarm in alarmsResponse.Alarms)
{
    Console.WriteLine($"Alarm Type: {alarm.Type}");
    Console.WriteLine($"Alarm Member ID: {alarm.MemberId}");
    Console.WriteLine();
}
```

## Disarming Alarms

You can disarm (deactivate) alarms in the etcd cluster:

```csharp
// Disarm all alarms
var disarmResponse = client.Alarm(AlarmAction.Deactivate, AlarmType.None);

// Display disarm result
Console.WriteLine($"Disarmed alarms with header revision: {disarmResponse.Header.Revision}");
Console.WriteLine($"Number of alarms disarmed: {disarmResponse.Alarms.Count}");
```

## Disarming Alarms Asynchronously

You can disarm alarms asynchronously:

```csharp
// Disarm all alarms asynchronously
var disarmResponse = await client.AlarmAsync(AlarmAction.Deactivate, AlarmType.None);

// Display disarm result
Console.WriteLine($"Disarmed alarms with header revision: {disarmResponse.Header.Revision}");
Console.WriteLine($"Number of alarms disarmed: {disarmResponse.Alarms.Count}");
```

## Disarming Specific Alarms

You can disarm specific types of alarms:

```csharp
// Disarm NOSPACE alarms
var disarmResponse = client.Alarm(AlarmAction.Deactivate, AlarmType.Nospace);

// Display disarm result
Console.WriteLine($"Disarmed NOSPACE alarms with header revision: {disarmResponse.Header.Revision}");
Console.WriteLine($"Number of alarms disarmed: {disarmResponse.Alarms.Count}");
```

## Implementing a Maintenance Utility

You can implement a utility class to manage maintenance operations:

```csharp
public class EtcdMaintenanceManager
{
    private readonly EtcdClient _client;
    
    public EtcdMaintenanceManager(EtcdClient client)
    {
        _client = client;
    }
    
    public async Task<StatusResponse> GetClusterStatusAsync()
    {
        return await _client.StatusAsync();
    }
    
    public async Task<bool> DefragmentDatabaseAsync()
    {
        try
        {
            await _client.DefragmentAsync();
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<Stream> TakeSnapshotAsync()
    {
        return await _client.SnapshotAsync();
    }
    
    public async Task<bool> SaveSnapshotToFileAsync(string filePath)
    {
        try
        {
            using (var snapshotStream = await _client.SnapshotAsync())
            using (var fileStream = File.Create(filePath))
            {
                await snapshotStream.CopyToAsync(fileStream);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task<bool> TransferLeadershipAsync(ulong targetMemberId)
    {
        try
        {
            await _client.MoveLeaderAsync(targetMemberId);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<List<AlarmMember>> GetAlarmsAsync()
    {
        var response = await _client.AlarmAsync(AlarmAction.Get, AlarmType.None);
        return response.Alarms.ToList();
    }
    
    public async Task<bool> DisarmAllAlarmsAsync()
    {
        try
        {
            await _client.AlarmAsync(AlarmAction.Deactivate, AlarmType.None);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<bool> DisarmNoSpaceAlarmsAsync()
    {
        try
        {
            await _client.AlarmAsync(AlarmAction.Deactivate, AlarmType.Nospace);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<bool> DisarmCorruptAlarmsAsync()
    {
        try
        {
            await _client.AlarmAsync(AlarmAction.Deactivate, AlarmType.Corrupt);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<string> GetClusterHealthSummaryAsync()
    {
        var status = await GetClusterStatusAsync();
        var alarms = await GetAlarmsAsync();
        
        var summary = new StringBuilder();
        summary.AppendLine("Etcd Cluster Health Summary:");
        summary.AppendLine($"Leader: {status.Leader}");
        summary.AppendLine($"Version: {status.Version}");
        summary.AppendLine($"DB Size: {status.DbSize} bytes");
        summary.AppendLine($"DB Size In Use: {status.DbSizeInUse} bytes");
        summary.AppendLine($"Current Revision: {status.Header.Revision}");
        
        if (alarms.Count > 0)
        {
            summary.AppendLine("\nActive Alarms:");
            foreach (var alarm in alarms)
            {
                summary.AppendLine($"- {alarm.Type} on member {alarm.MemberId}");
            }
        }
        else
        {
            summary.AppendLine("\nNo active alarms.");
        }
        
        return summary.ToString();
    }
}

// Usage
var maintenanceManager = new EtcdMaintenanceManager(client);

// Get cluster status
var status = await maintenanceManager.GetClusterStatusAsync();
Console.WriteLine($"Leader: {status.Leader}, DB Size: {status.DbSize} bytes");

// Take a snapshot
bool snapshotSaved = await maintenanceManager.SaveSnapshotToFileAsync("etcd-backup.db");
Console.WriteLine($"Snapshot saved: {snapshotSaved}");

// Get health summary
string healthSummary = await maintenanceManager.GetClusterHealthSummaryAsync();
Console.WriteLine(healthSummary);
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for maintenance operations
- [Cluster Operations](../cluster/index.md) - Working with etcd cluster operations
- [Authentication Operations](../authentication/index.md) - Working with etcd authentication
