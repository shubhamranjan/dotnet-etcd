# Dependency Injection

The dotnet-etcd client provides built-in support for Microsoft.Extensions.DependencyInjection, allowing you to easily integrate etcd into your dependency injection-enabled applications.

## Basic Usage

```csharp
// In your Program.cs or Startup.cs
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.UseInsecureChannel = true;
});
```

Then inject `IEtcdClient` into your services:

```csharp
public class MyService
{
    private readonly IEtcdClient _etcdClient;

    public MyService(IEtcdClient etcdClient)
    {
        _etcdClient = etcdClient;
    }
}
```

## Configuration Options

### Connection Configuration

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.Port = 2379;                    // Optional, defaults to 2379
    options.UseInsecureChannel = false;     // Optional, defaults to false
    options.ServerName = "my-etcd-server";  // Optional
});
```

### Authentication Configuration

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.CallCredentials = CallCredentials.FromInterceptor((context, metadata) => {
        metadata.Add("token", "my-auth-token");
        return Task.CompletedTask;
    });
});
```

### Channel Configuration

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.ConfigureChannel = channelOptions => {
        channelOptions.HttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };
    };
});
```

### Interceptor Configuration

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.Interceptors = new[] {
        new ClientLoggingInterceptor(),
        new MetricsInterceptor()
    };
});
```

## Registration Lifetime

The etcd client is registered as a singleton service by default. This means the same client instance will be used throughout the lifetime of your application.

## Multiple Client Registration

If you need to register multiple etcd clients with different configurations:

```csharp
// Register the default client
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
});

// Register a named client
services.AddEtcdClient("secondary", options => {
    options.ConnectionString = "localhost:2380";
});

// Inject the named client using IEtcdClientFactory
public class MyService
{
    private readonly IEtcdClient _defaultClient;
    private readonly IEtcdClient _secondaryClient;

    public MyService(
        IEtcdClient defaultClient,
        IEtcdClientFactory clientFactory)
    {
        _defaultClient = defaultClient;
        _secondaryClient = clientFactory.CreateClient("secondary");
    }
}
```

## Watch Manager Registration

The watch manager is automatically registered and can be injected separately if needed:

```csharp
public class MyService
{
    private readonly IWatchManager _watchManager;

    public MyService(IWatchManager watchManager)
    {
        _watchManager = watchManager;
    }
}
```

## Best Practices

1. **Configuration**: Store connection strings and other configuration in your application's configuration system (appsettings.json, environment variables, etc.)

```csharp
services.AddEtcdClient(configuration.GetSection("Etcd"));
```

2. **Disposal**: The client is automatically disposed when the application shuts down. No manual disposal is needed when using dependency injection.

3. **Error Handling**: Consider registering custom interceptors for logging and error handling:

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.Interceptors = new[] {
        new RetryInterceptor(maxRetries: 3),
        new LoggingInterceptor()
    };
    options.EnableRetryPolicy = true;
});
```

## Examples

### ASP.NET Core Web API

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add etcd client
builder.Services.AddEtcdClient(options => {
    options.ConnectionString = builder.Configuration["Etcd:ConnectionString"];
    options.UseInsecureChannel = builder.Configuration.GetValue<bool>("Etcd:UseInsecureChannel");
});

// Use in controller
public class ValuesController : ControllerBase
{
    private readonly IEtcdClient _etcdClient;

    public ValuesController(IEtcdClient etcdClient)
    {
        _etcdClient = etcdClient;
    }

    [HttpGet("config/{key}")]
    public async Task<IActionResult> GetConfig(string key)
    {
        try
        {
            var response = await _etcdClient.GetAsync(key);
            return Ok(response.Kvs[0].Value.ToStringUtf8());
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return NotFound();
        }
    }
}
```

### Worker Service

```csharp
public class Worker : BackgroundService
{
    private readonly IEtcdClient _etcdClient;
    private readonly IWatchManager _watchManager;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IEtcdClient etcdClient,
        IWatchManager watchManager,
        ILogger<Worker> logger)
    {
        _etcdClient = etcdClient;
        _watchManager = watchManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _watchManager.WatchAsync(
            "config/",
            (response) => {
                foreach (var evt in response.Events)
                {
                    _logger.LogInformation(
                        "Config changed: {Key} = {Value}",
                        evt.Kv.Key.ToStringUtf8(),
                        evt.Kv.Value.ToStringUtf8());
                }
            },
            stoppingToken);
    }
}
```

## See Also

- [Client Initialization](index.md) - Other ways to initialize the client
- [API Reference](api-reference.md) - Complete API reference
- [Authentication](../authentication/index.md) - Authentication options
- [Watch Operations](../watch/index.md) - Watch functionality details