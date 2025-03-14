using dotnet_etcd.interfaces;
using Etcdserverpb;
using Moq;
using V3Electionpb;
using Lock = V3Lockpb.Lock;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     Helper class to create a mock IConnection
/// </summary>
public static class MockConnection
{
    /// <summary>
    ///     Creates a fully mocked IConnection with all client properties set up
    /// </summary>
    /// <returns>A Mock of IConnection</returns>
    public static Mock<IConnection> Create()
    {
        var mockConnection = new Mock<IConnection>();

        // Set up all the client properties
        mockConnection.Setup(c => c.KVClient).Returns(new Mock<KV.KVClient>().Object);
        mockConnection.Setup(c => c.WatchClient).Returns(new Mock<Watch.WatchClient>().Object);
        mockConnection.Setup(c => c.LeaseClient).Returns(new Mock<Lease.LeaseClient>().Object);
        mockConnection.Setup(c => c.LockClient).Returns(new Mock<Lock.LockClient>().Object);
        mockConnection.Setup(c => c.ClusterClient).Returns(new Mock<Cluster.ClusterClient>().Object);
        mockConnection.Setup(c => c.MaintenanceClient).Returns(new Mock<Maintenance.MaintenanceClient>().Object);
        mockConnection.Setup(c => c.AuthClient).Returns(new Mock<Auth.AuthClient>().Object);
        mockConnection.Setup(c => c.ElectionClient).Returns(new Mock<Election.ElectionClient>().Object);

        return mockConnection;
    }

    /// <summary>
    ///     Sets up the KVClient in a mock IConnection
    /// </summary>
    /// <param name="connection">The mock connection to set up</param>
    /// <returns>The KVClient mock for further setup</returns>
    public static Mock<KV.KVClient> SetupKVClient(this Mock<IConnection> connection)
    {
        var mockKvClient = new Mock<KV.KVClient>();
        connection.Setup(c => c.KVClient).Returns(mockKvClient.Object);
        return mockKvClient;
    }

    /// <summary>
    ///     Sets up the WatchClient in a mock IConnection
    /// </summary>
    /// <param name="connection">The mock connection to set up</param>
    /// <returns>The WatchClient mock for further setup</returns>
    public static Mock<Watch.WatchClient> SetupWatchClient(this Mock<IConnection> connection)
    {
        var mockWatchClient = new Mock<Watch.WatchClient>();
        connection.Setup(c => c.WatchClient).Returns(mockWatchClient.Object);
        return mockWatchClient;
    }

    /// <summary>
    ///     Sets up the LeaseClient in a mock IConnection
    /// </summary>
    /// <param name="connection">The mock connection to set up</param>
    /// <returns>The LeaseClient mock for further setup</returns>
    public static Mock<Lease.LeaseClient> SetupLeaseClient(this Mock<IConnection> connection)
    {
        var mockLeaseClient = new Mock<Lease.LeaseClient>();
        connection.Setup(c => c.LeaseClient).Returns(mockLeaseClient.Object);
        return mockLeaseClient;
    }

    /// <summary>
    ///     Sets up the AuthClient in a mock IConnection
    /// </summary>
    /// <param name="connection">The mock connection to set up</param>
    /// <returns>The AuthClient mock for further setup</returns>
    public static Mock<Auth.AuthClient> SetupAuthClient(this Mock<IConnection> connection)
    {
        var mockAuthClient = new Mock<Auth.AuthClient>();
        connection.Setup(c => c.AuthClient).Returns(mockAuthClient.Object);
        return mockAuthClient;
    }
}