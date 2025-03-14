# dotnet-etcd Tests

This directory contains tests for the dotnet-etcd library.

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

For convenience, you can use the `run-integration-tests.sh` script to start the cluster, run the integration tests, and stop the cluster:

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