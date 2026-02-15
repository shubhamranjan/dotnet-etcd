# Watch Operations

This page documents how to use watch operations in etcd using the `dotnet-etcd` client.

## Overview

etcd provides watch operations that allow you to monitor changes to keys or ranges of keys. When a key is created, updated, or deleted, etcd will notify all watchers of that key. This is useful for building distributed systems that need to react to changes in configuration or state.

## Watching a Key

To watch a single key for changes:

```csharp
// Create a watcher for a key
long watchId = client.Watch("my-key", (response) =>
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

## Automatic Reconnection

The client includes robust automatic reconnection logic for watches. If the network connection is lost or the etcd server restarts:
1.  The client detects the failure (via gRPC stream error or connection callback).
2.  It automatically attempts to reconnect to the server, with exponential backoff.
3.  Once connected, it re-establishes all active watches transparently.

No manual intervention is required to recover watches after a disruption.

## Watching a Key Range

To watch a range of keys for changes:

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

## Watching from a Specific Revision

To watch a key from a specific revision, use a `WatchRequest` with `StartRevision` set:

```csharp
// Get the current revision
var getResponse = client.Get("my-key");
long startRevision = getResponse.Header.Revision;

// Create a watch request with a start revision
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        StartRevision = startRevision,
        ProgressNotify = true,
        PrevKv = true
    }
};

// Create a watcher starting from the specified revision
long watchId = client.Watch(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}' at revision {evt.Kv.ModRevision}");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted at revision {evt.Kv.ModRevision}");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

You can also watch a range of keys from a specific revision:

```csharp
// Get the current revision
var getResponse = client.GetRange("config/");
long startRevision = getResponse.Header.Revision;

// Create a watch request for a range with a start revision
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("config/"),
        RangeEnd = ByteString.CopyFromUtf8(EtcdClient.GetRangeEnd("config/")),
        StartRevision = startRevision,
        ProgressNotify = true,
        PrevKv = true
    }
};

// Create a watcher for the range starting from the specified revision
long watchId = client.Watch(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}' at revision {evt.Kv.ModRevision}");
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted at revision {evt.Kv.ModRevision}");
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

## Watching with Filters

To watch a key with filters:

```csharp
// Create a watch request with filters
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        FilterType = WatchCreateRequest.Types.FilterType.NodePut // Only watch for Put events
    }
};

