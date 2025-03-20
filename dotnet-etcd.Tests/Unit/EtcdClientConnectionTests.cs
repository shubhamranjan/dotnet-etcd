using System;
using System.Net.Http;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientConnectionTests : IDisposable
{
    private readonly EtcdClient _client;

    public EtcdClientConnectionTests()
    {
        // Create a client with a simple connection string for basic tests
        _client = new EtcdClient("localhost");
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    [Fact]
    public void Constructor_WithSingleHost_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("localhost");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithMultipleHosts_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("localhost:2379,localhost:2380");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithHttpPrefix_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("http://localhost:2379");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact(Skip = "This test requires a real HTTPS connection which isn't available in the test environment")]
    public void Constructor_WithHttpsPrefix_ShouldCreateClient()
    {
        // Arrange
        Action<GrpcChannelOptions> configureOptions = options =>
        {
            options.HttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Credentials = new SslCredentials();
        };

        // We're skipping this test because it requires a real HTTPS connection
        // which isn't available in the test environment
        
        // In a real test, we would:
        // using var client = new EtcdClient("https://localhost:2379", configureChannelOptions: configureOptions);
        // Assert.NotNull(client);
        // Assert.NotNull(client.GetConnection());
        // Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithDnsPrefix_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("dns://localhost");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithDiscoverySrvPrefix_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("discovery-srv://localhost");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithCustomPort_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("localhost", 2380);

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithCustomServerName_ShouldCreateClient()
    {
        // Act
        using var client = new EtcdClient("localhost", serverName: "custom-server");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.GetConnection());
        Assert.NotNull(client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithConfigureChannelOptions_ShouldCreateClient()
    {
        // Arrange
        bool optionsConfigured = false;
        Action<GrpcChannelOptions> configureOptions = options =>
        {
            optionsConfigured = true;
        };

        // Act
        using var client = new EtcdClient("localhost", configureChannelOptions: configureOptions);

        // Assert
        Assert.NotNull(client);
        Assert.True(optionsConfigured);
    }

    [Fact]
    public void Constructor_WithInterceptors_ShouldCreateClient()
    {
        // Arrange
        var interceptor = new TestInterceptor();

        // Act
        using var client = new EtcdClient("localhost", interceptors: new[] { interceptor });

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void CancelWatch_WithSingleId_ShouldCallWatchManager()
    {
        // Arrange
        var mockConnection = dotnet_etcd.Tests.Unit.Mocks.MockConnection.Create();
        var mockWatchManager = new Moq.Mock<dotnet_etcd.interfaces.IWatchManager>();
        using var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

        // Act
        client.CancelWatch(123);

        // Assert
        mockWatchManager.Verify(m => m.CancelWatch(123), Moq.Times.Once);
    }

    [Fact]
    public void CancelWatch_WithMultipleIds_ShouldCallWatchManagerForEachId()
    {
        // Arrange
        var mockConnection = dotnet_etcd.Tests.Unit.Mocks.MockConnection.Create();
        var mockWatchManager = new Moq.Mock<dotnet_etcd.interfaces.IWatchManager>();
        using var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);
        long[] watchIds = { 123, 456, 789 };

        // Act
        client.CancelWatch(watchIds);

        // Assert
        mockWatchManager.Verify(m => m.CancelWatch(123), Moq.Times.Once);
        mockWatchManager.Verify(m => m.CancelWatch(456), Moq.Times.Once);
        mockWatchManager.Verify(m => m.CancelWatch(789), Moq.Times.Once);
    }

    private class TestInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return continuation(request, context);
        }
    }
}
