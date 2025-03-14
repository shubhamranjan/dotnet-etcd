# Client Initialization

The `EtcdClient` class is the main entry point for interacting with etcd. This page documents the various ways to initialize the client.

## Constructor Overloads

### Basic Constructor

```csharp
public EtcdClient(string connectionString, int port = 2379, string serverName = "my-etcd-server",
    Action<GrpcChannelOptions> configureChannelOptions = null, Interceptor[] interceptors = null)
```

#### Parameters

- `connectionString` (required): String containing comma-separated list of etcd endpoints.
- `port` (optional): Port to use when no port is specified in the endpoints. Default: 2379
- `serverName` (optional): Name used for the static resolver. Default: "my-etcd-server"
- `configureChannelOptions` (optional): Action to configure gRPC channel options. Can be used to set credentials for secure/insecure connections.
- `interceptors` (optional): Array of gRPC interceptors to use with the client.

#### Basic Constructor Examples

```csharp
// Basic initialization with a single endpoint
EtcdClient client = new EtcdClient("localhost:2379");

// Multiple endpoints
EtcdClient client = new EtcdClient("localhost:2379,localhost:2380,localhost:2381");

// Specifying HTTPS endpoints
EtcdClient client = new EtcdClient("https://localhost:2379,https://localhost:2380");

// Specifying HTTP endpoints
EtcdClient client = new EtcdClient("http://localhost:2379,http://localhost:2380");
```

### Advanced Constructor

```csharp
public EtcdClient(CallInvoker callInvoker)
```

#### Advanced Constructor Parameter

- `callInvoker` (required): A gRPC CallInvoker instance.

#### CallInvoker Example

```csharp
// Create a channel
var channel = GrpcChannel.ForAddress("localhost:2379");
var callInvoker = channel.CreateCallInvoker();

// Initialize with a custom CallInvoker
EtcdClient client = new EtcdClient(callInvoker);
```

## Connection String Formats

The `EtcdClient` supports several connection string formats:

### Static Host List

A comma-separated list of endpoints:

```csharp
// Format: host1:port1,host2:port2,...
EtcdClient client = new EtcdClient("localhost:2379,localhost:2380");
```

### DNS Discovery

Use DNS SRV records for discovery:

```csharp
// Format: dns://domain
EtcdClient client = new EtcdClient("dns://example.com");
```

### Alternative DNS Discovery

An alternative format for DNS SRV discovery:

```csharp
// Format: discovery-srv://domain
EtcdClient client = new EtcdClient("discovery-srv://example.com");
```

## Secure vs. Insecure Connections

### Secure Connection (HTTPS)

By default, the client uses secure connections (HTTPS):

```csharp
// Explicitly using HTTPS
EtcdClient client = new EtcdClient("https://localhost:2379");
```

### Insecure Connection (HTTP)

To use insecure connections (HTTP), you need to configure the channel options:

```csharp
// Using HTTP with insecure channel
EtcdClient client = new EtcdClient(
    "http://localhost:2379",
    configureChannelOptions: options => {
        options.Credentials = ChannelCredentials.Insecure;
    }
);
```

## Advanced Configuration

### Custom HTTP Handler

You can configure a custom HTTP handler for the gRPC client:

```csharp
EtcdClient client = new EtcdClient(
    "https://localhost:2379",
    configureChannelOptions: options => {
        var handler = new SocketsHttpHandler {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            EnableMultipleHttp2Connections = true
        };
        options.HttpHandler = handler;
    }
);
```

### Client Certificates

To use client certificates for authentication:

```csharp
EtcdClient client = new EtcdClient(
    "https://localhost:2379",
    configureChannelOptions: options => {
        var handler = new SocketsHttpHandler();

        // Load client certificate
        var clientCert = new X509Certificate2("client.pfx", "password");

        handler.SslOptions.ClientCertificates = new X509CertificateCollection { clientCert };
        options.HttpHandler = handler;
    }
);
```

### Custom Interceptors

You can add custom gRPC interceptors:

```csharp
// Create custom interceptors
var loggingInterceptor = new LoggingInterceptor();
var metricInterceptor = new MetricInterceptor();

// Initialize with interceptors
EtcdClient client = new EtcdClient(
    "https://localhost:2379",
    interceptors: new Interceptor[] { loggingInterceptor, metricInterceptor }
);
```

### Custom Retry Policy

You can customize the retry policy:

```csharp
EtcdClient client = new EtcdClient(
    "https://localhost:2379",
    configureChannelOptions: options => {
        options.ServiceConfig = new ServiceConfig {
            MethodConfigs = {
                new MethodConfig {
                    Names = { MethodName.Default },
                    RetryPolicy = new RetryPolicy {
                        MaxAttempts = 3,
                        InitialBackoff = TimeSpan.FromSeconds(2),
                        MaxBackoff = TimeSpan.FromSeconds(10),
                        BackoffMultiplier = 2,
                        RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.DeadlineExceeded }
                    }
                }
            }
        };
    }
);
```

## See Also

- [API Reference](client-methods.md) - Complete API reference for the EtcdClient class
- [Authentication](../authentication/index.md) - How to authenticate with etcd
- [Disposing the Client](../index.md#disposing-the-client) - How to properly dispose the client