// Create a watcher with the request
long watchId = client.Watch(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        // This will only include Put events due to the filter
        string key = evt.Kv.Key.ToStringUtf8();
        string value = evt.Kv.Value.ToStringUtf8();
        Console.WriteLine($"Key '{key}' was put with value '{value}'");
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

## Watching with Progress Notifications

To watch a key with progress notifications:

```csharp
// Create a watch request with progress notifications
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        ProgressNotify = true
    }
};

// Create a watcher with the request
long watchId = client.Watch(watchRequest, (response) =>
{
    if (response.Events.Count == 0 && response.CompactRevision == 0)
    {
        Console.WriteLine($"Received progress notification at revision {response.Header.Revision}");
        return;
    }

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

## Watching with Previous Values

To watch a key with previous values:

```csharp
// Create a watch request with previous values
var watchRequest = new WatchRequest
{
    CreateRequest = new WatchCreateRequest
    {
        Key = ByteString.CopyFromUtf8("my-key"),
        PrevKv = true
    }
};

// Create a watcher with the request
long watchId = client.Watch(watchRequest, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            Console.WriteLine($"Key '{key}' was put with value '{value}'");
            
            if (evt.PrevKv != null)
            {
                string prevValue = evt.PrevKv.Value.ToStringUtf8();
                Console.WriteLine($"Previous value was '{prevValue}'");
            }
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            Console.WriteLine($"Key '{key}' was deleted");
            
            if (evt.PrevKv != null)
            {
                string prevValue = evt.PrevKv.Value.ToStringUtf8();
                Console.WriteLine($"Deleted value was '{prevValue}'");
            }
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Cancel the watch when done
client.CancelWatch(watchId);
```

## Handling Watch Errors

To handle errors that occur during watching:

```csharp
try
{
    // Create a watcher with error handling
    long watchId = client.Watch("my-key", (response) =>
    {
        // Process watch events
        foreach (var evt in response.Events)
        {
            // Process events as normal
        }
    },
    (exception) =>
    {
        // Handle any errors that occur during watching
        Console.WriteLine($"Watch error: {exception.Message}");
        
        // You might want to retry or take other actions
        // depending on the error
    });
    
    // Do some work while watching
    await Task.Delay(TimeSpan.FromMinutes(1));
    
    // Cancel the watch when done
    client.CancelWatch(watchId);
}
catch (Exception ex)
{
    // Handle any errors that occur during watch setup
    Console.WriteLine($"Error setting up watch: {ex.Message}");
}
```

## Implementing a Configuration Watcher

You can implement a configuration watcher that reacts to changes in configuration:

```csharp
public class ConfigurationWatcher : IDisposable
{
    private readonly EtcdClient _client;
    private readonly string _configPrefix;
    private readonly Dictionary<string, string> _currentConfig = new Dictionary<string, string>();
    private IWatcher _watcher;
    
    public event EventHandler<ConfigChangedEventArgs> ConfigChanged;
    
    public ConfigurationWatcher(EtcdClient client, string configPrefix)
    {
        _client = client;
        _configPrefix = configPrefix;
    }
    
    public async Task StartAsync()
    {
        // Load initial configuration
        var response = await _client.GetRangeAsync(_configPrefix, WatchRange.Prefix);
        foreach (var kv in response.Kvs)
        {
            string key = kv.Key.ToStringUtf8().Substring(_configPrefix.Length);
            string value = kv.Value.ToStringUtf8();
            _currentConfig[key] = value;
        }
        
        // Start watching for changes
        _watcher = _client.Watch(_configPrefix, WatchRange.Prefix, (watchResponse) =>
        {
            var changedKeys = new List<string>();
            
            foreach (var evt in watchResponse.Events)
            {
                string fullKey = evt.Kv.Key.ToStringUtf8();
                string key = fullKey.Substring(_configPrefix.Length);
                
                if (evt.Type == Event.Types.EventType.Put)
                {
                    string value = evt.Kv.Value.ToStringUtf8();
                    
                    if (!_currentConfig.TryGetValue(key, out var currentValue) || currentValue != value)
                    {
                        _currentConfig[key] = value;
                        changedKeys.Add(key);
                    }
                }
                else if (evt.Type == Event.Types.EventType.Delete)
                {
                    if (_currentConfig.Remove(key))
                    {
                        changedKeys.Add(key);
                    }
                }
            }
            
            if (changedKeys.Count > 0)
            {
                OnConfigChanged(new ConfigChangedEventArgs(changedKeys, _currentConfig));
            }
        });
    }
    
    public string GetValue(string key, string defaultValue = null)
    {
        return _currentConfig.TryGetValue(key, out var value) ? value : defaultValue;
    }
    
    public int GetIntValue(string key, int defaultValue = 0)
    {
        if (_currentConfig.TryGetValue(key, out var value) && int.TryParse(value, out var intValue))
        {
            return intValue;
        }
        
        return defaultValue;
    }
    
    public bool GetBoolValue(string key, bool defaultValue = false)
    {
        if (_currentConfig.TryGetValue(key, out var value) && bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }
        
        return defaultValue;
    }
    
    protected virtual void OnConfigChanged(ConfigChangedEventArgs e)
    {
        ConfigChanged?.Invoke(this, e);
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class ConfigChangedEventArgs : EventArgs
{
    public IReadOnlyList<string> ChangedKeys { get; }
    public IReadOnlyDictionary<string, string> CurrentConfig { get; }
    
    public ConfigChangedEventArgs(IReadOnlyList<string> changedKeys, IReadOnlyDictionary<string, string> currentConfig)
    {
        ChangedKeys = changedKeys;
        CurrentConfig = currentConfig;
    }
}

// Usage
var client = new EtcdClient("localhost:2379");
var configWatcher = new ConfigurationWatcher(client, "config/");

configWatcher.ConfigChanged += (sender, e) =>
{
    Console.WriteLine("Configuration changed:");
    foreach (var key in e.ChangedKeys)
    {
        if (e.CurrentConfig.TryGetValue(key, out var value))
        {
            Console.WriteLine($"  {key} = {value}");
        }
        else
        {
            Console.WriteLine($"  {key} was removed");
        }
    }
};

await configWatcher.StartAsync();

// Use the configuration
int maxConnections = configWatcher.GetIntValue("max-connections", 100);
bool enableFeature = configWatcher.GetBoolValue("enable-feature", false);

// Keep the application running
await Task.Delay(TimeSpan.FromHours(1));

// Clean up
configWatcher.Dispose();
```

## Implementing a Service Discovery Client

You can implement a simple service discovery client using watches:

```csharp
public class ServiceDiscoveryClient : IDisposable
{
    private readonly EtcdClient _client;
    private readonly string _servicesPrefix;
    private readonly Dictionary<string, List<ServiceInstance>> _services = new Dictionary<string, List<ServiceInstance>>();
    private IWatcher _watcher;
    
    public event EventHandler<ServiceChangedEventArgs> ServiceChanged;
    
    public ServiceDiscoveryClient(EtcdClient client, string servicesPrefix)
    {
        _client = client;
        _servicesPrefix = servicesPrefix;
    }
    
    public async Task StartAsync()
    {
        // Load initial services
        var response = await _client.GetRangeAsync(_servicesPrefix, WatchRange.Prefix);
        foreach (var kv in response.Kvs)
        {
            ProcessServiceKey(kv.Key.ToStringUtf8(), kv.Value.ToStringUtf8(), false);
        }
        
        // Start watching for changes
        _watcher = _client.Watch(_servicesPrefix, WatchRange.Prefix, (watchResponse) =>
        {
            var changedServices = new HashSet<string>();
            
            foreach (var evt in watchResponse.Events)
            {
                string key = evt.Kv.Key.ToStringUtf8();
                
                if (evt.Type == Event.Types.EventType.Put)
                {
                    string value = evt.Kv.Value.ToStringUtf8();
                    string serviceName = ProcessServiceKey(key, value, true);
                    if (serviceName != null)
                    {
                        changedServices.Add(serviceName);
                    }
                }
                else if (evt.Type == Event.Types.EventType.Delete)
                {
                    string serviceName = RemoveServiceKey(key);
                    if (serviceName != null)
                    {
                        changedServices.Add(serviceName);
                    }
                }
            }
            
            foreach (var serviceName in changedServices)
            {
                OnServiceChanged(new ServiceChangedEventArgs(serviceName, GetServiceInstances(serviceName)));
            }
        });
    }
    
    private string ProcessServiceKey(string key, string value, bool notifyChange)
    {
        if (!key.StartsWith(_servicesPrefix))
        {
            return null;
        }
        
        string relativePath = key.Substring(_servicesPrefix.Length);
        string[] parts = relativePath.Split('/');
        
        if (parts.Length != 2)
        {
            return null;
        }
        
        string serviceName = parts[0];
        string instanceId = parts[1];
        
        var instance = JsonSerializer.Deserialize<ServiceInstance>(value);
        instance.Id = instanceId;
        
        if (!_services.TryGetValue(serviceName, out var instances))
        {
            instances = new List<ServiceInstance>();
            _services[serviceName] = instances;
        }
        
        // Remove existing instance with the same ID
        instances.RemoveAll(i => i.Id == instanceId);
        
        // Add the new/updated instance
        instances.Add(instance);
        
        return serviceName;
    }
    
    private string RemoveServiceKey(string key)
    {
        if (!key.StartsWith(_servicesPrefix))
        {
            return null;
        }
        
        string relativePath = key.Substring(_servicesPrefix.Length);
        string[] parts = relativePath.Split('/');
        
        if (parts.Length != 2)
        {
            return null;
        }
        
        string serviceName = parts[0];
        string instanceId = parts[1];
        
        if (_services.TryGetValue(serviceName, out var instances))
        {
            instances.RemoveAll(i => i.Id == instanceId);
            
            if (instances.Count == 0)
            {
                _services.Remove(serviceName);
            }
        }
        
        return serviceName;
    }
    
    public List<ServiceInstance> GetServiceInstances(string serviceName)
    {
        return _services.TryGetValue(serviceName, out var instances)
            ? new List<ServiceInstance>(instances)
            : new List<ServiceInstance>();
    }
    
    protected virtual void OnServiceChanged(ServiceChangedEventArgs e)
    {
        ServiceChanged?.Invoke(this, e);
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class ServiceInstance
{
    public string Id { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class ServiceChangedEventArgs : EventArgs
{
    public string ServiceName { get; }
    public List<ServiceInstance> Instances { get; }
    
    public ServiceChangedEventArgs(string serviceName, List<ServiceInstance> instances)
    {
        ServiceName = serviceName;
        Instances = instances;
    }
}

// Usage
var client = new EtcdClient("localhost:2379");
var serviceDiscovery = new ServiceDiscoveryClient(client, "services/");

serviceDiscovery.ServiceChanged += (sender, e) =>
{
    Console.WriteLine($"Service '{e.ServiceName}' changed:");
    foreach (var instance in e.Instances)
    {
        Console.WriteLine($"  {instance.Id}: {instance.Host}:{instance.Port}");
    }
};

await serviceDiscovery.StartAsync();

// Get instances of a service
var apiInstances = serviceDiscovery.GetServiceInstances("api-service");
if (apiInstances.Count > 0)
{
    // Use a simple round-robin load balancing
    var instance = apiInstances[new Random().Next(apiInstances.Count)];
    Console.WriteLine($"Connecting to API at {instance.Host}:{instance.Port}");
}

// Keep the application running
await Task.Delay(TimeSpan.FromHours(1));

// Clean up
serviceDiscovery.Dispose();
```

## Canceling Watches

You can cancel a watch operation at any time using the `CancelWatch` method:

```csharp
// Create a watcher
long watchId = client.Watch("my-key", (response) => { /* ... */ });

// Cancel the watch when you're done with it
client.CancelWatch(watchId);
```

You can also cancel multiple watches at once:

```csharp
// Create multiple watchers
long watchId1 = client.Watch("key1", (response) => { /* ... */ });
long watchId2 = client.Watch("key2", (response) => { /* ... */ });

// Cancel multiple watches
client.CancelWatch(new long[] { watchId1, watchId2 });
```

## See Also

- [API Reference](api-reference.md) - Complete API reference for watch operations
- [Key-Value Operations](../key-value/index.md) - Working with key-value operations
- [Lease Operations](../lease/index.md) - Working with leases
