using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class KvClientCoverageTests
{
    // ---------------------------------------------------------------------
    // Get (RangeRequest overload)
    // ---------------------------------------------------------------------
    [Fact]
    public void Get_WithRangeRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new RangeRequest { Key = ByteString.CopyFromUtf8("test-key") };
        var result = client.Get(request);

        Assert.NotNull(result);
        mock.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Get_WithStringKey_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        client.Get("test-key");

        mock.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithRangeRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new RangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new RangeRequest { Key = ByteString.CopyFromUtf8("test-key") };
        var result = await client.GetAsync(request);

        Assert.NotNull(result);
        mock.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithStringKey_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new RangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        await client.GetAsync("test-key");

        mock.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // GetVal / GetValAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void GetVal_WhenKeyExists_ShouldReturnTrimmedValue()
    {
        var mock = new Mock<KV.KVClient>();
        var response = new RangeResponse { Count = 1 };
        response.Kvs.Add(TestHelper.CreateKeyValue("test-key", "  test-value  "));
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(response);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.GetVal("test-key");

        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetVal_WhenKeyMissing_ShouldReturnEmptyString()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse { Count = 0 });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.GetVal("missing-key");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetValAsync_WhenKeyExists_ShouldReturnTrimmedValue()
    {
        var mock = new Mock<KV.KVClient>();
        var response = new RangeResponse { Count = 1 };
        response.Kvs.Add(TestHelper.CreateKeyValue("test-key", "  test-value  "));
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(response));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.GetValAsync("test-key");

        Assert.Equal("test-value", result);
    }

    [Fact]
    public async Task GetValAsync_WhenKeyMissing_ShouldReturnEmptyString()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new RangeResponse { Count = 0 }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.GetValAsync("missing-key");

        Assert.Equal(string.Empty, result);
    }

    // ---------------------------------------------------------------------
    // GetRange / GetRangeAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void GetRange_ShouldCallGrpcClientWithRangeEnd()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.GetRange("test/");

        Assert.NotNull(result);
        mock.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.RangeEnd.Length > 0),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRangeAsync_ShouldCallGrpcClientWithRangeEnd()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new RangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.GetRangeAsync("test/");

        Assert.NotNull(result);
        mock.Verify(x => x.RangeAsync(
            It.Is<RangeRequest>(r => r.RangeEnd.Length > 0),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // GetRangeVal / GetRangeValAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void GetRangeVal_ShouldReturnDictionary()
    {
        var mock = new Mock<KV.KVClient>();
        var response = new RangeResponse();
        response.Kvs.Add(TestHelper.CreateKeyValue("test/key1", "value1"));
        response.Kvs.Add(TestHelper.CreateKeyValue("test/key2", "value2"));
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(response);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.GetRangeVal("test/");

        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["test/key1"]);
        Assert.Equal("value2", result["test/key2"]);
    }

    [Fact]
    public async Task GetRangeValAsync_ShouldReturnDictionary()
    {
        var mock = new Mock<KV.KVClient>();
        var response = new RangeResponse();
        response.Kvs.Add(TestHelper.CreateKeyValue("test/key1", "value1"));
        mock.Setup(x => x.RangeAsync(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(response));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.GetRangeValAsync("test/");

        Assert.Single(result);
        Assert.Equal("value1", result["test/key1"]);
    }

    // ---------------------------------------------------------------------
    // Put (PutRequest + string overloads)
    // ---------------------------------------------------------------------
    [Fact]
    public void Put_WithPutRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Put(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new PutResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new PutRequest
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        };
        var result = client.Put(request);

        Assert.NotNull(result);
        mock.Verify(x => x.Put(
            It.Is<PutRequest>(r => r.Key.ToStringUtf8() == "test-key" && r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Put_WithStringKeyValue_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Put(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new PutResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        client.Put("test-key", "test-value");

        mock.Verify(x => x.Put(
            It.Is<PutRequest>(r => r.Key.ToStringUtf8() == "test-key" && r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PutAsync_WithPutRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.PutAsync(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new PutResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new PutRequest
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Value = ByteString.CopyFromUtf8("test-value")
        };
        var result = await client.PutAsync(request);

        Assert.NotNull(result);
        mock.Verify(x => x.PutAsync(
            It.Is<PutRequest>(r => r.Key.ToStringUtf8() == "test-key" && r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PutAsync_WithStringKeyValue_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.PutAsync(It.IsAny<PutRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new PutResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        await client.PutAsync("test-key", "test-value");

        mock.Verify(x => x.PutAsync(
            It.Is<PutRequest>(r => r.Key.ToStringUtf8() == "test-key" && r.Value.ToStringUtf8() == "test-value"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Delete (DeleteRangeRequest + string overloads)
    // ---------------------------------------------------------------------
    [Fact]
    public void Delete_WithDeleteRangeRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new DeleteRangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new DeleteRangeRequest { Key = ByteString.CopyFromUtf8("test-key") };
        var result = client.Delete(request);

        Assert.NotNull(result);
        mock.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Delete_WithStringKey_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new DeleteRangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        client.Delete("test-key");

        mock.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithDeleteRangeRequest_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRangeAsync(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new DeleteRangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var request = new DeleteRangeRequest { Key = ByteString.CopyFromUtf8("test-key") };
        var result = await client.DeleteAsync(request);

        Assert.NotNull(result);
        mock.Verify(x => x.DeleteRangeAsync(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithStringKey_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRangeAsync(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new DeleteRangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        await client.DeleteAsync("test-key");

        mock.Verify(x => x.DeleteRangeAsync(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // DeleteRange / DeleteRangeAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void DeleteRange_ShouldCallGrpcClientWithRangeEnd()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRange(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new DeleteRangeResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.DeleteRange("test/");

        Assert.NotNull(result);
        mock.Verify(x => x.DeleteRange(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test/" && r.RangeEnd.Length > 0),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldCallGrpcClientWithRangeEnd()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.DeleteRangeAsync(It.IsAny<DeleteRangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new DeleteRangeResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.DeleteRangeAsync("test/");

        Assert.NotNull(result);
        mock.Verify(x => x.DeleteRangeAsync(
            It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test/" && r.RangeEnd.Length > 0),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Transaction / TransactionAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void Transaction_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new TxnResponse { Succeeded = true });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.Transaction(new TxnRequest());

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        mock.Verify(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransactionAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.TxnAsync(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new TxnResponse { Succeeded = true }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.TransactionAsync(new TxnRequest());

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        mock.Verify(x => x.TxnAsync(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Compact / CompactAsync
    // ---------------------------------------------------------------------
    [Fact]
    public void Compact_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Compact(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new CompactionResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = client.Compact(new CompactionRequest { Revision = 5 });

        Assert.NotNull(result);
        mock.Verify(x => x.Compact(
            It.Is<CompactionRequest>(r => r.Revision == 5),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompactAsync_ShouldCallGrpcClient()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.CompactAsync(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(TestHelper.CreateAsyncUnaryCall(new CompactionResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        var result = await client.CompactAsync(new CompactionRequest { Revision = 5 });

        Assert.NotNull(result);
        mock.Verify(x => x.CompactAsync(
            It.Is<CompactionRequest>(r => r.Revision == 5),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
