# Watch API Reference

This page provides a complete reference of all watch-related methods available in the `dotnet-etcd` client.

## Watch Methods

### Watch

Watches for changes to keys.

#### Watch Overloads

```csharp
// Watch a key with a callback
public void Watch(
    string key,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a key with a callback and watch options
public void Watch(
    string key,
    Action<WatchResponse> callback,
    WatchOption watchOption,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a key with a WatchRequest
public void Watch(
    WatchRequest watchRequest,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### Watch Parameters

- `key`: The key to watch.
- `callback`: The callback to invoke when changes are detected.
- `watchOption`: Options for the watch operation.
- `watchRequest`: The WatchRequest to send to the server.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

### WatchAsync

Watches for changes to keys asynchronously.

#### WatchAsync Overloads

```csharp
// Watch a key with a callback asynchronously
public Task WatchAsync(
    string key,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a key with a callback and watch options asynchronously
public Task WatchAsync(
    string key,
    Action<WatchResponse> callback,
    WatchOption watchOption,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a key with a WatchRequest asynchronously
public Task WatchAsync(
    WatchRequest watchRequest,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### WatchAsync Parameters

- `key`: The key to watch.
- `callback`: The callback to invoke when changes are detected.
- `watchOption`: Options for the watch operation.
- `watchRequest`: The WatchRequest to send to the server.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### WatchAsync Returns

- `Task`: A task that represents the asynchronous watch operation.

## Range Watch Methods

### WatchRange

Watches for changes to a range of keys.

#### WatchRange Overloads

```csharp
// Watch a range of keys with a callback
public void WatchRange(
    string prefixKey,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a range of keys with a callback and watch options
public void WatchRange(
    string prefixKey,
    Action<WatchResponse> callback,
    WatchOption watchOption,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### WatchRange Parameters

- `prefixKey`: The prefix to match keys against.
- `callback`: The callback to invoke when changes are detected.
- `watchOption`: Options for the watch operation.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

### WatchRangeAsync

Watches for changes to a range of keys asynchronously.

#### WatchRangeAsync Overloads

```csharp
// Watch a range of keys with a callback asynchronously
public Task WatchRangeAsync(
    string prefixKey,
    Action<WatchResponse> callback,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)

// Watch a range of keys with a callback and watch options asynchronously
public Task WatchRangeAsync(
    string prefixKey,
    Action<WatchResponse> callback,
    WatchOption watchOption,
    Grpc.Core.Metadata headers = null,
    DateTime? deadline = null,
    CancellationToken cancellationToken = default)
```

#### WatchRangeAsync Parameters

- `prefixKey`: The prefix to match keys against.
- `callback`: The callback to invoke when changes are detected.
- `watchOption`: Options for the watch operation.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### WatchRangeAsync Returns

- `Task`: A task that represents the asynchronous watch operation.

## Watch Options

The `WatchOption` class provides options for watch operations.

### WatchOption Properties

- `Revision`: Start watching from a specific revision.
- `PrevKv`: If true, return the previous key-value pair before the event happens.
- `ProgressNotify`: If true, periodically receive a WatchResponse with no events to update the progress.
- `Filters`: A list of event types to filter out.
- `Fragment`: If true, watch server-side events with multiple chunks.

### WatchOption Example

```csharp
// Create watch options
var watchOption = new WatchOption
{
    Revision = 0, // Start watching from the current revision
    PrevKv = true, // Include previous key-value pairs
    ProgressNotify = true, // Receive progress updates
    Filters = new List<WatchCreateRequest.Types.FilterType>
    {
        // Filter out PUT events
        WatchCreateRequest.Types.FilterType.Nodelete
    }
};

// Watch with options
client.Watch("my-key", response =>
{
    foreach (var evt in response.Events)
    {
        Console.WriteLine($"Event type: {evt.Type}");
        Console.WriteLine($"Key: {evt.Kv.Key.ToStringUtf8()}");
        Console.WriteLine($"Value: {evt.Kv.Value.ToStringUtf8()}");
        
        if (evt.PrevKv != null)
        {
            Console.WriteLine($"Previous value: {evt.PrevKv.Value.ToStringUtf8()}");
        }
    }
}, watchOption);
```

## Watch Response

The `WatchResponse` class represents the response from a watch operation.

### WatchResponse Properties

- `Header`: The response header.
- `WatchId`: The ID of the watch.
- `Created`: Whether the watch was created in this response.
- `Canceled`: Whether the watch was canceled in this response.
- `CompactRevision`: The minimum revision the watcher may receive.
- `Events`: The list of events that occurred.

### Event Types

The `Event.Types.EventType` enum represents the type of event that occurred:

- `Put`: A key was created or updated.
- `Delete`: A key was deleted.

### WatchResponse Example

```csharp
client.Watch("my-key", response =>
{
    if (response.Created)
    {
        Console.WriteLine($"Watch created with ID: {response.WatchId}");
    }
    else if (response.Canceled)
    {
        Console.WriteLine($"Watch canceled: {response.CancelReason}");
        return;
    }
    
    foreach (var evt in response.Events)
    {
        switch (evt.Type)
        {
            case Event.Types.EventType.Put:
                Console.WriteLine($"Key created/updated: {evt.Kv.Key.ToStringUtf8()}");
                Console.WriteLine($"New value: {evt.Kv.Value.ToStringUtf8()}");
                break;
                
            case Event.Types.EventType.Delete:
                Console.WriteLine($"Key deleted: {evt.Kv.Key.ToStringUtf8()}");
                break;
        }
    }
});
```

## See Also

- [Watch Operations](index.md) - Overview and examples of watch operations
- [Key-Value API Reference](../key-value/api-reference.md) - API reference for key-value operations
- [Lease API Reference](../lease/api-reference.md) - API reference for lease operations
