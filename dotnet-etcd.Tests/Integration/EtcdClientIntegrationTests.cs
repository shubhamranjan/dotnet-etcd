using Etcdserverpb;
using Google.Protobuf;

namespace dotnet_etcd.Tests.Integration;

[Trait("Category", "Integration")]
public class EtcdClientIntegrationTests
{
    [Fact]
    public async Task ConnectionTest()
    {
        // Arrange & Act
        using var client = new EtcdClient("127.0.0.1:2379");

        // Assert - Get status to verify connection works
        var statusResponse = await client.StatusAsync(new StatusRequest());
        Assert.NotNull(statusResponse);
        Assert.NotEmpty(statusResponse.Version);
    }

    [Fact]
    public async Task MultiEndpointConnectionTest()
    {
        // Arrange - Test with multiple endpoints (assuming at least one is valid)
        var endpoints = new[] { "127.0.0.1:2379", "127.0.0.1:9999" }; // Second one likely invalid

        // Act
        using var client = new EtcdClient(string.Join(",", endpoints));

        // Assert - Should connect to the valid endpoint
        var statusResponse = await client.StatusAsync(new StatusRequest());
        Assert.NotNull(statusResponse);
        Assert.NotEmpty(statusResponse.Version);
    }

    [Fact]
    public async Task BasicOperationsTest()
    {
        // Arrange
        using var client = new EtcdClient("127.0.0.1:2379");

        var testKey = "etcd-client-test-key";
        var testValue = "etcd-client-test-value";
        var updatedValue = "etcd-client-updated-value";

        try
        {
            // Clean up in case previous test didn't complete
            await client.DeleteAsync(testKey);

            // Act & Assert

            // 1. Put a key-value
            var putRequest = new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(testValue)
            };
            await client.PutAsync(putRequest);

            // 2. Get the value
            var getRequest = new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(testKey)
            };
            var getResponse = await client.GetAsync(getRequest);
            Assert.Single(getResponse.Kvs);
            Assert.Equal(testValue, getResponse.Kvs[0].Value.ToStringUtf8());

            // 3. Update the value
            var updateRequest = new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(updatedValue)
            };
            await client.PutAsync(updateRequest);

            // 4. Get the updated value
            var getUpdatedResponse = await client.GetAsync(getRequest);
            Assert.Single(getUpdatedResponse.Kvs);
            Assert.Equal(updatedValue, getUpdatedResponse.Kvs[0].Value.ToStringUtf8());

            // 5. Delete the key
            await client.DeleteAsync(testKey);

            // 6. Verify the key is gone
            var getAfterDeleteResponse = await client.GetAsync(getRequest);
            Assert.Empty(getAfterDeleteResponse.Kvs);
        }
        finally
        {
            // Clean up
            await client.DeleteAsync(testKey);
        }
    }

    [Fact]
    public async Task TransactionTest()
    {
        // Arrange
        using var client = new EtcdClient("127.0.0.1:2379");

        var testKey = "etcd-client-tx-test-key";
        var testValue = "etcd-client-tx-test-value";
        var ifMatchValue = "etcd-client-tx-match-value";
        var thenValue = "etcd-client-tx-then-value";
        var elseValue = "etcd-client-tx-else-value";

        try
        {
            // Setup - ensure the key exists with a known value
            var setupRequest = new PutRequest
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(testValue)
            };
            await client.PutAsync(setupRequest);

            // Act - Create a transaction that will fail the condition
            var failingTxn = new TxnRequest();

            // Add a compare condition that should fail
            failingTxn.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(ifMatchValue), // Different value
                Target = Compare.Types.CompareTarget.Value,
                Result = Compare.Types.CompareResult.Equal
            });

            // Add a success operation
            failingTxn.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey),
                    Value = ByteString.CopyFromUtf8(thenValue)
                }
            });

            // Add a failure operation
            failingTxn.Failure.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey),
                    Value = ByteString.CopyFromUtf8(elseValue)
                }
            });

            var failingResult = await client.TransactionAsync(failingTxn);

            // Assert
            Assert.False(failingResult.Succeeded);

            // Verify the ELSE clause was executed
            var getRequest = new RangeRequest
            {
                Key = ByteString.CopyFromUtf8(testKey)
            };
            var getResponse = await client.GetAsync(getRequest);
            Assert.Single(getResponse.Kvs);
            Assert.Equal(elseValue, getResponse.Kvs[0].Value.ToStringUtf8());

            // Act - Create a transaction that will succeed
            var succeedingTxn = new TxnRequest();

            // Add a compare condition that should succeed
            succeedingTxn.Compare.Add(new Compare
            {
                Key = ByteString.CopyFromUtf8(testKey),
                Value = ByteString.CopyFromUtf8(elseValue), // Now matches
                Target = Compare.Types.CompareTarget.Value,
                Result = Compare.Types.CompareResult.Equal
            });

            // Add a success operation
            succeedingTxn.Success.Add(new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(testKey),
                    Value = ByteString.CopyFromUtf8(thenValue)
                }
            });

            var succeedingResult = await client.TransactionAsync(succeedingTxn);

            // Assert
            Assert.True(succeedingResult.Succeeded);

            // Verify the THEN clause was executed
            getResponse = await client.GetAsync(getRequest);
            Assert.Single(getResponse.Kvs);
            Assert.Equal(thenValue, getResponse.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            // Clean up
            await client.DeleteAsync(testKey);
        }
    }
}