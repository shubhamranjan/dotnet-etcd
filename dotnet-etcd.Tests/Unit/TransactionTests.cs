using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class TransactionTests
{
    [Fact]
    public void Transaction_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        var expectedResponse = new TxnResponse
        {
            Succeeded = true
        };
        expectedResponse.Responses.Add(new ResponseOp
        {
            ResponsePut = new PutResponse()
        });

        mockKvClient
            .Setup(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var request = new TxnRequest();

        // Add a compare condition
        request.Compare.Add(new Compare
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Target = Compare.Types.CompareTarget.Value,
            Result = Compare.Types.CompareResult.Equal,
            Value = ByteString.CopyFromUtf8("test-value")
        });

        // Add a success operation
        request.Success.Add(new RequestOp
        {
            RequestPut = new PutRequest
            {
                Key = ByteString.CopyFromUtf8("success-key"),
                Value = ByteString.CopyFromUtf8("success-value")
            }
        });

        var result = client.Transaction(request);

        // Assert
        mockKvClient.Verify(x => x.Txn(
            It.Is<TxnRequest>(r =>
                r.Compare.Count == 1 &&
                r.Success.Count == 1),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.True(result.Succeeded);
        Assert.Single(result.Responses);
        Assert.NotNull(result.Responses[0].ResponsePut);
    }

    [Fact]
    public async Task TransactionAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        var expectedResponse = new TxnResponse
        {
            Succeeded = true
        };
        expectedResponse.Responses.Add(new ResponseOp
        {
            ResponsePut = new PutResponse()
        });

        var asyncResponse = new AsyncUnaryCall<TxnResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.TxnAsync(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var request = new TxnRequest();

        // Add a compare condition
        request.Compare.Add(new Compare
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Target = Compare.Types.CompareTarget.Value,
            Result = Compare.Types.CompareResult.Equal,
            Value = ByteString.CopyFromUtf8("test-value")
        });

        // Add a success operation
        request.Success.Add(new RequestOp
        {
            RequestPut = new PutRequest
            {
                Key = ByteString.CopyFromUtf8("success-key"),
                Value = ByteString.CopyFromUtf8("success-value")
            }
        });

        var result = await client.TransactionAsync(request);

        // Assert
        mockKvClient.Verify(x => x.TxnAsync(
            It.Is<TxnRequest>(r =>
                r.Compare.Count == 1 &&
                r.Success.Count == 1),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.True(result.Succeeded);
        Assert.Single(result.Responses);
        Assert.NotNull(result.Responses[0].ResponsePut);
    }

    [Fact]
    public void Transaction_WithFailedCondition_ShouldExecuteFailureOperations()
    {
        // Arrange
        var mockKvClient = new Mock<KV.KVClient>();

        var expectedResponse = new TxnResponse
        {
            Succeeded = false
        };
        expectedResponse.Responses.Add(new ResponseOp
        {
            ResponseRange = new RangeResponse()
        });

        mockKvClient
            .Setup(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock KV client
        TestHelper.SetupMockClientViaConnection(client, mockKvClient.Object, "_kvClient");

        // Act
        var request = new TxnRequest();

        // Add a compare condition that will fail
        request.Compare.Add(new Compare
        {
            Key = ByteString.CopyFromUtf8("test-key"),
            Target = Compare.Types.CompareTarget.Value,
            Result = Compare.Types.CompareResult.Equal,
            Value = ByteString.CopyFromUtf8("test-value")
        });

        // Add a failure operation
        request.Failure.Add(new RequestOp
        {
            RequestRange = new RangeRequest
            {
                Key = ByteString.CopyFromUtf8("failure-key")
            }
        });

        var result = client.Transaction(request);

        // Assert
        mockKvClient.Verify(x => x.Txn(
            It.Is<TxnRequest>(r =>
                r.Compare.Count == 1 &&
                r.Failure.Count == 1),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.False(result.Succeeded);
        Assert.Single(result.Responses);
        Assert.NotNull(result.Responses[0].ResponseRange);
    }
}