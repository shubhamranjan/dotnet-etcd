# Testing Guide for dotnet-etcd

This document provides information about testing the dotnet-etcd library, including running tests and generating code coverage reports.

## Test Categories

The tests are organized into two main categories:

1. **Unit Tests**: These tests focus on testing individual components in isolation, using mocks for dependencies. They don't require a running etcd server.
2. **Integration Tests**: These tests interact with a real etcd server and verify the behavior of the library against an actual etcd instance.

## Running Tests

### Prerequisites

- .NET SDK 7.0 or later
- For integration tests: A running etcd server (v3.x) accessible at localhost:2379

### Running Unit Tests

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Unit"
```

### Running Integration Tests

Ensure you have an etcd server running, then:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Integration"
```

### Running All Tests

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj
```

## Code Coverage

### Generating Code Coverage Reports

We use the `coverlet` and `reportgenerator` tools to generate code coverage reports.

1. Install the required tools:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

2. Run tests with coverage:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --collect:"XPlat Code Coverage"
```

3. Generate a report:

```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

4. Open the generated report:

```bash
open coveragereport/index.html  # On macOS
# or
start coveragereport/index.html  # On Windows
```

### Excluding gRPC Generated Code

To get a more accurate representation of code coverage, we exclude gRPC generated code from the reports. These files are auto-generated and don't need to be tested.

To exclude gRPC generated code when generating a report:

```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html -classfilters:"-Authpb.*;-Etcdserverpb.*;-Mvccpb.*;-V3Electionpb.*;-V3Lockpb.*;-Versionpb.*;-Gogoproto.*"
```

This excludes all the auto-generated gRPC code in the following namespaces:
- Authpb
- Etcdserverpb
- Mvccpb
- V3Electionpb
- V3Lockpb
- Versionpb
- Gogoproto

### Using the Coverage Script

For convenience, you can use the provided script:

```bash
./run-unit-tests-with-coverage.sh
```

This script will:
1. Run the unit tests with coverage enabled
2. Generate an HTML report
3. Display the coverage summary

## Continuous Integration

GitHub Actions workflows are set up to run tests automatically:

1. **dotnet-tests.yml**: Runs on PRs and pushes to the main branch
2. **code-coverage.yml**: Generates and publishes code coverage reports

## Improving Test Coverage

When adding new features or fixing bugs, please ensure:

1. Add unit tests for new functionality
2. Add integration tests for end-to-end scenarios
3. Run the coverage report to identify areas that need more testing

## Test Structure

- **Unit Tests**: Located in `dotnet-etcd.Tests/Unit/`
- **Integration Tests**: Located in `dotnet-etcd.Tests/Integration/`
- **Test Infrastructure**: Located in `dotnet-etcd.Tests/Infrastructure/` 