using dotnet_etcd.DependencyInjection;
using dotnet_etcd.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace dotnet_etcd.Tests.Unit.DependencyInjection;

[Trait("Category", "Unit")]
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddEtcdClient_WithConnectionString_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEtcdClient("localhost:2379");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
    }

    [Fact]
    public void AddEtcdClient_WithOptionsAction_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configActionCalled = false;

        // Act
        services.AddEtcdClient(options =>
        {
            options.ConnectionString = "localhost:2379";
            options.UseInsecureChannel = true;
            configActionCalled = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
        Assert.True(configActionCalled);
    }

    [Fact]
    public void AddEtcdClient_WithOptions_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new EtcdClientOptions
        {
            ConnectionString = "localhost:2379",
            UseInsecureChannel = true
        };

        // Act
        services.AddEtcdClient(options);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
    }

    [Fact]
    public void AddEtcdClient_WithFactories_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockConnection = new Mock<IConnection>();
        var mockWatchManager = new Mock<IWatchManager>();

        // Act
        services.AddEtcdClient(
            _ => mockConnection.Object,
            _ => mockWatchManager.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
    }

    [Fact]
    public void AddEtcdClient_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddEtcdClient(null, "localhost:2379"));
    }

    [Fact]
    public void AddEtcdClient_WithNullConnectionString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<EtcdClientOptions> nullAction = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEtcdClient(nullAction));
    }
}