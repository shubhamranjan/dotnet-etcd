using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class TransactionIntegrationTests
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;

    public TransactionIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });
    }

    [Fact]
    public void Transaction_WithSuccessfulCondition_ShouldExecuteSuccessOperations()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var successKey = $"success-key-{Guid.NewGuid()}";
        var failureKey = $"failure-key-{Guid.NewGuid()}";

        try
        {
            // Setup initial state
            _client.Put(testKey, "test-value");

            // Create transaction
            var txnRequest = new TxnRequest();

            // Add a compare condition that should succeed
            txnRequest.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Result = Compare.Types.CompareResult.Equal,
                Target = Compare.Types.CompareTarget.Value,
                Value = ByteString.CopyFromUtf8("test-value")
            });

            // Add a success operation
            txnRequest.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(successKey),
                    Value = ByteString.CopyFromUtf8("success-value")
                }
            });

            // Add a failure operation
            txnRequest.Failure.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(failureKey),
                    Value = ByteString.CopyFromUtf8("failure-value")
                }
            });

            // Act
            var txnResponse = _client.Transaction(txnRequest);
            var successKeyExists = _client.Get(successKey);
            var failureKeyExists = _client.Get(failureKey);

            // Assert
            Assert.True(txnResponse.Succeeded);
            Assert.Single(successKeyExists.Kvs);
            Assert.Equal("success-value", successKeyExists.Kvs[0].Value.ToStringUtf8());
            Assert.Empty(failureKeyExists.Kvs);
        }
        finally
        {
            // Cleanup
            _client.Delete(testKey);
            _client.Delete(successKey);
            _client.Delete(failureKey);
        }
    }

    [Fact]
    public void Transaction_WithFailedCondition_ShouldExecuteFailureOperations()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var successKey = $"success-key-{Guid.NewGuid()}";
        var failureKey = $"failure-key-{Guid.NewGuid()}";

        try
        {
            // Setup initial state
            _client.Put(testKey, "test-value");

            // Create transaction
            var txnRequest = new TxnRequest();

            // Add a compare condition that should fail
            txnRequest.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Result = Compare.Types.CompareResult.Equal,
                Target = Compare.Types.CompareTarget.Value,
                Value = ByteString.CopyFromUtf8("wrong-value")
            });

            // Add a success operation
            txnRequest.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(successKey),
                    Value = ByteString.CopyFromUtf8("success-value")
                }
            });

            // Add a failure operation
            txnRequest.Failure.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(failureKey),
                    Value = ByteString.CopyFromUtf8("failure-value")
                }
            });

            // Act
            var txnResponse = _client.Transaction(txnRequest);
            var successKeyExists = _client.Get(successKey);
            var failureKeyExists = _client.Get(failureKey);

            // Assert
            Assert.False(txnResponse.Succeeded);
            Assert.Empty(successKeyExists.Kvs);
            Assert.Single(failureKeyExists.Kvs);
            Assert.Equal("failure-value", failureKeyExists.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            // Cleanup
            _client.Delete(testKey);
            _client.Delete(successKey);
            _client.Delete(failureKey);
        }
    }

    [Fact]
    public async Task TransactionAsync_WithSuccessfulCondition_ShouldExecuteSuccessOperations()
    {
        // Arrange
        var testKey = $"test-key-{Guid.NewGuid()}";
        var successKey = $"success-key-{Guid.NewGuid()}";
        var failureKey = $"failure-key-{Guid.NewGuid()}";

        try
        {
            // Setup initial state
            await _client.PutAsync(testKey, "test-value");

            // Create transaction
            var txnRequest = new TxnRequest();

            // Add a compare condition that should succeed
            txnRequest.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Result = Compare.Types.CompareResult.Equal,
                Target = Compare.Types.CompareTarget.Value,
                Value = ByteString.CopyFromUtf8("test-value")
            });

            // Add a success operation
            txnRequest.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(successKey),
                    Value = ByteString.CopyFromUtf8("success-value")
                }
            });

            // Add a failure operation
            txnRequest.Failure.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(failureKey),
                    Value = ByteString.CopyFromUtf8("failure-value")
                }
            });

            // Act
            var txnResponse = await _client.TransactionAsync(txnRequest);
            var successKeyExists = await _client.GetAsync(successKey);
            var failureKeyExists = await _client.GetAsync(failureKey);

            // Assert
            Assert.True(txnResponse.Succeeded);
            Assert.Single(successKeyExists.Kvs);
            Assert.Equal("success-value", successKeyExists.Kvs[0].Value.ToStringUtf8());
            Assert.Empty(failureKeyExists.Kvs);
        }
        finally
        {
            // Cleanup
            await _client.DeleteAsync(testKey);
            await _client.DeleteAsync(successKey);
            await _client.DeleteAsync(failureKey);
        }
    }

    [Fact]
    public void Transaction_WithMultipleConditions_ShouldWorkCorrectly()
    {
        // Arrange
        var key1 = $"test-key1-{Guid.NewGuid()}";
        var key2 = $"test-key2-{Guid.NewGuid()}";
        var resultKey = $"result-key-{Guid.NewGuid()}";

        try
        {
            // Setup initial state
            _client.Put(key1, "value1");
            _client.Put(key2, "value2");

            // Create transaction
            var txnRequest = new TxnRequest();

            // Add multiple compare conditions
            txnRequest.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(key1),
                Result = Compare.Types.CompareResult.Equal,
                Target = Compare.Types.CompareTarget.Value,
                Value = ByteString.CopyFromUtf8("value1")
            });

            txnRequest.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(key2),
                Result = Compare.Types.CompareResult.Equal,
                Target = Compare.Types.CompareTarget.Value,
                Value = ByteString.CopyFromUtf8("value2")
            });

            // Add a success operation
            txnRequest.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(resultKey),
                    Value = ByteString.CopyFromUtf8("both-conditions-met")
                }
            });

            // Add a failure operation
            txnRequest.Failure.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(resultKey),
                    Value = ByteString.CopyFromUtf8("conditions-not-met")
                }
            });

            // Act
            var txnResponse = _client.Transaction(txnRequest);
            var resultValue = _client.GetVal(resultKey);

            // Assert
            Assert.True(txnResponse.Succeeded);
            Assert.Equal("both-conditions-met", resultValue);
        }
        finally
        {
            // Cleanup
            _client.Delete(key1);
            _client.Delete(key2);
            _client.Delete(resultKey);
        }
    }
}