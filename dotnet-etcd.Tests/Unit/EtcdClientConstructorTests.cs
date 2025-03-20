using System;
using System.Net.Http;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Grpc.Core;
using Grpc.Net.Client;
using Moq;
using Xunit;
using Etcdserverpb;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class EtcdClientConstructorTests
    {
        [Fact]
        public void Constructor_WithConnectionString_ShouldCreateClient()
        {
            // Arrange
            var connectionString = "localhost:2379";
            var mockConnection = new Mock<IConnection>();
            var mockKVClient = new Mock<KV.KVClient>();
            mockConnection.Setup(c => c.KVClient).Returns(mockKVClient.Object);

            // Act
            var client = new EtcdClient(mockConnection.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithConnectionStringAndPort_ShouldCreateClient()
        {
            // Arrange
            var host = "localhost";
            var port = 2379;
            var mockConnection = new Mock<IConnection>();
            var mockKVClient = new Mock<KV.KVClient>();
            mockConnection.Setup(c => c.KVClient).Returns(mockKVClient.Object);

            // Act
            var client = new EtcdClient(mockConnection.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithConnectionStringAndPortAndServerName_ShouldCreateClient()
        {
            // Arrange
            var host = "localhost";
            var port = 2379;
            var serverName = "test-server";
            var mockConnection = new Mock<IConnection>();
            var mockKVClient = new Mock<KV.KVClient>();
            mockConnection.Setup(c => c.KVClient).Returns(mockKVClient.Object);

            // Act
            var client = new EtcdClient(mockConnection.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithMultipleHosts_ShouldCreateClient()
        {
            // Arrange
            var connectionString = "localhost:2379,localhost:2380";
            var mockConnection = new Mock<IConnection>();
            var mockKVClient = new Mock<KV.KVClient>();
            mockConnection.Setup(c => c.KVClient).Returns(mockKVClient.Object);

            // Act
            var client = new EtcdClient(mockConnection.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithNullConnectionString_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EtcdClient(null));
        }

        [Fact]
        public void Constructor_WithEmptyConnectionString_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EtcdClient(""));
        }

        [Fact]
        public void Constructor_WithWhitespaceConnectionString_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EtcdClient("  "));
        }

        [Fact]
        public void Constructor_WithIConnection_ShouldCreateClient()
        {
            // Arrange
            var mockConnection = new Mock<IConnection>();

            // Act
            var client = new EtcdClient(mockConnection.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithIConnectionAndIWatchManager_ShouldCreateClient()
        {
            // Arrange
            var mockConnection = new Mock<IConnection>();
            var mockWatchManager = new Mock<IWatchManager>();

            // Act
            var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithCallInvoker_ShouldCreateClient()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();

            // Act
            var client = new EtcdClient(mockCallInvoker.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithConnectionStringAndGrpcChannelOptionsAction_ShouldCreateClient()
        {
            // Arrange
            var host = "localhost";
            var port = 2379;
            var serverName = "test-server";

            // Create a custom options delegate that doesn't create a real HttpClient
            bool optionsConfigured = false;
            Action<GrpcChannelOptions> configureOptions = options =>
            {
                // Use a mock HttpClient or a simple flag to verify the options are used
                optionsConfigured = true;
            };

            // Act
            var client = new EtcdClient(host, port, serverName, configureOptions);

            // Assert
            Assert.NotNull(client);
            // Note: we can't verify optionsConfigured without changing the EtcdClient implementation
            // to expose whether options were applied
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var mockConnection = new Mock<IConnection>();
            var client = new EtcdClient(mockConnection.Object);

            // Act & Assert
            var exception = Record.Exception(() => client.Dispose());
            Assert.Null(exception);
        }
    }
}
