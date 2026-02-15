# Watch API Reference

This page provides a complete reference of all watch-related methods available in the `dotnet-etcd` client.

## Watch Methods

### Watch

Creates a watcher for a key or key range. Returns a watch ID that can be used to cancel the watch.

#### Overloads

```csharp
public void Watch(string key, Action<WatchResponse> callback)
public void Watch(string key, Action<WatchResponse>[] callbacks)
public void Watch(string[] keys, Action<WatchResponse> callback)
public void Watch(string[] keys, Action<WatchResponse>[] callbacks)
public long Watch(WatchRequest watchRequest, Action<WatchResponse> callback)
public long Watch(WatchRequest watchRequest, Action<WatchResponse>[] callbacks)
public long Watch(WatchRequest[] watchRequests, Action<WatchResponse>[] callbacks)
```

#### Parameters

- `key`: The key to watch.
- `keys`: An array of keys to watch.
- `watchRequest`: A watch request containing watch options (including `StartRevision` for watching from a specific revision).
- `watchRequests`: An array of watch requests.
- `callback`: A callback function that is called when a watch event is received.
- `callbacks`: An array of callback functions that are called when watch events are received.

#### Returns

- `void` for `Watch(string, ...)` and `Watch(string[], ...)` overloads - these methods don't return a watch ID.
- `long` for `Watch(WatchRequest, ...)` and `Watch(WatchRequest[], ...)` overloads - a watch ID that can be used to cancel the watch.

#### Example

```csharp
// Create a watcher for a key using WatchRequest to get a watch ID
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        ProgressNotify = true,
        PrevKv = true
    }
};

long watchId = client.Watch(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

**Note**: To watch from a specific revision, use the `WatchRequest` overload with `StartRevision` set in the `WatchCreateRequest`. See the [Watch Options](#watch-options) section for details.

### WatchAsync

Creates a watcher for a key or key range asynchronously. Returns a watch ID that can be used to cancel the watch.

#### Overloads

```csharp
public Task<long> WatchAsync(WatchRequest watchRequest, Action<WatchResponse> callback)
public Task<long> WatchAsync(WatchRequest watchRequest, Action<WatchResponse>[] callbacks)
public Task<long[]> WatchAsync(WatchRequest[] watchRequests, Action<WatchResponse> callback)
public Task<long[]> WatchAsync(WatchRequest[] watchRequests, Action<WatchResponse>[] callbacks)
```

#### Parameters

- `watchRequest`: A single watch request containing watch options (including `StartRevision` for watching from a specific revision).
- `watchRequests`: An array of watch requests.
  - When used with a single `callback`, the same callback is invoked for all watch requests.
  - When used with `callbacks` array, each callback corresponds to its respective watch request.
- `callback`: A callback function that is called when a watch event is received.
  - For single `watchRequest` overloads, called for events from that watch.
  - For `watchRequests` array overloads, this single callback handles events from all watches.
- `callbacks`: An array of callback functions where each callback at index i handles events from the watch request at index i.

#### Returns

- `Task<long>`: A task that represents the asynchronous operation. The task result contains a watch ID that can be used to cancel the watch.
- `Task<long[]>`: For array overloads, a task that represents the asynchronous operation. The task result contains an array of watch IDs.

#### Example

```csharp
// Create a watcher for a key asynchronously using WatchRequest
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        ProgressNotify = true,
        PrevKv = true
    }
};

