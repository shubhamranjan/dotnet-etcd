# dotnet-etcd Tests

This directory contains tests for the dotnet-etcd library.

## Test Structure

The tests are organized into two main categories:

1. **Unit Tests** - Located in the `Unit` directory, these tests don't require a running etcd server and use mocks to
   test the client functionality.
2. **Integration Tests** - Located in the `Integration` directory, these tests require a running etcd server to test
   actual interactions with etcd.

### Unit Tests

Unit tests use mocking to test the client functionality without requiring a running etcd server. They test:

- Client method calls
- Parameter validation
- Response handling
- Error handling

Key unit test files:

- `KvClientTests.cs` - Tests for key-value operations
- `AuthClientTests.cs` - Tests for authentication operations
- `LeaseClientTests.cs` - Tests for lease operations
- `LockClientTests.cs` - Tests for lock operations
- `ClusterClientTests.cs` - Tests for cluster operations
- `WatchStreamTests.cs` - Tests for watch stream functionality
- `WatchManagerTests.cs` - Tests for watch manager functionality

### Integration Tests

Integration tests require a running etcd server and test actual interactions with etcd. They test:

- Connection to etcd
- Key-value operations
- Watch functionality
- Lease management
- Authentication
- Cluster management
- Lock operations
- Maintenance operations

## Running Tests

### Prerequisites

- .NET SDK 6.0 or higher
- For integration tests: a running etcd server (default: localhost:2379)

### Running Unit Tests

To run only the unit tests:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "FullyQualifiedName~Unit"
```

To run a specific unit test class:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "FullyQualifiedName~KvClientTests"
```

### Running Integration Tests

To run integration tests, you need a running etcd server. The integration tests will connect to etcd at `localhost:2379`
by default.

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "FullyQualifiedName~Integration"
```

### Running All Tests

To run all tests:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj
```

## Code Coverage

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

## Test Infrastructure

The tests use the following infrastructure:

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **Coverlet** - Code coverage tool
- **ReportGenerator** - Coverage report generator
- **TestHelper** - A helper class for setting up test clients and mocks

### TestHelper

The `TestHelper` class provides methods for creating test clients and setting up mocks:

- `CreateEtcdClientWithMockCallInvoker()` - Creates an EtcdClient with a mocked CallInvoker
- `SetupMockClientViaConnection()` - Sets up a mock client via the Connection class using reflection

## Contributing Tests

When adding new tests:

1. For unit tests, use the `TestHelper` class to create clients and mocks
2. For integration tests, ensure they can be run in isolation
3. Add appropriate assertions to verify behavior
4. Run the tests to ensure they pass
5. Check code coverage to ensure the tests cover the intended functionality

## Test Types

- **Unit Tests**: Tests that don't require an actual etcd server. These use mocks to simulate etcd behavior.
- **Integration Tests**: Tests that require a running etcd server. These tests interact with a real etcd instance.

## Running the Tests

### Prerequisites

- Docker and Docker Compose
- .NET SDK

### Starting the etcd Cluster

You can start either a single-node or a 3-node etcd cluster for testing:

#### Single-node Cluster

```bash
./start-etcd.sh
# or explicitly specify single-node
./start-etcd.sh single
```

#### 3-node Cluster

```bash
./start-etcd.sh 3nodes
```

### Running the Tests

Once the etcd cluster is running, you can run the tests:

```bash
dotnet test
```

To run only the integration tests:

```bash
dotnet test --filter Category=Integration
```

To run only the unit tests:

```bash
dotnet test --filter Category=Unit
```

### Running Integration Tests with Cluster Setup and Teardown

For convenience, you can use the `run-integration-tests.sh` script to start the cluster, run the integration tests, and
stop the cluster:

```bash
# Run with single-node cluster (default)
./run-integration-tests.sh

# Run with 3-node cluster
./run-integration-tests.sh 3nodes
```

### Stopping the etcd Cluster

To stop the etcd cluster:

```bash
./stop-etcd.sh
```

## Test Files

### Unit Tests

- `EtcdClientWatchTests.cs`: Tests for the watch functionality in EtcdClient
- `WatchManagerTests.cs`: Tests for the WatchManager class
- `WatchStreamTests.cs`: Tests for the WatchStream class

### Integration Tests

- `WatchIntegrationTests.cs`: Integration tests for watching individual keys
- `WatchRangeIntegrationTests.cs`: Tests for watching ranges of keys

## Cluster Configuration

- `docker-compose-single.yml`: Configuration for a single-node etcd cluster
- `docker-compose-3nodes.yml`: Configuration for a 3-node etcd cluster
- `start-etcd.sh`: Script to start the etcd cluster
- `stop-etcd.sh`: Script to stop the etcd cluster 