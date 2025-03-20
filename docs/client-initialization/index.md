# Client Initialization

The `EtcdClient` class is the main entry point for interacting with etcd. This page documents the various ways to initialize the client.

## Constructor Overloads

### Basic Constructor
```csharp
// Single endpoint
var client = new EtcdClient("localhost:2379");

// Multiple endpoints
var client = new EtcdClient("https://localhost:23790,https://localhost:23791");
```

### Advanced Constructor
```csharp
var client = new EtcdClient(
    connectionString: "localhost:2379",
    port: 2379,
    serverName: "my-etcd-server",
    configureChannelOptions: options => {
        options.Credentials = ChannelCredentials.Insecure;
    });
```

### Dependency Injection
```csharp
// In Program.cs or Startup.cs
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.UseInsecureChannel = true;
});

// Then inject IEtcdClient into your services
public class MyService
{
    private readonly IEtcdClient _etcdClient;

    public MyService(IEtcdClient etcdClient)
    {
        _etcdClient = etcdClient;
    }
}
```

For detailed DI configuration options, see [Dependency Injection](dependency-injection.md).

## Connection String Formats

### Basic Format
```
hostname:port
```

### Multiple Endpoints
```
host1:port1,host2:port2,host3:port3
```

### With Protocol
```
http://hostname:port
https://hostname:port
```

## Secure vs. Insecure Connections

### Insecure (HTTP)
```csharp
// Using constructor
var client = new EtcdClient(
    "localhost:2379",
    configureChannelOptions: options => {
        options.Credentials = ChannelCredentials.Insecure;
    });

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.UseInsecureChannel = true;
});
```

### Secure (HTTPS)
```csharp
// Using constructor
var client = new EtcdClient(
    "https://localhost:2379",
    configureChannelOptions: options => {
        options.Credentials = new SslCredentials();
    });

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "https://localhost:2379";
    options.CallCredentials = CallCredentials.FromInterceptor((context, metadata) => {
        metadata.Add("token", "my-auth-token");
        return Task.CompletedTask;
    });
});
```

## Advanced Configuration

### Custom HTTP Handler
```csharp
var handler = new SocketsHttpHandler
{
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
    EnableMultipleHttp2Connections = true
};

// Using constructor
var client = new EtcdClient(
    "localhost:2379",
    configureChannelOptions: options => {
        options.HttpHandler = handler;
    });

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.ConfigureChannel = channelOptions => {
        channelOptions.HttpHandler = handler;
    };
});
```

### Client Certificates
```csharp
var clientCertificate = new X509Certificate2("client.pfx", "password");
var channelCredentials = new SslCredentials(
    rootCertificates: File.ReadAllText("ca.crt"),
    keyCertificatePair: new KeyCertificatePair(
        File.ReadAllText("client.crt"),
        File.ReadAllText("client.key")
    ));

// Using constructor
var client = new EtcdClient(
    "https://localhost:2379",
    configureChannelOptions: options => {
        options.Credentials = channelCredentials;
    });

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "https://localhost:2379";
    options.ConfigureChannel = channelOptions => {
        channelOptions.Credentials = channelCredentials;
    };
});
```

### Custom Interceptors
```csharp
var interceptors = new[]
{
    new LoggingInterceptor(),
    new MetricsInterceptor(),
    new RetryInterceptor()
};

// Using constructor
var client = new EtcdClient(
    "localhost:2379",
    interceptors: interceptors);

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.Interceptors = interceptors;
});
```

### Custom Retry Policy
```csharp
// Using constructor
var client = new EtcdClient(
    "localhost:2379",
    retryPolicy: new ExponentialBackoffRetryPolicy(maxRetries: 5));

// Using DI
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.EnableRetryPolicy = true;
});
```

## See Also

- [Dependency Injection](dependency-injection.md) - Detailed DI configuration
- [API Reference](api-reference.md) - Complete API reference
- [Authentication](../authentication/index.md) - Authentication options
