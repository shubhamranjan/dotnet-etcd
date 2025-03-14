# Election Operations

This page documents how to use leader election in etcd using the `dotnet-etcd` client.

## Overview

etcd provides a leader election service that allows multiple clients to coordinate and elect a leader among them. The election service in etcd is built on top of the key-value store and lease system.

## Creating a Campaign

To participate in an election, you need to create a campaign:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Start a campaign with the lease
var campaignResponse = client.Campaign("my-election", leaseId, "candidate-1");

// Get the leader key
string leaderKey = campaignResponse.Leader.Key.ToStringUtf8();
string leaderValue = campaignResponse.Leader.Value.ToStringUtf8();

Console.WriteLine($"Elected as leader with key: {leaderKey}");
Console.WriteLine($"Leader value: {leaderValue}");
```

The `Campaign` method creates a campaign with the specified election name, lease ID, and candidate value. If there is already a leader, the method will block until the current leader's lease expires or the leader resigns.

## Creating a Campaign with Timeout

You can specify a timeout for creating a campaign:

```csharp
try
{
    // Create a lease with a TTL of 10 seconds
    var leaseResponse = client.LeaseGrant(10);
    long leaseId = leaseResponse.ID;

    // Start a campaign with a 5-second timeout
    var campaignResponse = client.Campaign(
        "my-election",
        leaseId,
        "candidate-1",
        headers: null,
        deadline: DateTime.UtcNow.AddSeconds(5), // 5-second timeout
        cancellationToken: default
    );

    Console.WriteLine("Elected as leader");
}
catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
{
    Console.WriteLine("Failed to become leader within the timeout period");
}
```

## Creating a Campaign Asynchronously

You can create a campaign asynchronously:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = await client.LeaseGrantAsync(10);
long leaseId = leaseResponse.ID;

// Start a campaign asynchronously
var campaignResponse = await client.CampaignAsync("my-election", leaseId, "candidate-1");

Console.WriteLine("Elected as leader");
```

## Proclaiming New Values

Once elected as leader, you can proclaim new values:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Start a campaign
var campaignResponse = client.Campaign("my-election", leaseId, "candidate-1");

// Proclaim a new value
client.Proclaim(campaignResponse.Leader, "new-value");

Console.WriteLine("Proclaimed new value");
```

## Proclaiming New Values Asynchronously

You can proclaim new values asynchronously:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = await client.LeaseGrantAsync(10);
long leaseId = leaseResponse.ID;

// Start a campaign asynchronously
var campaignResponse = await client.CampaignAsync("my-election", leaseId, "candidate-1");

// Proclaim a new value asynchronously
await client.ProclaimAsync(campaignResponse.Leader, "new-value");

Console.WriteLine("Proclaimed new value");
```

## Observing the Leader

To observe the current leader of an election:

```csharp
// Observe the leader
var observeResponse = client.Leader("my-election");

// Get the leader key and value
string leaderKey = observeResponse.Kv.Key.ToStringUtf8();
string leaderValue = observeResponse.Kv.Value.ToStringUtf8();

Console.WriteLine($"Current leader key: {leaderKey}");
Console.WriteLine($"Current leader value: {leaderValue}");
```

## Observing the Leader Asynchronously

You can observe the leader asynchronously:

```csharp
// Observe the leader asynchronously
var observeResponse = await client.LeaderAsync("my-election");

// Get the leader key and value
string leaderKey = observeResponse.Kv.Key.ToStringUtf8();
string leaderValue = observeResponse.Kv.Value.ToStringUtf8();

Console.WriteLine($"Current leader key: {leaderKey}");
Console.WriteLine($"Current leader value: {leaderValue}");
```

## Resigning Leadership

To resign leadership:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = client.LeaseGrant(10);
long leaseId = leaseResponse.ID;

// Start a campaign
var campaignResponse = client.Campaign("my-election", leaseId, "candidate-1");

try
{
    // Use leadership
    Console.WriteLine("Elected as leader, performing work...");
    
    // Simulate work
    Thread.Sleep(2000);
}
finally
{
    // Resign leadership
    client.Resign(campaignResponse.Leader);
    Console.WriteLine("Resigned leadership");
}
```

## Resigning Leadership Asynchronously

You can resign leadership asynchronously:

```csharp
// Create a lease with a TTL of 10 seconds
var leaseResponse = await client.LeaseGrantAsync(10);
long leaseId = leaseResponse.ID;

