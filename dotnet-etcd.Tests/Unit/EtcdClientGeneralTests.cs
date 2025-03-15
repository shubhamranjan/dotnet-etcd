using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientGeneralTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IWatchManager> _mockWatchManager;

    public EtcdClientGeneralTests()
    {
        _mockConnection = MockConnection.Create();
        _mockWatchManager = MockAsyncCalls.CreateWatchManager();
    }

    [Fact]
    public void Constructor_WithNullCallInvoker_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient(null));

        // Assert
        Assert.Equal("callInvoker", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidCallInvoker_ShouldCreateClient()
    {
        // Arrange
        var mockCallInvoker = new Mock<CallInvoker>();

        // Act
        using var client = new EtcdClient(mockCallInvoker.Object);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithNullConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient((string)null));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient(""));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespaceConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient("  "));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullConnection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient((IConnection)null));
        Assert.Equal("connection", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithConnection_ShouldInitializeClient()
    {
        // Act
        using var client = new EtcdClient(_mockConnection.Object);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(_mockConnection.Object, client.GetConnection());
    }

    [Fact]
    public void Constructor_WithConnectionAndWatchManager_ShouldInitializeClient()
    {
        // Act
        using var client = new EtcdClient(_mockConnection.Object, _mockWatchManager.Object);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(_mockConnection.Object, client.GetConnection());
        Assert.Equal(_mockWatchManager.Object, client.GetWatchManager());
    }

    [Fact]
    public void GetConnection_ShouldReturnConnection()
    {
        // Arrange
        var client = new EtcdClient(_mockConnection.Object);

        // Act
        var result = client.GetConnection();

        // Assert
        Assert.Same(_mockConnection.Object, result);
    }

    [Fact]
    public void Dispose_ShouldDisposeWatchManager()
    {
        // Arrange
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(_mockConnection.Object, mockWatchManager.Object);

        // Act
        client.Dispose();

        // Assert
        mockWatchManager.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldOnlyDisposeOnce()
    {
        // Arrange
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(_mockConnection.Object, mockWatchManager.Object);

        // Act
        client.Dispose();
        client.Dispose(); // Call dispose a second time

        // Assert
        mockWatchManager.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void CallEtcd_ShouldCallFunctionWithConnection()
    {
        // Arrange
        var mockKvClient = _mockConnection.SetupKVClient();
        var testResponse = new RangeResponse();

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(testResponse);

        var client = new EtcdClient(_mockConnection.Object);

        // Act
        var result = client.Get("test-key");

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CallEtcdAsync_ShouldCallFunctionWithConnection()
    {
        // Arrange
        var mockKvClient = _mockConnection.SetupKVClient();
        var testResponse = new RangeResponse();

        var asyncResponse = AsyncUnaryCallFactory.Create(testResponse);

        mockKvClient
            .Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        var client = new EtcdClient(_mockConnection.Object);

        // Act
        var result = await client.GetAsync("test-key");

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}