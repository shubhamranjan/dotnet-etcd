# Testing dotnet-etcd

This document provides comprehensive guidance for running tests and generating code coverage reports for the dotnet-etcd project.

## Test Categories

The test suite is divided into two main categories:

1. **Unit Tests**: Don't require a running etcd server
2. **Integration Tests**: Require a running etcd server

## Running Tests

### Prerequisites

- .NET SDK (6.0 or later)
- Docker (for running integration tests)
- ReportGenerator tool (for coverage reports)

Install the required tools:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Unit Tests

Run unit tests only:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Unit"
```

### Integration Tests

1. Start an etcd server:

```bash
# Single node
docker run -d --name etcd-server \
  -p 2379:2379 \
  quay.io/coreos/etcd:v3.5.0 \
  etcd --advertise-client-urls http://0.0.0.0:2379 \
  --listen-client-urls http://0.0.0.0:2379

# Or use the provided script
./dotnet-etcd.Tests/start-etcd.sh
```

2. Run integration tests:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Integration"
```

3. Stop the server:

```bash
docker stop etcd-server
docker rm etcd-server

# Or use the provided script
./dotnet-etcd.Tests/stop-etcd.sh
```

### All Tests

Run both unit and integration tests:

```bash
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj
```

## Code Coverage

### Generating Coverage Reports

1. Run tests with coverage:

```bash
# Use the provided script
./run-unit-tests-with-coverage.sh

# Or run manually
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./TestResults/Coverage/
```

2. Generate HTML report:

```bash
reportgenerator \
  -reports:"dotnet-etcd.Tests/TestResults/Coverage/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html
```

3. View the report at `coveragereport/index.html`

### Continuous Integration

- GitHub Actions automatically runs tests and generates coverage reports
- Coverage reports are published to GitHub Pages
- Coverage trends are tracked over time

## Test Structure

### Unit Tests

- Located in `dotnet-etcd.Tests/Unit/`
- Test individual components in isolation
- Use mocking for external dependencies
- Fast execution, no external dependencies

### Integration Tests

- Located in `dotnet-etcd.Tests/Integration/`
- Test full functionality with real etcd server
- Cover real-world scenarios
- Require running etcd server

## Writing Tests

### Best Practices

1. **Categories**: Mark tests with appropriate category:
```csharp
[Trait("Category", "Unit")]
public class MyUnitTest { }

[Trait("Category", "Integration")]
public class MyIntegrationTest { }
```

2. **Naming**: Follow the convention:
```csharp
public class ClassNameTests
{
    [Fact]
    public void MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

3. **Mocking**: Use Moq for unit tests:
```csharp
var mockClient = new Mock<IEtcdClient>();
mockClient.Setup(x => x.Get(It.IsAny<string>()))
         .Returns(expectedResponse);
```

4. **Clean Up**: Use IDisposable for resource cleanup:
```csharp
public class IntegrationTest : IDisposable
{
    private readonly EtcdClient _client;

    public IntegrationTest()
    {
        _client = new EtcdClient("localhost:2379");
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
```

### Test Data

- Use meaningful test data
- Clean up test data in integration tests
- Use constants for common values
- Document test data requirements

## Troubleshooting

### Common Issues

1. **Integration Tests Failing**
   - Verify etcd server is running
   - Check connection string
   - Ensure no port conflicts

2. **Coverage Reports**
   - Verify coverage files exist
   - Check ReportGenerator installation
   - Ensure all test categories are run

3. **Test Timeouts**
   - Increase timeout in test settings
   - Check etcd server health
   - Verify network connectivity

### Getting Help

- Check the [GitHub Issues](https://github.com/shubhamranjan/dotnet-etcd/issues)
- Create a new issue with test output
- Include environment details
- Provide minimal reproduction