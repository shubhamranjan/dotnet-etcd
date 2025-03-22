using System;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class GrpcChannelFactoryTests
{
    private readonly GrpcChannelFactory _factory;

    public GrpcChannelFactoryTests()
    {
        _factory = new GrpcChannelFactory();
    }

    [Fact]
    public void CreateChannel_WithDnsConnection_ShouldCreateDnsChannel()
    {
        // Arrange
        var connectionString = "dns://localhost";
        var port = 2379;
        var serverName = "test-server";
        var credentials = ChannelCredentials.Insecure;

        // Act
        var channel = _factory.CreateChannel(connectionString, port, serverName, credentials);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal("localhost", channel.Target);
    }

    [Fact]
    public void CreateChannel_WithStaticHosts_ShouldCreateStaticChannel()
    {
        // Arrange
        var connectionString = "localhost,127.0.0.1";
        var port = 2379;
        var serverName = "test-server";
        var credentials = ChannelCredentials.Insecure;

        // Act
        var channel = _factory.CreateChannel(connectionString, port, serverName, credentials);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal("test-server", channel.Target);
    }

    [Fact]
    public void CreateChannel_WithCustomOptions_ShouldApplyOptions()
    {
        // Arrange
        var connectionString = "localhost";
        var port = 2379;
        var serverName = "test-server";
        var credentials = ChannelCredentials.Insecure;
        
        // Act
        var channel = _factory.CreateChannel(connectionString, port, serverName, credentials, options =>
        {
            options.MaxReceiveMessageSize = 1024;
            options.MaxSendMessageSize = 2048;
        });

        // Assert
        Assert.NotNull(channel);
        // Note: We can only verify the channel was created, as GrpcChannel doesn't expose these properties
    }

    [Fact]
    public void CreateChannel_WithSecureCredentials_ShouldUseSecureChannel()
    {
        // Arrange
        var connectionString = "localhost";
        var port = 2379;
        var serverName = "test-server";
        var credentials = new SslCredentials();

        // Act
        var channel = _factory.CreateChannel(connectionString, port, serverName, credentials);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal("test-server", channel.Target);
        // Note: We can't directly verify HTTPS usage as it's internal to the channel
    }

    [Fact]
    public void CreateChannel_ShouldConfigureDefaultOptions()
    {
        // Arrange
        var connectionString = "localhost";
        var port = 2379;
        var serverName = "test-server";
        var credentials = ChannelCredentials.Insecure;

        // Act
        var channel = _factory.CreateChannel(connectionString, port, serverName, credentials);

        // Assert
        Assert.NotNull(channel);
        Assert.Equal("test-server", channel.Target);
    }
} 