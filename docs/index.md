# dotnet-etcd Documentation

![etcd logo](img/etcd-logo-rectangle.png)

A C# .NET (dotnet) GRPC client for etcd v3+

## Documentation Sections

### Getting Started

- [Client Initialization](client-initialization/index.md) - How to initialize and configure the etcd client
- [Authentication](authentication/index.md) - How to authenticate with etcd

### Core Operations

- [Key-Value Operations](key-value/index.md) - Working with keys and values
- [Watch Operations](watch/index.md) - Watching for changes to keys
- [Lease Operations](lease/index.md) - Working with leases
- [Lock Operations](lock/index.md) - Distributed locking
- [Election Operations](election/index.md) - Leader election
- [Cluster Operations](cluster/index.md) - Managing the etcd cluster
- [Maintenance Operations](maintenance/index.md) - Maintenance tasks
- [Transactions](transactions/index.md) - Atomic operations

## Common Functionality

### Disposing the Client

The `EtcdClient` implements `IDisposable` and should be properly disposed when no longer needed to release resources:

```csharp
// Using statement automatically disposes the client when done
using (var client = new EtcdClient("https://localhost:2379"))
{
  // Perform operations with the client
  var response = client.Get("my-key");
  // ...
}

// Alternative manual disposal
var etcdClient = new EtcdClient("https://localhost:2379");
try
{
  // Perform operations with the client
  var response = etcdClient.Get("my-key");
  // ...
}
finally
{
  // Ensure the client is disposed
  etcdClient.Dispose();
}
```

## API Reference

For a complete reference of all available methods and their overloads, see the following sections:

- [EtcdClient](client-initialization/client-methods.md) - Main client methods
- [Key-Value API](key-value/api-reference.md) - Key-value operations
- [Watch API](watch/api-reference.md) - Watch operations
- [Lease API](lease/api-reference.md) - Lease operations
- [Lock API](lock/api-reference.md) - Lock operations
- [Election API](election/api-reference.md) - Election operations
- [Cluster API](cluster/api-reference.md) - Cluster operations
- [Maintenance API](maintenance/api-reference.md) - Maintenance operations
- [Authentication API](authentication/api-reference.md) - Authentication operations
- [Transaction API](transactions/api-reference.md) - Transaction operations

## Contributing

We welcome contributions to help improve dotnet-etcd! Please see the [CONTRIBUTING.md](https://github.com/shubhamranjan/dotnet-etcd/blob/master/CONTRIBUTING.md) file for guidelines on how to contribute.

For bug reports, feature requests, or questions, please create an [issue](https://github.com/shubhamranjan/dotnet-etcd/issues) on GitHub.