// Start a campaign asynchronously
var campaignResponse = await client.CampaignAsync("my-election", leaseId, "candidate-1");

try
{
    // Use leadership
    Console.WriteLine("Elected as leader, performing work...");
    
    // Simulate work
    await Task.Delay(2000);
}
finally
{
    // Resign leadership asynchronously
    await client.ResignAsync(campaignResponse.Leader);
    Console.WriteLine("Resigned leadership");
}
```

## Implementing a Leader Election Pattern

You can implement a leader election pattern to coordinate distributed tasks:

```csharp
public class EtcdLeaderElection
{
    private readonly EtcdClient _client;
    private readonly string _electionName;
    private readonly string _candidateValue;
    private readonly int _leaseTtl;
    private long _leaseId;
    private KeyValue _leader;
    private CancellationTokenSource _keepAliveCts;
    
    public EtcdLeaderElection(EtcdClient client, string electionName, string candidateValue, int leaseTtl = 10)
    {
        _client = client;
        _electionName = electionName;
        _candidateValue = candidateValue;
        _leaseTtl = leaseTtl;
    }
    
    public async Task<bool> TryBecomeLeaderAsync(TimeSpan timeout)
    {
        try
        {
            // Create a lease
            var leaseResponse = await _client.LeaseGrantAsync(_leaseTtl);
            _leaseId = leaseResponse.ID;
            
            // Start keep-alive for the lease
            _keepAliveCts = new CancellationTokenSource();
            _ = KeepLeaseAliveAsync(_keepAliveCts.Token);
            
            // Start a campaign with timeout
            var campaignResponse = await _client.CampaignAsync(
                _electionName,
                _leaseId,
                _candidateValue,
                deadline: DateTime.UtcNow.Add(timeout)
            );
            
            _leader = campaignResponse.Leader;
            return true;
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
        {
            // Failed to become leader within timeout
            if (_keepAliveCts != null)
            {
                _keepAliveCts.Cancel();
                _keepAliveCts = null;
            }
            
            if (_leaseId != 0)
            {
                await _client.LeaseRevokeAsync(_leaseId);
                _leaseId = 0;
            }
            
            return false;
        }
    }
    
    public async Task ResignLeadershipAsync()
    {
        if (_leader != null)
        {
            await _client.ResignAsync(_leader);
            _leader = null;
        }
        
        if (_keepAliveCts != null)
        {
            _keepAliveCts.Cancel();
            _keepAliveCts = null;
        }
        
        if (_leaseId != 0)
        {
            await _client.LeaseRevokeAsync(_leaseId);
            _leaseId = 0;
        }
    }
    
    public async Task<KeyValue> GetCurrentLeaderAsync()
    {
        var observeResponse = await _client.LeaderAsync(_electionName);
        return observeResponse.Kv;
    }
    
    private async Task KeepLeaseAliveAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _client.LeaseKeepAliveAsync(_leaseId);
                await Task.Delay(TimeSpan.FromSeconds(_leaseTtl / 3), cancellationToken);
            }
            catch (Exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Try to resign leadership if we can't keep the lease alive
                    try
                    {
                        if (_leader != null)
                        {
                            await _client.ResignAsync(_leader);
                            _leader = null;
                        }
                    }
                    catch
                    {
                        // Ignore errors during resignation
                    }
                }
                break;
            }
        }
    }
}

// Usage
var election = new EtcdLeaderElection(client, "my-election", "candidate-1");

// Try to become leader
bool isLeader = await election.TryBecomeLeaderAsync(TimeSpan.FromSeconds(5));

if (isLeader)
{
    try
    {
        // Perform leader-specific tasks
        Console.WriteLine("I am the leader, performing work...");
        
        // Simulate work
        await Task.Delay(10000);
    }
    finally
    {
        // Resign leadership
        await election.ResignLeadershipAsync();
        Console.WriteLine("Resigned leadership");
    }
}
else
{
    // Perform follower-specific tasks
    Console.WriteLine("I am a follower");
    
    // Get the current leader
    var leader = await election.GetCurrentLeaderAsync();
    Console.WriteLine($"Current leader: {leader.Value.ToStringUtf8()}");
}
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for election operations
- [Lock Operations](../lock/index.md) - Using etcd's distributed locking
- [Lease Operations](../lease/index.md) - Working with leases
