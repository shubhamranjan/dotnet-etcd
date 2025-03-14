# dotnet-etcd

![etcd logo](https://raw.githubusercontent.com/shubhamranjan/dotnet-etcd/master/docs/img/etcd-logo-rectangle.png)

A C# .NET (dotnet) GRPC client for etcd v3+

![Build Status](https://github.com/shubhamranjan/dotnet-etcd/actions/workflows/codeql-analysis.yml/badge.svg)
![Nuget Version Info](https://img.shields.io/nuget/v/dotnet-etcd.svg)
![Nuget Download Info](https://img.shields.io/nuget/dt/dotnet-etcd.svg)

## Table of Contents

- [Latest Version](#latest-version-720)
- [Supported .NET Versions](#supported-net-versions)
- [Installing Package](#installing-package)
- [Documentation](#documentation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Contributing](#contributing)

## Latest Version: 7.2.0

- Support for .NET 8.0
- Improved watch reconnects
- Enhanced retry policy for better resilience
- Package updates and dependencies refresh

## Supported .NET Versions

- .NET 8
- .NET 7
- .NET 6

### Compatibility Note

For older dotnet versions, use lib version < 5.x

## Installing Package

Nuget package is published on [nuget.org](https://www.nuget.org/packages/dotnet-etcd/) and can be installed in the following ways:

### Nuget Package Manager

```powershell
Install-Package dotnet-etcd
```

### .NET CLI

```bash
dotnet add package dotnet-etcd
```

### Paket CLI

```bash
paket add dotnet-etcd
```

> The NuGet Team does not provide support for this client. Please contact its maintainers for support.

## Documentation

For comprehensive documentation of all operations and method overloads, please see the [documentation pages](docs/index.md).

The documentation is organized into the following sections:

- [Client Initialization](docs/client-initialization/index.md) - How to initialize and configure the client
- [Key-Value Operations](docs/key-value/index.md) - Working with keys and values
- [Watch Operations](docs/watch/index.md) - Watching for changes to keys
- [Lease Operations](docs/lease/index.md) - Working with leases
- [Lock Operations](docs/lock/index.md) - Distributed locking
- [Election Operations](docs/election/index.md) - Leader election
- [Cluster Operations](docs/cluster/index.md) - Managing the etcd cluster
- [Maintenance Operations](docs/maintenance/index.md) - Maintenance tasks
- [Transactions](docs/transactions/index.md) - Atomic operations
- [Authentication](docs/authentication/index.md) - Authentication with etcd

The documentation includes detailed API references with all method overloads, parameters, and return types, as well as examples for common use cases.

## Quick Start

Add using statement at the top of your class file:

```csharp
using dotnet_etcd;
```

### Client Initialization

```csharp
// Basic initialization with a single endpoint
EtcdClient client = new EtcdClient("localhost:2379");

// Multiple endpoints
EtcdClient client = new EtcdClient("https://localhost:23790,https://localhost:23791,https://localhost:23792");

// Insecure connection (HTTP)
EtcdClient client = new EtcdClient("http://localhost:23790", configureChannelOptions: (options =>
{
    options.Credentials = ChannelCredentials.Insecure;
}));
```

For more advanced initialization options, see the [Client Initialization documentation](docs/client-initialization/index.md).

### Authentication

```csharp
// Authenticate with username and password
EtcdClient client = new EtcdClient("https://localhost:23790");
var authRes = client.Authenticate(new Etcdserverpb.AuthenticateRequest()
{
    Name = "username",
    Password = "password",
});

// Use the token for authenticated operations
client.Put("foo/bar", "barfoo", new Grpc.Core.Metadata() {
    new Grpc.Core.Metadata.Entry("token", authRes.Token)
});
```

For more authentication options, see the [Authentication documentation](docs/authentication/index.md).

## Features

### Automatic Retry Functionality

dotnet-etcd includes an automatic retry mechanism for handling transient failures when communicating with etcd clusters. By default, the client is configured with a retry policy that:

- Retries up to 5 times when encountering unavailable servers
- Uses exponential backoff between retry attempts
- Handles common transient errors automatically

This functionality is enabled by default and requires no additional configuration.

### Canceling Operations

Operations can be canceled using a CancellationToken. By default, the client throws OperationCanceledException when a request is canceled.

```csharp
CancellationTokenSource cts = new CancellationTokenSource();
try {
    cts.Cancel();
    var response = client.Status(new StatusRequest(), cancellationToken: cts.Token);
} catch (OperationCanceledException) {
    Console.WriteLine("Operation was canceled.");
}
```

For legacy cancellation behavior with RpcException, see the [documentation](docs/index.md).

### Error Handling

Most errors from the etcd client are thrown as `RpcException` with specific status codes that can be handled appropriately:

```csharp
try {
    var response = client.Get("non-existent-key");
} catch (RpcException ex) {
    switch (ex.StatusCode) {
        case StatusCode.NotFound:
            Console.WriteLine("Key not found");
            break;
        case StatusCode.Unavailable:
            Console.WriteLine("Server unavailable");
            break;
        // Handle other status codes
    }
}
```

### Disposing the Client

The `EtcdClient` implements `IDisposable` and should be properly disposed when no longer needed:

```csharp
using (var client = new EtcdClient("https://localhost:2379")) {
    var response = client.Get("my-key");
    // ...
}
```

For more details on proper client disposal, see the [documentation](docs/index.md#disposing-the-client).

## Contributing

We welcome contributions to help improve dotnet-etcd! Please see the [CONTRIBUTING.md](https://github.com/shubhamranjan/dotnet-etcd/blob/master/CONTRIBUTING.md) file for guidelines on how to contribute.

For bug reports, feature requests, or questions, please create an [issue](https://github.com/shubhamranjan/dotnet-etcd/issues) on GitHub.
