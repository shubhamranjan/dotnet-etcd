using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;
using System.Collections.Generic;
using dotnet_etcd.interfaces;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientUnitTests
{
    [Fact]
    public void Constructor_WithEndPoint_ShouldInitializeCorrectly()
    {
        // Act
        var client = new EtcdClient("127.0.0.1:2379");
        
        // Assert
        Assert.NotNull(client);
    }
    
    [Fact]
    public void Constructor_WithEndPoints_ShouldInitializeCorrectly()
    {
        // Arrange
        var endpoints = "127.0.0.1:2379,127.0.0.1:2380";
        
        // Act
        var client = new EtcdClient(endpoints);
        
        // Assert
        Assert.NotNull(client);
    }
    
    [Fact]
    public void Constructor_WithCallInvoker_ShouldInitializeCorrectly()
    {
        // Arrange
        var mockCallInvoker = new Mock<CallInvoker>();
        
        // Act
        var client = new EtcdClient(mockCallInvoker.Object);
        
        // Assert
        Assert.NotNull(client);
    }
    
    [Fact]
    public void Dispose_ShouldDisposeCorrectly()
    {
        // Arrange
        var client = new EtcdClient("127.0.0.1:2379");
        
        // Act & Assert - Should not throw
        client.Dispose();
    }
    
    [Fact]
    public void AuthenticateUser_ShouldSetAuthToken()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        mockAuthClient
            .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthenticateResponse { Token = "test-token" });
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");
        
        // Act
        var request = new AuthenticateRequest { Name = "user", Password = "password" };
        var response = client.Authenticate(request);
        
        // Assert
        Assert.Equal("test-token", response.Token);
        mockAuthClient.Verify(x => x.Authenticate(
            It.Is<AuthenticateRequest>(r => r.Name == "user" && r.Password == "password"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public void Status_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();
        mockMaintenanceClient
            .Setup(x => x.Status(It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new StatusResponse { Version = "3.4.0" });
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");
        
        // Act
        var response = client.Status(new StatusRequest());
        
        // Assert
        Assert.Equal("3.4.0", response.Version);
        mockMaintenanceClient.Verify(x => x.Status(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public async Task StatusAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();
        var expectedResponse = new StatusResponse { Version = "3.4.0" };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);
        
        mockMaintenanceClient
            .Setup(x => x.StatusAsync(It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");
        
        // Act
        var response = await client.StatusAsync(new StatusRequest());
        
        // Assert
        Assert.Equal("3.4.0", response.Version);
        mockMaintenanceClient.Verify(x => x.StatusAsync(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public void Put_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        mockKvClient
            .Setup(x => x.Put(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new PutResponse());
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");
        
        // Act
        var key = "test-key";
        var value = "test-value";
        var request = new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key),
            Value = ByteString.CopyFromUtf8(value)
        };
        
        client.Put(request);
        
        // Assert
        mockKvClient.Verify(x => x.Put(
            It.Is<PutRequest>(r => 
                r.Key.ToStringUtf8() == key && 
                r.Value.ToStringUtf8() == value),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public void Get_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var expectedResponse = new RangeResponse 
        { 
            Kvs = 
            { 
                new KeyValue 
                { 
                    Key = ByteString.CopyFromUtf8("test-key"),
                    Value = ByteString.CopyFromUtf8("test-value") 
                } 
            } 
        };
        
        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");
        
        // Act
        var key = "test-key";
        var request = new RangeRequest
        {
            Key = ByteString.CopyFromUtf8(key)
        };
        var response = client.Get(request);
        
        // Assert
        Assert.Single(response.Kvs);
        Assert.Equal("test-key", response.Kvs[0].Key.ToStringUtf8());
        Assert.Equal("test-value", response.Kvs[0].Value.ToStringUtf8());
        
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == key),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public void Delete_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        mockKvClient
            .Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DeleteRangeResponse { Deleted = 1 });
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");
        
        // Act
        var key = "test-key";
        var request = new DeleteRangeRequest
        {
            Key = ByteString.CopyFromUtf8(key)
        };
        var response = client.Delete(request);
        
        // Assert
        Assert.Equal(1, response.Deleted);
        
        mockKvClient.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == key),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
    
    [Fact]
    public void Transaction_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        mockKvClient
            .Setup(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new TxnResponse { Succeeded = true });
        
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");
        
        // Act
        var txnRequest = new TxnRequest();
        txnRequest.Compare.Add(new Compare
        {
            Key = ByteString.CopyFromUtf8("key"),
            Target = Compare.Types.CompareTarget.Value,
            Result = Compare.Types.CompareResult.Equal,
            Value = ByteString.CopyFromUtf8("value")
        });
        
        txnRequest.Success.Add(new RequestOp
        {
            RequestPut = new PutRequest
            {
                Key = ByteString.CopyFromUtf8("key"),
                Value = ByteString.CopyFromUtf8("new-value")
            }
        });
        
        var response = client.Transaction(txnRequest);
        
        // Assert
        Assert.True(response.Succeeded);
        
        mockKvClient.Verify(x => x.Txn(
            It.IsAny<TxnRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
