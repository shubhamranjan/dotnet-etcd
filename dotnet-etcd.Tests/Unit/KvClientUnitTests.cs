using System.Reflection;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class KvClientUnitTests
{
    [Fact]
    public void Put_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        mockKvClient
            .Setup(x => x.Put(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new PutResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        client.Put("test-key", "test-value");

        // Assert
        mockKvClient.Verify(x => x.Put(
            It.Is<PutRequest>(r =>
                r.Key.ToStringUtf8() == "test-key" &&
                r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task PutAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        var expectedResponse = new PutResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockKvClient
            .Setup(x => x.PutAsync(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        await client.PutAsync("test-key", "test-value");

        // Assert
        mockKvClient.Verify(x => x.PutAsync(
            It.Is<PutRequest>(r =>
                r.Key.ToStringUtf8() == "test-key" &&
                r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Get_ShouldReturnValue()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var expectedResponse = TestHelper.CreateRangeResponse("test-key", "test-value");

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var result = client.Get("test-key");

        // Assert
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Single(result.Kvs);
        Assert.Equal("test-key", result.Kvs[0].Key.ToStringUtf8());
        Assert.Equal("test-value", result.Kvs[0].Value.ToStringUtf8());
    }

    [Fact]
    public async Task GetAsync_ShouldReturnValue()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var expectedResponse = TestHelper.CreateRangeResponse("test-key", "test-value");
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockKvClient
            .Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var result = await client.GetAsync("test-key");

        // Assert
        mockKvClient.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Single(result.Kvs);
        Assert.Equal("test-key", result.Kvs[0].Key.ToStringUtf8());
        Assert.Equal("test-value", result.Kvs[0].Value.ToStringUtf8());
    }

    [Fact]
    public void GetVal_ShouldReturnStringValue()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        
        // Create a response with a key-value pair
        var kvPair = new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        };
        
        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(kvPair);
        expectedResponse.Count = 1;

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var result = client.GetVal("test-key");

        // Assert
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("test-value", result);
    }

    [Fact]
    public void Delete_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        mockKvClient
            .Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DeleteRangeResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        client.Delete("test-key");

        // Assert
        mockKvClient.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
