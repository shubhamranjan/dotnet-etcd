# Cluster Operations

This page documents how to use cluster operations in etcd using the `dotnet-etcd` client.

## Overview

etcd provides cluster operations to manage the etcd cluster itself. These operations allow you to:

- Add or remove members from the cluster
- Get information about the cluster members
- Update member information

## Getting Cluster Members

To get information about all members in the cluster:

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
    Console.WriteLine();
}
```

## Getting Cluster Members Asynchronously

You can get cluster members asynchronously:

```csharp
// Get all members in the cluster asynchronously
var membersResponse = await client.MemberListAsync();

// Display member information
foreach (var member in membersResponse.Members)
{
    Console.WriteLine($"Member ID: {member.ID}");
    Console.WriteLine($"Member Name: {member.Name}");
    Console.WriteLine($"Peer URLs: {string.Join(", ", member.PeerURLs)}");
    Console.WriteLine($"Client URLs: {string.Join(", ", member.ClientURLs)}");
    Console.WriteLine();
}
```

## Adding a Member to the Cluster

To add a new member to the cluster:

```csharp
// Define the peer URLs for the new member
string[] peerURLs = { "http://example.com:2380" };

// Add the member to the cluster
var addResponse = client.MemberAdd(peerURLs);

// Display the new member information
Console.WriteLine($"Added member with ID: {addResponse.Member.ID}");
Console.WriteLine($"Peer URLs: {string.Join(", ", addResponse.Member.PeerURLs)}");

// Display updated cluster information
Console.WriteLine("Updated cluster members:");
foreach (var member in addResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
}
```

## Adding a Member to the Cluster Asynchronously

You can add a member asynchronously:

```csharp
// Define the peer URLs for the new member
string[] peerURLs = { "http://example.com:2380" };

// Add the member to the cluster asynchronously
var addResponse = await client.MemberAddAsync(peerURLs);

// Display the new member information
Console.WriteLine($"Added member with ID: {addResponse.Member.ID}");
Console.WriteLine($"Peer URLs: {string.Join(", ", addResponse.Member.PeerURLs)}");
```

## Adding a Learner Member to the Cluster

You can add a member as a learner, which means it will not participate in voting:

```csharp
// Define the peer URLs for the new learner member
string[] peerURLs = { "http://example.com:2380" };

// Add the learner member to the cluster
var addResponse = client.MemberAddAsLearner(peerURLs);

// Display the new learner member information
Console.WriteLine($"Added learner member with ID: {addResponse.Member.ID}");
Console.WriteLine($"Peer URLs: {string.Join(", ", addResponse.Member.PeerURLs)}");
Console.WriteLine($"Is Learner: {addResponse.Member.IsLearner}");
```

## Adding a Learner Member to the Cluster Asynchronously

You can add a learner member asynchronously:

```csharp
// Define the peer URLs for the new learner member
string[] peerURLs = { "http://example.com:2380" };

// Add the learner member to the cluster asynchronously
var addResponse = await client.MemberAddAsLearnerAsync(peerURLs);

// Display the new learner member information
Console.WriteLine($"Added learner member with ID: {addResponse.Member.ID}");
Console.WriteLine($"Peer URLs: {string.Join(", ", addResponse.Member.PeerURLs)}");
Console.WriteLine($"Is Learner: {addResponse.Member.IsLearner}");
```

## Updating a Member in the Cluster

To update a member's peer URLs:

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

## Updating a Member in the Cluster Asynchronously

You can update a member asynchronously:

```csharp
// Get the member ID to update
ulong memberId = 12345;

// Define the new peer URLs
string[] newPeerURLs = { "http://new-example.com:2380" };

// Update the member asynchronously
var updateResponse = await client.MemberUpdateAsync(memberId, newPeerURLs);

// Display updated cluster information
Console.WriteLine("Updated cluster members:");
foreach (var member in updateResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
    Console.WriteLine($"  Peer URLs: {string.Join(", ", member.PeerURLs)}");
}
```

## Removing a Member from the Cluster

To remove a member from the cluster:

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

## Removing a Member from the Cluster Asynchronously

You can remove a member asynchronously:

```csharp
// Get the member ID to remove
ulong memberId = 12345;

// Remove the member asynchronously
var removeResponse = await client.MemberRemoveAsync(memberId);

// Display updated cluster information
Console.WriteLine("Remaining cluster members:");
foreach (var member in removeResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
}
```

## Promoting a Learner Member

To promote a learner member to a voting member:

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

## Promoting a Learner Member Asynchronously

You can promote a learner member asynchronously:

```csharp
// Get the learner member ID to promote
ulong learnerId = 12345;

// Promote the learner member asynchronously
var promoteResponse = await client.MemberPromoteAsync(learnerId);

// Display updated cluster information
Console.WriteLine("Updated cluster members:");
foreach (var member in promoteResponse.Members)
{
    Console.WriteLine($"- {member.Name} (ID: {member.ID})");
    Console.WriteLine($"  Is Learner: {member.IsLearner}");
}
```

## Implementing a Cluster Management Utility

You can implement a utility class to manage cluster operations:

```csharp
public class EtcdClusterManager
{
    private readonly EtcdClient _client;
    
    public EtcdClusterManager(EtcdClient client)
    {
        _client = client;
    }
    
    public async Task<List<Member>> GetClusterMembersAsync()
    {
        var response = await _client.MemberListAsync();
        return response.Members.ToList();
    }
    
    public async Task<Member> AddMemberAsync(string[] peerURLs, bool asLearner = false)
    {
        if (asLearner)
        {
            var response = await _client.MemberAddAsLearnerAsync(peerURLs);
            return response.Member;
        }
        else
        {
            var response = await _client.MemberAddAsync(peerURLs);
            return response.Member;
        }
    }
    
    public async Task<bool> UpdateMemberAsync(ulong memberId, string[] newPeerURLs)
    {
        try
        {
            await _client.MemberUpdateAsync(memberId, newPeerURLs);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<bool> RemoveMemberAsync(ulong memberId)
    {
        try
        {
            await _client.MemberRemoveAsync(memberId);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<bool> PromoteLearnerAsync(ulong learnerId)
    {
        try
        {
            await _client.MemberPromoteAsync(learnerId);
            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }
    
    public async Task<Member> FindMemberByNameAsync(string name)
    {
        var members = await GetClusterMembersAsync();
        return members.FirstOrDefault(m => m.Name == name);
    }
    
    public async Task<Member> FindMemberByClientURLAsync(string clientURL)
    {
        var members = await GetClusterMembersAsync();
        return members.FirstOrDefault(m => m.ClientURLs.Contains(clientURL));
    }
}

// Usage
var clusterManager = new EtcdClusterManager(client);

// Get all members
var members = await clusterManager.GetClusterMembersAsync();
foreach (var member in members)
{
    Console.WriteLine($"Member: {member.Name} (ID: {member.ID})");
}

// Add a new member
var newMember = await clusterManager.AddMemberAsync(
    new[] { "http://new-node.example.com:2380" },
    asLearner: true
);
Console.WriteLine($"Added new learner member with ID: {newMember.ID}");

// Find a member by name
var foundMember = await clusterManager.FindMemberByNameAsync("etcd-node-1");
if (foundMember != null)
{
    Console.WriteLine($"Found member: {foundMember.Name} (ID: {foundMember.ID})");
}
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for cluster operations
- [Maintenance Operations](../maintenance/index.md) - Working with etcd maintenance operations
- [Authentication Operations](../authentication/index.md) - Working with etcd authentication
