using dotnet_etcd.interfaces;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientCoreCoverageTests
{
    [Fact]
    public void GetConnection_ShouldReturnInjectedConnection()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var watchManager = new Mock<IWatchManager>().Object;
        var client = new EtcdClient(connection, watchManager);

        // Act & Assert
        Assert.Same(connection, client.GetConnection());
    }

    [Fact]
    public void GetWatchManager_ShouldReturnInjectedWatchManager()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var watchManager = new Mock<IWatchManager>().Object;
        var client = new EtcdClient(connection, watchManager);

        // Act & Assert
        Assert.Same(watchManager, client.GetWatchManager());
    }

    [Fact]
    public void Constructor_WithNullConnection_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EtcdClient((IConnection)null, new Mock<IWatchManager>().Object));
    }

    [Fact]
    public void CancelWatch_Single_ShouldDelegateToWatchManager()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(connection, mockWatchManager.Object);

        // Act
        client.CancelWatch(5);

        // Assert
        mockWatchManager.Verify(m => m.CancelWatch(5), Times.Once);
    }

    [Fact]
    public void CancelWatch_Array_ShouldDelegateEachIdToWatchManager()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(connection, mockWatchManager.Object);
        var ids = new long[] { 1, 2, 3 };

        // Act
        client.CancelWatch(ids);

        // Assert
        foreach (var id in ids)
        {
            mockWatchManager.Verify(m => m.CancelWatch(id), Times.Once);
        }

        mockWatchManager.Verify(m => m.CancelWatch(It.IsAny<long>()), Times.Exactly(ids.Length));
    }

    [Fact]
    public void CancelWatch_EmptyArray_ShouldNotCallWatchManager()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(connection, mockWatchManager.Object);

        // Act
        client.CancelWatch(Array.Empty<long>());

        // Assert
        mockWatchManager.Verify(m => m.CancelWatch(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Dispose_ShouldDisposeWatchManager()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(connection, mockWatchManager.Object);

        // Act
        client.Dispose();

        // Assert
        mockWatchManager.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        var connection = new Mock<IConnection>().Object;
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(connection, mockWatchManager.Object);

        // Act - calling Dispose twice must not throw
        client.Dispose();
        client.Dispose();

        // Assert - the guard (_disposed) ensures the watch manager is disposed only once
        mockWatchManager.Verify(m => m.Dispose(), Times.Once);
    }
}
