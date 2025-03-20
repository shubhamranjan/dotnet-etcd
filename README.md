# dotnet-etcd

![etcd logo](https://raw.githubusercontent.com/shubhamranjan/dotnet-etcd/master/docs/img/etcd-logo-rectangle.png)

A C# .NET (dotnet) GRPC client for etcd v3+

![Build Status](https://github.com/shubhamranjan/dotnet-etcd/actions/workflows/codeql-analysis.yml/badge.svg)
![Nuget Version Info](https://img.shields.io/nuget/v/dotnet-etcd.svg)
![Nuget Download Info](https://img.shields.io/nuget/dt/dotnet-etcd.svg)

## Table of Contents

- [Supported .NET Versions](#supported-net-versions)
- [Installing Package](#installing-package)
- [Documentation](#documentation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Contributing](#contributing)
- [Testing and Code Coverage](#testing-and-code-coverage)


## Supported .NET Versions

- .NET 9
- .NET 8

### Compatibility Note

For older .NET versions:
- For .NET 6/7 support, use version 7.2.0
- For .NET versions < 6, use version < 5.x

## Installing Package

Nuget package is published on [nuget.org](https://www.nuget.org/packages/dotnet-etcd/) and can be installed in the
following ways:

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

### Getting Started
- [Client Initialization](docs/client-initialization/index.md) - How to initialize and configure the client
- [Dependency Injection](docs/client-initialization/dependency-injection.md) - Using the client with DI
- [Authentication](docs/authentication/index.md) - Authentication with etcd

### Core Operations
- [Key-Value Operations](docs/key-value/index.md) - Working with keys and values
- [Watch Operations](docs/watch/index.md) - Watching for changes to keys
- [Lease Operations](docs/lease/index.md) - Working with leases
- [Lock Operations](docs/lock/index.md) - Distributed locking
- [Election Operations](docs/election/index.md) - Leader election

### Advanced Operations
- [Cluster Operations](docs/cluster/index.md) - Managing the etcd cluster
- [Maintenance Operations](docs/maintenance/index.md) - Maintenance tasks
- [Transactions](docs/transactions/index.md) - Atomic operations

The documentation includes detailed API references with all method overloads, parameters, and return types, as well as
examples for common use cases.

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

For more advanced initialization options, see
the [Client Initialization documentation](docs/client-initialization/index.md).

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

### Dependency Injection Support
Built-in support for Microsoft.Extensions.DependencyInjection with extension methods for easy configuration:

```csharp
services.AddEtcdClient(options => {
    options.ConnectionString = "localhost:2379";
    options.UseInsecureChannel = true;
});
```

### Automatic Retry Functionality

dotnet-etcd includes an automatic retry mechanism for handling transient failures when communicating with etcd clusters.
By default, the client is configured with a retry policy that:

- Retries up to 5 times when encountering unavailable servers
- Uses exponential backoff between retry attempts
- Handles common transient errors automatically

This functionality is enabled by default and requires no additional configuration.

### Canceling Operations

Operations can be canceled using a CancellationToken. By default, the client throws OperationCanceledException when a
request is canceled.

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

Most errors from the etcd client are thrown as `RpcException` with specific status codes that can be handled
appropriately:

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

We welcome contributions to help improve dotnet-etcd! Please see
the [CONTRIBUTING.md](https://github.com/shubhamranjan/dotnet-etcd/blob/master/CONTRIBUTING.md) file for guidelines on
how to contribute.

For bug reports, feature requests, or questions, please create
an [issue](https://github.com/shubhamranjan/dotnet-etcd/issues) on GitHub.

### Running Tests

The project includes both unit tests and integration tests. Unit tests can be run without any external dependencies,
while integration tests require a running etcd cluster.

#### Setting Up etcd for Testing

The `dotnet-etcd.Tests` directory includes scripts to easily set up either a single-node or a 3-node etcd cluster using
Docker:

```bash
cd dotnet-etcd.Tests

# Start a single-node cluster
./start-etcd.sh

# Or start a 3-node cluster
./start-etcd.sh 3nodes

# Run the tests
dotnet test

# Stop the cluster when done
./stop-etcd.sh
```

For convenience, you can also use the script that handles the entire process:

```bash
cd dotnet-etcd.Tests

# Run integration tests with a single-node cluster
./run-integration-tests.sh

# Or with a 3-node cluster
./run-integration-tests.sh 3nodes
```

See the [test README](dotnet-etcd.Tests/README.md) for more details on running tests.

## Development

### Running Tests

#### Unit Tests

To run only the unit tests (which don't require a running etcd server):

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "FullyQualifiedName~Unit"
```

#### Integration Tests

To run integration tests, you need a running etcd server. The integration tests will connect to etcd at `localhost:2379`
by default.

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "FullyQualifiedName~Integration"
```

#### All Tests

To run all tests:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj
```

### Code Coverage

To run tests with code coverage and generate a report:

1. Make sure you have the required tools:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

2. Run the coverage script:

```bash
./run-unit-tests-with-coverage.sh
```

3. View the coverage report at `./dotnet-etcd.Tests/TestResults/CoverageReport/index.html`

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Testing and Code Coverage

We have comprehensive test coverage for dotnet-etcd, including both unit tests and integration tests.

### Running Tests

For detailed information about running tests and generating code coverage reports, see the [TESTING.md](TESTING.md)
file.

#### Quick Test Commands

```bash
# Run unit tests only
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Unit"

# Run integration tests (requires running etcd server)
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Integration"

# Run all tests
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj
```

### Code Coverage

We use GitHub Actions to automatically generate code coverage reports for the main branch. You can view the latest code
coverage report on the [GitHub Pages site](https://shubhamranjan.github.io/dotnet-etcd/).

To generate a code coverage report locally:

```bash
# Install the required tools
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open the report
open coveragereport/index.html  # On macOS
# or
start coveragereport/index.html  # On Windows
```

For more details, see the [TESTING.md](TESTING.md) file.
