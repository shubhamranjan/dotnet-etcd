using dotnet_etcd.Tests.Unit.Mocks;
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
    public void AddEtcdClient_WithOptionsActionAndServiceProvider_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configActionCalled = false;
        var providedConnectionString = "";

        // Register some service that should be resolved within AddEtcdClient()
        services.AddKeyedSingleton<string>("connectionString", "localhost:2379");

        // Act
        services.AddEtcdClient((sp, options) =>
        {
            var connectionString = sp.GetRequiredKeyedService<string>("connectionString");

            providedConnectionString = connectionString;

            options.ConnectionString = connectionString;
            options.UseInsecureChannel = true;
            configActionCalled = true;
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
        Assert.True(configActionCalled);
        Assert.Equal("localhost:2379", providedConnectionString);
    }

    [Fact]
    public void AddEtcdClient_WithOptionsFuncAndServiceProvider_ShouldRegisterClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configActionCalled = false;
        var providedConnectionString = "";

        // Register some service that should be resolved within AddEtcdClient()
        services.AddKeyedSingleton<string>("connectionString", "localhost:2379");

        // Act
        services.AddEtcdClient(sp =>
        {
            var connectionString = sp.GetRequiredKeyedService<string>("connectionString");

            providedConnectionString = connectionString;

            var options = new EtcdClientOptions();
            options.ConnectionString = connectionString;
            options.UseInsecureChannel = true;
            configActionCalled = true;

            return options;
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IEtcdClient>();
        Assert.NotNull(client);
        Assert.IsType<EtcdClient>(client);
        Assert.True(configActionCalled);
        Assert.Equal("localhost:2379", providedConnectionString);
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
        Action<EtcdClientOptions>? nullAction = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddEtcdClient(nullAction));
    }

    // ----- IWatchManager registration (previously never resolved by any test) -----

    [Fact]
    public void AddEtcdClient_WithConnectionString_ShouldResolveIWatchManager()
    {
        var services = new ServiceCollection();
        services.AddEtcdClient("localhost:2379");
        using var provider = services.BuildServiceProvider();

        var watchManager = provider.GetService<IWatchManager>();

        Assert.NotNull(watchManager);
        Assert.Same(((EtcdClient)provider.GetRequiredService<IEtcdClient>()).GetWatchManager(), watchManager);
    }

    [Fact]
    public void AddEtcdClient_WithOptionsAction_ShouldResolveIWatchManager()
    {
        var services = new ServiceCollection();
        services.AddEtcdClient(options =>
        {
            options.ConnectionString = "localhost:2379";
            options.UseInsecureChannel = true;
        });
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IWatchManager>());
    }

    [Fact]
    public void AddEtcdClient_WithConnectionFactory_WithoutWatchManagerFactory_ShouldResolveClientsWatchManager()
    {
        var services = new ServiceCollection();
        services.AddEtcdClient(_ => MockConnection.Create().Object);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IEtcdClient>();
        var watchManager = provider.GetService<IWatchManager>();

        Assert.IsType<EtcdClient>(client);
        Assert.NotNull(watchManager);
        Assert.Same(((EtcdClient)client).GetWatchManager(), watchManager);
    }

    [Fact]
    public void AddEtcdClient_WithConnectionFactory_AndWatchManagerFactory_ShouldUseTheSuppliedWatchManager()
    {
        var services = new ServiceCollection();
        var suppliedWatchManager = new Mock<IWatchManager>().Object;
        services.AddEtcdClient(_ => MockConnection.Create().Object, _ => suppliedWatchManager);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IEtcdClient>();

        Assert.Same(suppliedWatchManager, provider.GetService<IWatchManager>());
        Assert.Same(suppliedWatchManager, ((EtcdClient)client).GetWatchManager());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddEtcdClient_WithBlankConnectionString_ShouldThrow(string? connectionString)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddEtcdClient(connectionString!));
    }
}
