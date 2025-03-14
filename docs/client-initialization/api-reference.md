# EtcdClient API Reference

This page provides a complete reference of the `EtcdClient` class, including constructors, properties, and methods.

## Constructors

### EtcdClient Constructor Overloads

```csharp
// Initialize with connection string
public EtcdClient(
    string connectionString, 
    int port = 2379, 
    string serverName = "my-etcd-server",
    Action<GrpcChannelOptions> configureChannelOptions = null, 
    Interceptor[] interceptors = null)

// Initialize with CallInvoker
public EtcdClient(CallInvoker callInvoker)
```

#### Constructor Parameters

- `connectionString`: String containing comma-separated list of etcd endpoints.
- `port`: Port to use when no port is specified in the endpoints. Default: 2379
- `serverName`: Name used for the static resolver. Default: "my-etcd-server"
- `configureChannelOptions`: Action to configure gRPC channel options. Can be used to set credentials for secure/insecure connections.
- `interceptors`: Array of gRPC interceptors to use with the client.
- `callInvoker`: A gRPC CallInvoker instance for advanced scenarios.

## Properties

### Channel

```csharp
public GrpcChannel Channel { get; }
```

Gets the underlying gRPC channel used by the client.

### CallInvoker

```csharp
public CallInvoker CallInvoker { get; }
```

Gets the CallInvoker used by the client.

### Endpoints

```csharp
public string[] Endpoints { get; }
```

Gets the array of endpoints the client is configured to use.

## Authentication Methods

### Authenticate

Authenticates with etcd using the provided username and password.

```csharp
public AuthenticateResponse Authenticate(
    string name, 
    string password, 
    Grpc.Core.Metadata headers = null, 
    DateTime? deadline = null, 
    CancellationToken cancellationToken = default)

public async Task<AuthenticateResponse> AuthenticateAsync(
    string name, 
    string password, 
    Grpc.Core.Metadata headers = null, 
    DateTime? deadline = null, 
    CancellationToken cancellationToken = default)
```

#### Authenticate Parameters

- `name`: The username to authenticate with.
- `password`: The password to authenticate with.
- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Authenticate Returns

- `AuthenticateResponse`: The response containing the authentication token.

### SetCredentials

Sets the credentials to use for subsequent requests.

```csharp
public void SetCredentials(string username, string password)
```

#### SetCredentials Parameters

- `username`: The username to use for authentication.
- `password`: The password to use for authentication.

### SetAuthToken

Sets the authentication token to use for subsequent requests.

```csharp
public void SetAuthToken(string authToken)
```

#### SetAuthToken Parameters

- `authToken`: The authentication token to use.

## Connection Management

### Dispose

Disposes the client and releases all resources.

```csharp
public void Dispose()
```

### ResetConnection

Resets the connection to etcd.

```csharp
public void ResetConnection()
```

## Health Check Methods

### Health

Checks the health of the etcd cluster.

```csharp
public HealthCheckResponse Health(
    Grpc.Core.Metadata headers = null, 
    DateTime? deadline = null, 
    CancellationToken cancellationToken = default)

public async Task<HealthCheckResponse> HealthAsync(
    Grpc.Core.Metadata headers = null, 
    DateTime? deadline = null, 
    CancellationToken cancellationToken = default)
```

#### Health Parameters

- `headers`: The initial metadata to send with the call. This parameter is optional.
- `deadline`: An optional deadline for the call. The call will be cancelled if deadline is hit.
- `cancellationToken`: An optional token for canceling the call.

#### Health Returns

- `HealthCheckResponse`: The response containing the health status.

## Utility Methods

### GetVersion

Gets the version of the etcd server.

```csharp
public string GetVersion()
```

#### GetVersion Returns

- `string`: The version of the etcd server.

### GetClientVersion

Gets the version of the client.

```csharp
public string GetClientVersion()
```

#### GetClientVersion Returns

- `string`: The version of the client.

## Response Types

### AuthenticateResponse

The `AuthenticateResponse` class represents the response from an authenticate operation.

#### AuthenticateResponse Properties

- `Header`: The response header.
- `Token`: The authentication token.

### HealthCheckResponse

The `HealthCheckResponse` class represents the response from a health check operation.

#### HealthCheckResponse Properties

- `Status`: The health status of the etcd cluster.

## Examples

### Basic Authentication

```csharp
// Initialize the client
var client = new EtcdClient("localhost:2379");

// Authenticate with username and password
var authResponse = client.Authenticate("root", "rootpw");

// Set the auth token for subsequent requests
client.SetAuthToken(authResponse.Token);

// Alternatively, set credentials directly
client.SetCredentials("root", "rootpw");
```

### Health Check

```csharp
// Initialize the client
var client = new EtcdClient("localhost:2379");

// Check the health of the etcd cluster
var healthResponse = client.Health();

// Display health status
Console.WriteLine($"Health Status: {healthResponse.Status}");
```

### Proper Disposal

```csharp
// Using statement automatically disposes the client when done
using (var client = new EtcdClient("localhost:2379"))
{
    // Perform operations with the client
    var healthResponse = client.Health();
    // ...
}

// Alternative manual disposal
var etcdClient = new EtcdClient("localhost:2379");
try
{
    // Perform operations with the client
    var healthResponse = etcdClient.Health();
    // ...
}
finally
{
    // Ensure the client is disposed
    etcdClient.Dispose();
}
```

## See Also

- [Client Initialization](index.md) - How to initialize and configure the etcd client
- [Authentication](../authentication/index.md) - How to authenticate with etcd
- [Key-Value Operations](../key-value/index.md) - Working with keys and values