long watchId = await client.WatchAsync(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

### CancelWatch

Cancels a watch operation.

#### CancelWatch Overloads

```csharp
// Cancel a single watch
public void CancelWatch(long watchId)

// Cancel multiple watches
public void CancelWatch(long[] watchIds)
```

#### CancelWatch Parameters

- `watchId`: The ID of the watch to cancel.
- `watchIds`: The IDs of the watches to cancel.

## Range Watch Methods

### WatchRange

Creates a watcher for a range of keys. Returns a watch ID that can be used to cancel the watch.

#### Overloads

```csharp
public long WatchRange(string prefixKey, Action<WatchResponse> callback)
public long WatchRange(string prefixKey, Action<WatchResponse>[] callbacks)
public long[] WatchRange(string[] prefixKeys, Action<WatchResponse> callback)
public long[] WatchRange(string[] prefixKeys, Action<WatchResponse>[] callbacks)
```

#### Parameters

- `prefixKey`: The prefix key to watch. All keys with this prefix will be watched.
- `prefixKeys`: An array of prefix keys to watch.
- `callback`: A callback function that is called when a watch event is received.
- `callbacks`: An array of callback functions that are called when watch events are received.

#### Returns

- `long`: A watch ID that can be used to cancel the watch.

#### Example

```csharp
// Create a watcher for a key range (all keys with prefix "config/")
long watchId = client.WatchRange("config/", (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

### WatchRangeAsync

Creates a watcher for a range of keys asynchronously. Returns a watch ID that can be used to cancel the watch.

#### Overloads

```csharp
public Task<long> WatchRangeAsync(string prefixKey, Action<WatchResponse> callback)
public Task<long> WatchRangeAsync(string prefixKey, Action<WatchResponse>[] callbacks)
public Task<long[]> WatchRangeAsync(string[] prefixKeys, Action<WatchResponse> callback)
public Task<long[]> WatchRangeAsync(string[] prefixKeys, Action<WatchResponse>[] callbacks)
```

#### Parameters

- `prefixKey`: The prefix key to watch. All keys with this prefix will be watched.
- `prefixKeys`: An array of prefix keys to watch.
- `callback`: A callback function that is called when a watch event is received.
- `callbacks`: An array of callback functions that are called when watch events are received.

#### Returns

- `Task<long>`: A task that represents the asynchronous operation. The task result contains a watch ID that can be used to cancel the watch.

#### Example

```csharp
// Create a watcher for a key range asynchronously (all keys with prefix "config/")
long watchId = await client.WatchRangeAsync("config/", (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

## Watch Options

The `WatchCreateRequest` class provides options for configuring watch operations.

### WatchCreateRequest Properties

- `Key`: The key to watch.
- `RangeEnd`: The end of the range [key, rangeEnd) to watch. If not set, only the key is watched. If set to `\0`, all keys greater than or equal to the key are watched.
- `StartRevision`: The revision to start watching from. If not set, starts from the current revision.
- `ProgressNotify`: If set, the watch will periodically receive a WatchResponse with no events when there are no recent events. It is useful when clients wish to recover a disconnected watcher starting from a recent known revision.
- `FilterType`: The type of events to filter out. By default, no events are filtered out.
- `PrevKv`: If set, created watcher gets the previous key-value before the event happens. If the previous key-value is already compacted, nothing will be returned.
- `WatchId`: The ID of the watcher. If set, the server will use this ID instead of generating one.
- `Fragment`: If set, the server will split large watch events into multiple responses.

### WatchCreateRequest Example

```csharp
// Create a watch request with various options
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        RangeEnd = ByteString.CopyFromUtf8("my-key\0"), // Watch all keys with prefix "my-key"
        StartRevision = 100, // Start watching from revision 100
        ProgressNotify = true, // Receive progress notifications
        PrevKv = true, // Include previous key-value pairs
        FilterType = WatchCreateRequest.Types.FilterType.NodePut // Only watch for Put events
    }
};

// Create a watcher with the request
long watchId = client.Watch(watchRequest, (response) =>
{
    // Process watch events
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

## Watch Response

The `WatchResponse`

### WatchResponse

The response from a watch operation.

#### Properties

- `Header`: The response header.
- `Events`: The list of events that occurred.
- `CompactRevision`: The compact revision. If not 0, indicates all revisions <= CompactRevision have been compacted.
- `Canceled`: Indicates whether the watch has been canceled by the server.
- `Created`: Indicates whether the watch was created in this response.

#### Example

```csharp
client.Watch("my-key", (response) =>
{
    // Check if the watch was created
    if (response.Created)
    {
        Console.WriteLine("Watch created successfully");
    }
    
    // Check if the watch was canceled
    if (response.Canceled)
    {
        Console.WriteLine("Watch was canceled");
        return;
    }
    
    // Check if compaction occurred
    if (response.CompactRevision != 0)
    {
        Console.WriteLine($"Compaction occurred at revision {response.CompactRevision}");
    }
    
    // Process events
    foreach (var evt in response.Events)
    {
        // Process each event
    }
});
```

### WatchEvent

Represents a single watch event.

#### Properties

- `Type`: The type of event (Put or Delete).
- `Kv`: The key-value pair that was changed.
- `PrevKv`: The previous key-value pair (if requested).

#### Example

```csharp
client.Watch("my-key", (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
        }
    }
});
```

## See Also

- [Watch Operations](index.md)
- [Key-Value API Reference](../key-value/api-reference.md)
- [Client Initialization](../client-initialization/api-reference.md)