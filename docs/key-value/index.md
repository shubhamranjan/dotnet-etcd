# Key-Value Operations

The etcd key-value API provides methods for storing, retrieving, and deleting key-value pairs. This page documents the various key-value operations available in the `dotnet-etcd` client.

## Overview

The key-value operations in etcd allow you to:

- Put (write) key-value pairs
- Get (read) values for specific keys
- Get values for a range of keys with a common prefix
- Delete keys
- Delete a range of keys with a common prefix
- Compact the key-value store

## Put Operations

### Put a Key-Value Pair

```csharp
// Basic put operation
client.Put("key", "value");

// Put with lease ID
client.Put("key", "value", new PutRequest { Lease = leaseId });

// Put with full request object
var putRequest = new PutRequest {
  Key = ByteString.CopyFromUtf8("key"),
  Value = ByteString.CopyFromUtf8("value"),
  Lease = leaseId,
  PrevKv = true // Return the previous key-value pair
};
var response = client.Put(putRequest);

// Asynchronous put
await client.PutAsync("key", "value");
```

## Get Operations

### Get a Single Key

```csharp
// Get the value for a key
string value = client.GetVal("key");

// Get the full response for a key
RangeResponse response = client.Get("key");
if (response.Count > 0) {
  string key = response.Kvs[0].Key.ToStringUtf8();
  string value = response.Kvs[0].Value.ToStringUtf8();
  long createRevision = response.Kvs[0].CreateRevision;
  long modRevision = response.Kvs[0].ModRevision;
  long version = response.Kvs[0].Version;
  long lease = response.Kvs[0].Lease;
}

// Asynchronous get
string value = await client.GetValAsync("key");
RangeResponse response = await client.GetAsync("key");
```

### Get a Range of Keys with a Prefix

```csharp
// Get all keys with a common prefix
RangeResponse response = client.GetRange("prefix/");

// Process the results
foreach (var kv in response.Kvs) {
  string key = kv.Key.ToStringUtf8();
  string value = kv.Value.ToStringUtf8();
  Console.WriteLine($"{key}: {value}");
}

// Get all keys with a common prefix as a dictionary
IDictionary<string, string> keyValues = client.GetRangeVal("prefix/");
foreach (var kv in keyValues) {
  Console.WriteLine($"{kv.Key}: {kv.Value}");
}

// Asynchronous range get
RangeResponse response = await client.GetRangeAsync("prefix/");
IDictionary<string, string> keyValues = await client.GetRangeValAsync("prefix/");
```

### Advanced Get Operations

```csharp
// Get with custom request
var rangeRequest = new RangeRequest {
  Key = ByteString.CopyFromUtf8("key"),
  RangeEnd = ByteString.CopyFromUtf8("key\0"), // Get a single key
  Limit = 10, // Limit the number of results
  Revision = 0, // Get the latest revision
  SortOrder = RangeRequest.Types.SortOrder.Descend,
  SortTarget = RangeRequest.Types.SortTarget.Key,
  Serializable = true, // Allow serializable reads (may return stale data)
  KeysOnly = false, // Include values
  CountOnly = false // Return the full key-value pairs
};
RangeResponse response = client.Get(rangeRequest);

// Get all keys (empty string prefix)
RangeResponse allKeys = client.GetRange("");
```

## Delete Operations

### Delete a Single Key

```csharp
// Delete a key
DeleteRangeResponse response = client.Delete("key");

// Check if the key was deleted
bool deleted = response.Deleted > 0;

// Asynchronous delete
DeleteRangeResponse response = await client.DeleteAsync("key");
```

### Delete a Range of Keys with a Prefix

```csharp
// Delete all keys with a common prefix
DeleteRangeResponse response = client.DeleteRange("prefix/");

// Get the number of keys deleted
long deletedCount = response.Deleted;

// Asynchronous range delete
DeleteRangeResponse response = await client.DeleteRangeAsync("prefix/");
```

### Advanced Delete Operations

```csharp
// Delete with custom request
var deleteRequest = new DeleteRangeRequest {
  Key = ByteString.CopyFromUtf8("key"),
  RangeEnd = ByteString.CopyFromUtf8("key\0"), // Delete a single key
  PrevKv = true // Return the previous key-value pairs
};
DeleteRangeResponse response = client.Delete(deleteRequest);

// Process the previous key-value pairs
foreach (var kv in response.PrevKvs) {
  string key = kv.Key.ToStringUtf8();
  string value = kv.Value.ToStringUtf8();
  Console.WriteLine($"Deleted: {key}: {value}");
}
```

## Compact Operations

Compaction removes all revisions from the key-value store up to a given revision. This helps reclaim storage space.

```csharp
// Compact the key-value store up to a specific revision
CompactionResponse response = client.Compact(new CompactionRequest {
  Revision = 100, // Compact up to revision 100
  Physical = true // Physically remove the compacted entries
});

// Asynchronous compact
CompactionResponse response = await client.CompactAsync(new CompactionRequest {
  Revision = 100,
  Physical = true
});
```

## Method Overloads

The `dotnet-etcd` client provides multiple overloads for each operation to support different use cases. See the [API Reference](api-reference.md) for a complete list of all available method overloads.

## See Also

- [API Reference](api-reference.md) - Complete API reference for key-value operations
- [Watch Operations](../watch/index.md) - How to watch for changes to keys
- [Transactions](../transactions/index.md) - How to perform atomic operations
