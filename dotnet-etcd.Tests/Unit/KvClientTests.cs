using System.Reflection;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class KvClientTests
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
        var asyncResponse = new AsyncUnaryCall<PutResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

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
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        });

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

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
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        });

        var asyncResponse = new AsyncUnaryCall<RangeResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

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
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        });
        expectedResponse.Count = 1; // Set the count to 1 to indicate there is a value

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

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
        var mockCallInvoker = new Mock<CallInvoker>();

        mockKvClient
            .Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DeleteRangeResponse());

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

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

    [Fact]
    public void DeleteRange_ShouldCallGrpcClientWithPrefixRange()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        mockKvClient
            .Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DeleteRangeResponse());

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        client.DeleteRange("test-prefix");

        // Assert
        mockKvClient.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r =>
                r.Key.ToStringUtf8() == "test-prefix" &&
                r.RangeEnd.ToStringUtf8() ==
                "test-prefiy"), // The range end is calculated by incrementing the last character
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetValAsync_ShouldReturnEmptyStringWhenKeyNotFound()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        // Not adding any KeyValue to the response to simulate key not found
        expectedResponse.Count = 0;

        var asyncResponse = new AsyncUnaryCall<RangeResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var result = await client.GetValAsync("non-existent-key");

        // Assert
        mockKvClient.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "non-existent-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetVal_ShouldReturnEmptyStringWhenKeyNotFound()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        // Not adding any KeyValue to the response to simulate key not found
        expectedResponse.Count = 0;

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var result = client.GetVal("non-existent-key");

        // Assert
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "non-existent-key"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRangeVal_ShouldReturnDictionary()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key1"),
            Value = ByteString.CopyFromUtf8("test-value1")
        });
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key2"),
            Value = ByteString.CopyFromUtf8("test-value2")
        });
        expectedResponse.Count = 2;

        mockKvClient
            .Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var result = client.GetRangeVal("test-prefix");

        // Assert
        mockKvClient.Verify(x => x.Range(
            It.Is<RangeRequest>(r =>
                r.Key.ToStringUtf8() == "test-prefix" &&
                r.RangeEnd.ToStringUtf8() ==
                "test-prefiy"), // The range end is calculated by incrementing the last character
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("test-value1", result["test-key1"]);
        Assert.Equal("test-value2", result["test-key2"]);
    }

    [Fact]
    public async Task GetRangeValAsync_ShouldReturnDictionary()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new RangeResponse();
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key1"),
            Value = ByteString.CopyFromUtf8("test-value1")
        });
        expectedResponse.Kvs.Add(new KeyValue
        {
            Key = ByteString.CopyFromUtf8("test-key2"),
            Value = ByteString.CopyFromUtf8("test-value2")
        });
        expectedResponse.Count = 2;

        var asyncResponse = new AsyncUnaryCall<RangeResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var result = await client.GetRangeValAsync("test-prefix");

        // Assert
        mockKvClient.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r =>
                r.Key.ToStringUtf8() == "test-prefix" &&
                r.RangeEnd.ToStringUtf8() ==
                "test-prefiy"), // The range end is calculated by incrementing the last character
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("test-value1", result["test-key1"]);
        Assert.Equal("test-value2", result["test-key2"]);
    }

    [Fact]
    public void Compact_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        mockKvClient
            .Setup(x => x.Compact(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CompactionResponse());

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var request = new CompactionRequest { Revision = 100, Physical = true };
        client.Compact(request);

        // Assert
        mockKvClient.Verify(x => x.Compact(
            It.Is<CompactionRequest>(r => r.Revision == 100 && r.Physical == true),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CompactAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();
        var mockCallInvoker = new Mock<CallInvoker>();

        var expectedResponse = new CompactionResponse();

        var asyncResponse = new AsyncUnaryCall<CompactionResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.CompactAsync(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = new EtcdClient(mockCallInvoker.Object);

        // Use reflection to set the KV client via the Connection field
        var connectionField = typeof(EtcdClient).GetField("_connection",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var connection = connectionField?.GetValue(client);

        if (connection == null) throw new InvalidOperationException("Failed to get _connection field from EtcdClient");

        // Set the KVClient property on the Connection object
        var kvClientField = connection.GetType().GetField("_kvClient",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (kvClientField == null) throw new InvalidOperationException("Failed to get _kvClient field from Connection");

        kvClientField.SetValue(connection, mockKvClient.Object);

        // Act
        var request = new CompactionRequest { Revision = 100, Physical = true };
        await client.CompactAsync(request);

        // Assert
        mockKvClient.Verify(x => x.CompactAsync(
            It.Is<CompactionRequest>(r => r.Revision == 100 && r.Physical == true),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}