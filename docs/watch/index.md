# Watch Operations

This page documents how to use watch operations in etcd using the `dotnet-etcd` client.

## Overview

etcd provides watch operations that allow you to monitor changes to keys or ranges of keys. When a key is created, updated, or deleted, etcd will notify all watchers of that key. This is useful for building distributed systems that need to react to changes in configuration or state.

## Watching a Key

To watch a single key for changes:

```csharp
// Create a watcher for a key
var watcher = client.Watch("my-key", (response) =>
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

// Stop watching when done
watcher.Dispose();
```

## Watching a Key Range

To watch a range of keys for changes:

```csharp
// Create a watcher for a key range (all keys with prefix "config/")
var watcher = client.Watch("config/", WatchRange.Prefix, (response) =>
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

// Stop watching when done
watcher.Dispose();
```

## Watching from a Specific Revision

You can start watching from a specific revision:

```csharp
// Get the current revision
var getResponse = client.Get("any-key");
long currentRevision = getResponse.Header.Revision;

// Create a watcher starting from the current revision
var watcher = client.Watch("my-key", new WatchOptions { StartRevision = currentRevision }, (response) =>
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

// Stop watching when done
watcher.Dispose();
```

## Watching with Filters

You can filter the events you receive:

```csharp
// Create a watcher that only notifies about PUT events
var watcher = client.Watch("my-key", new WatchOptions { FilterType = WatchFilterType.Nodelete }, (response) =>
{
    foreach (var evt in response.Events)
    {
        // This will only include PUT events
        string key = evt.Kv.Key.ToStringUtf8();
        string value = evt.Kv.Value.ToStringUtf8();
        Console.WriteLine($"Key '{key}' was put with value '{value}'");
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Stop watching when done
watcher.Dispose();
```

## Watching with Progress Notifications

You can request progress notifications to ensure the watch is still active:

```csharp
// Create a watcher with progress notifications
var watcher = client.Watch("my-key", new WatchOptions { ProgressNotify = true }, (response) =>
{
    if (response.Events.Count == 0)
    {
        Console.WriteLine($"Received progress notification at revision {response.Header.Revision}");
    }
    else
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
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Stop watching when done
watcher.Dispose();
```

## Watching with Previous Values

You can include the previous value in the watch events:

```csharp
// Create a watcher that includes previous values
var watcher = client.Watch("my-key", new WatchOptions { PrevKv = true }, (response) =>
{
    foreach (var evt in response.Events)
    {
        string key = evt.Kv.Key.ToStringUtf8();
        
        if (evt.Type == Event.Types.EventType.Put)
        {
            string value = evt.Kv.Value.ToStringUtf8();
            
            if (evt.PrevKv != null)
            {
                string prevValue = evt.PrevKv.Value.ToStringUtf8();
                Console.WriteLine($"Key '{key}' was updated from '{prevValue}' to '{value}'");
            }
            else
            {
                Console.WriteLine($"Key '{key}' was created with value '{value}'");
            }
        }
        else if (evt.Type == Event.Types.EventType.Delete)
        {
            if (evt.PrevKv != null)
            {
                string prevValue = evt.PrevKv.Value.ToStringUtf8();
                Console.WriteLine($"Key '{key}' with value '{prevValue}' was deleted");
            }
            else
            {
                Console.WriteLine($"Key '{key}' was deleted");
            }
        }
    }
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Stop watching when done
watcher.Dispose();
```

## Handling Watch Errors

You can handle errors that occur during watching:

```csharp
// Create a watcher with error handling
var watcher = client.Watch("my-key", (response) =>
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
}, (ex) =>
{
    Console.WriteLine($"Watch error: {ex.Message}");
    
    // Decide whether to retry or not
    return true; // Return true to retry, false to stop watching
});

// Do some work while watching
await Task.Delay(TimeSpan.FromMinutes(1));

// Stop watching when done
watcher.Dispose();
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

## See Also

- [API Reference](api-reference.md) - Complete API reference for watch operations
- [Key-Value Operations](../key-value/index.md) - Working with key-value operations
- [Lease Operations](../lease/index.md) - Working with leases
