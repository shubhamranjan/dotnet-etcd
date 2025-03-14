# Key-Value API Reference

This page provides a complete reference of all key-value operation methods and their overloads available in the `dotnet-etcd` client.

## Get Operations

### Get

Gets the etcd response for a specified key or range request.

#### Get Overloads

```csharp
// Get with a RangeRequest
public RangeResponse Get(
  RangeRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// Get with a key string
public RangeResponse Get(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### Get Parameters

- `request`: The RangeRequest to send to the server.
- `key`: Key for which value needs to be fetched.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Get Returns

- `RangeResponse`: The etcd response for the specified request or key.

### GetAsync

Gets the etcd response for a specified key or range request asynchronously.

#### GetAsync Overloads

```csharp
// GetAsync with a RangeRequest
public async Task<RangeResponse> GetAsync(
  RangeRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// GetAsync with a key string
public async Task<RangeResponse> GetAsync(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetAsync Parameters

- `request`: The RangeRequest to send to the server.
- `key`: Key for which value needs to be fetched.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetAsync Returns

- `Task<RangeResponse>`: The etcd response for the specified request or key.

### GetVal

Gets the value for a specified key.

#### GetVal Overload

```csharp
public string GetVal(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetVal Parameters

- `key`: Key for which value needs to be fetched.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetVal Returns

- `string`: The value for the specified key, or an empty string if the key doesn't exist.

### GetValAsync

Gets the value for a specified key asynchronously.

#### GetValAsync Overload

```csharp
public async Task<string> GetValAsync(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetValAsync Parameters

- `key`: Key for which value needs to be fetched.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetValAsync Returns

- `Task<string>`: The value for the specified key, or an empty string if the key doesn't exist.

### GetRange

Gets all keys with a specified prefix.

#### GetRange Overload

```csharp
public RangeResponse GetRange(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetRange Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetRange Returns

- `RangeResponse`: The etcd response containing all keys with the specified prefix.

### GetRangeAsync

Gets all keys with a specified prefix asynchronously.

#### GetRangeAsync Overload

```csharp
public async Task<RangeResponse> GetRangeAsync(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetRangeAsync Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetRangeAsync Returns

- `Task<RangeResponse>`: The etcd response containing all keys with the specified prefix.

### GetRangeVal

Gets all keys with a specified prefix as a dictionary.

#### GetRangeVal Overload

```csharp
public IDictionary<string, string> GetRangeVal(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetRangeVal Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetRangeVal Returns

- `IDictionary<string, string>`: A dictionary containing all keys with the specified prefix and their values.

### GetRangeValAsync

Gets all keys with a specified prefix as a dictionary asynchronously.

#### GetRangeValAsync Overload

```csharp
public async Task<IDictionary<string, string>> GetRangeValAsync(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### GetRangeValAsync Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### GetRangeValAsync Returns

- `Task<IDictionary<string, string>>`: A dictionary containing all keys with the specified prefix and their values.

## Put Operations

### Put

Puts a key-value pair into etcd.

#### Put Overloads

```csharp
// Put with a PutRequest
public PutResponse Put(
  PutRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// Put with key and value strings
public PutResponse Put(
  string key,
  string val,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### Put Parameters

- `request`: The PutRequest to send to the server.
- `key`: The key to put.
- `val`: The value to put.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Put Returns

- `PutResponse`: The etcd response for the put operation.

### PutAsync

Puts a key-value pair into etcd asynchronously.

#### PutAsync Overloads

```csharp
// PutAsync with a PutRequest
public async Task<PutResponse> PutAsync(
  PutRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// PutAsync with key and value strings
public async Task<PutResponse> PutAsync(
  string key,
  string val,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### PutAsync Parameters

- `request`: The PutRequest to send to the server.
- `key`: The key to put.
- `val`: The value to put.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### PutAsync Returns

- `Task<PutResponse>`: The etcd response for the put operation.

## Delete Operations

### Delete

Deletes a key or range of keys from etcd.

#### Delete Overloads

```csharp
// Delete with a DeleteRangeRequest
public DeleteRangeResponse Delete(
  DeleteRangeRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// Delete with a key string
public DeleteRangeResponse Delete(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### Delete Parameters

- `request`: The DeleteRangeRequest to send to the server.
- `key`: The key to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Delete Returns

- `DeleteRangeResponse`: The etcd response for the delete operation.

### DeleteAsync

Deletes a key or range of keys from etcd asynchronously.

#### DeleteAsync Overloads

```csharp
// DeleteAsync with a DeleteRangeRequest
public async Task<DeleteRangeResponse> DeleteAsync(
  DeleteRangeRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)

// DeleteAsync with a key string
public async Task<DeleteRangeResponse> DeleteAsync(
  string key,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### DeleteAsync Parameters

- `request`: The DeleteRangeRequest to send to the server.
- `key`: The key to delete.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### DeleteAsync Returns

- `Task<DeleteRangeResponse>`: The etcd response for the delete operation.

### DeleteRange

Deletes all keys with a specified prefix.

#### DeleteRange Overload

```csharp
public DeleteRangeResponse DeleteRange(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### DeleteRange Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### DeleteRange Returns

- `DeleteRangeResponse`: The etcd response for the delete operation.

### DeleteRangeAsync

Deletes all keys with a specified prefix asynchronously.

#### DeleteRangeAsync Overload

```csharp
public async Task<DeleteRangeResponse> DeleteRangeAsync(
  string prefixKey,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### DeleteRangeAsync Parameters

- `prefixKey`: The prefix to match keys against.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### DeleteRangeAsync Returns

- `Task<DeleteRangeResponse>`: The etcd response for the delete operation.

## Compact Operations

### Compact

Compacts the key-value store up to a given revision.

#### Compact Overload

```csharp
public CompactionResponse Compact(
  CompactionRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### Compact Parameters

- `request`: The CompactionRequest to send to the server.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Compact Returns

- `CompactionResponse`: The etcd response for the compact operation.

### CompactAsync

Compacts the key-value store up to a given revision asynchronously.

#### CompactAsync Overload

```csharp
public async Task<CompactionResponse> CompactAsync(
  CompactionRequest request,
  Grpc.Core.Metadata headers = null,
  DateTime? deadline = null,
  CancellationToken cancellationToken = default)
```

#### CompactAsync Parameters

- `request`: The CompactionRequest to send to the server.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### CompactAsync Returns

- `Task<CompactionResponse>`: The etcd response for the compact operation.

## See Also

- [Key-Value Operations](index.md) - Overview and examples of key-value operations
- [Watch API Reference](../watch/api-reference.md) - API reference for watch operations
- [Transaction API Reference](../transactions/api-reference.md) - API reference for transaction operations
