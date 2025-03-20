using System;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Grpc.Core;
using Moq;
using Xunit;
using System.Reflection;
using System.Linq;
using System.Threading;
using Etcdserverpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientUtilAdditionalTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly EtcdClient _client;

    public EtcdClientUtilAdditionalTests()
    {
        _mockConnection = MockConnection.Create();
        _client = new EtcdClient(_mockConnection.Object);
    }

    [Fact]
    public void CallEtcdAsync_VoidReturn_ShouldCallFunctionWithConnection()
    {
        // Since we can't directly test the private method, we'll verify the behavior
        // by checking that the client correctly calls methods on the connection
        
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockKvClient = new Mock<KV.KVClient>();
        
        // Setup the mock KV client to return a successful response
        var putResponse = new PutResponse();
        var asyncCall = AsyncUnaryCallFactory.Create(putResponse);
        mockKvClient
            .Setup(x => x.PutAsync(
                It.IsAny<PutRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(asyncCall);
        
        // Setup the connection to return our mocked KV client
        mockConnection.SetupGet(x => x.KVClient).Returns(mockKvClient.Object);
        
        // Create a client with the mocked connection
        var client = new EtcdClient(mockConnection.Object);
        
        // Act
        client.Put("test-key", "test-value");
        
        // Assert
        // Verify that the KV client's PutAsync method was called, which indirectly
        // verifies that CallEtcd worked correctly
        mockKvClient.Verify(x => x.Put(
            It.Is<PutRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CallEtcdAsync_VoidReturn_WithException_ShouldPropagateException()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockKvClient = new Mock<KV.KVClient>();
        var expectedException = new InvalidOperationException("Test exception");
        
        // Setup the KV client to throw when PutAsync is called
        mockKvClient.Setup(x => x.PutAsync(
            It.IsAny<PutRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        )).Throws(expectedException);
        
        // Setup the connection to return our mocked KV client
        mockConnection.SetupGet(x => x.KVClient).Returns(mockKvClient.Object);
        
        // Create a client with the mocked connection
        var client = new EtcdClient(mockConnection.Object);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await client.PutAsync("test-key", "test-value"));
        
        // Verify that the exception was propagated correctly
        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public void CallEtcd_WithException_ShouldPropagateException()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var expectedException = new InvalidOperationException("Test exception");
        
        // Setup the connection to throw an exception when KVClient is accessed
        mockConnection.SetupGet(x => x.KVClient).Throws(expectedException);
        
        // Create a client with the mocked connection
        var client = new EtcdClient(mockConnection.Object);

        // Act & Assert
        var actualException = Assert.Throws<InvalidOperationException>(() => client.Get("test-key"));
        
        // Verify that the exception was propagated correctly
        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task CallEtcdAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var expectedException = new InvalidOperationException("Test exception");
        
        // Setup the connection to throw an exception when KVClient is accessed
        mockConnection.SetupGet(x => x.KVClient).Throws(expectedException);
        
        // Create a client with the mocked connection
        var client = new EtcdClient(mockConnection.Object);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await client.GetAsync("test-key"));
        
        // Verify that the exception was propagated correctly
        Assert.Same(expectedException, actualException);
    }
}
