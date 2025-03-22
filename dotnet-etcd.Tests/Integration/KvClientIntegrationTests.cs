using dotnet_etcd.Tests.Infrastructure;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class KvClientIntegrationTests
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;

    public KvClientIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });
    }

    [Fact]
    public void Put_ShouldStoreValue()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        try
        {
            // Act
            var putResponse = _client.Put(key, value);
            var getResponse = _client.Get(key);

            // Assert
            Assert.NotNull(putResponse);
            Assert.NotNull(getResponse);
            Assert.Single(getResponse.Kvs);
            Assert.Equal(key, getResponse.Kvs[0].Key.ToStringUtf8());
            Assert.Equal(value, getResponse.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            // Cleanup
            _client.Delete(key);
        }
    }

    [Fact]
    public async Task PutAsync_ShouldStoreValue()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        try
        {
            // Act
            var putResponse = await _client.PutAsync(key, value);
            var getResponse = await _client.GetAsync(key);

            // Assert
            Assert.NotNull(putResponse);
            Assert.NotNull(getResponse);
            Assert.Single(getResponse.Kvs);
            Assert.Equal(key, getResponse.Kvs[0].Key.ToStringUtf8());
            Assert.Equal(value, getResponse.Kvs[0].Value.ToStringUtf8());
        }
        finally
        {
            // Cleanup
            await _client.DeleteAsync(key);
        }
    }

    [Fact]
    public void GetVal_ShouldReturnStringValue()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        try
        {
            // Act
            _client.Put(key, value);
            var result = _client.GetVal(key);

            // Assert
            Assert.Equal(value, result);
        }
        finally
        {
            // Cleanup
            _client.Delete(key);
        }
    }

    [Fact]
    public async Task GetValAsync_ShouldReturnStringValue()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        try
        {
            // Act
            await _client.PutAsync(key, value);
            var result = await _client.GetValAsync(key);

            // Assert
            Assert.Equal(value, result);
        }
        finally
        {
            // Cleanup
            await _client.DeleteAsync(key);
        }
    }

    [Fact]
    public void Delete_ShouldRemoveKey()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        // Act
        _client.Put(key, value);
        var deleteResponse = _client.Delete(key);
        var getResponse = _client.Get(key);

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(1, deleteResponse.Deleted);
        Assert.NotNull(getResponse);
        Assert.Empty(getResponse.Kvs);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveKey()
    {
        // Arrange
        var key = $"test-key-{Guid.NewGuid()}";
        var value = "test-value";

        // Act
        await _client.PutAsync(key, value);
        var deleteResponse = await _client.DeleteAsync(key);
        var getResponse = await _client.GetAsync(key);

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(1, deleteResponse.Deleted);
        Assert.NotNull(getResponse);
        Assert.Empty(getResponse.Kvs);
    }

    [Fact]
    public void GetRange_ShouldReturnMultipleKeys()
    {
        // Arrange
        var prefix = $"test-prefix-{Guid.NewGuid()}";
        var key1 = $"{prefix}/key1";
        var key2 = $"{prefix}/key2";
        var key3 = $"{prefix}/key3";

        try
        {
            // Act
            _client.Put(key1, "value1");
            _client.Put(key2, "value2");
            _client.Put(key3, "value3");

            var rangeResponse = _client.GetRange(prefix);
            var rangeValues = _client.GetRangeVal(prefix);

            // Assert
            Assert.NotNull(rangeResponse);
            Assert.Equal(3, rangeResponse.Kvs.Count);

            Assert.NotNull(rangeValues);
            Assert.Equal(3, rangeValues.Count);
            Assert.Equal("value1", rangeValues[key1]);
            Assert.Equal("value2", rangeValues[key2]);
            Assert.Equal("value3", rangeValues[key3]);
        }
        finally
        {
            // Cleanup
            _client.DeleteRange(prefix);
        }
    }

    [Fact]
    public void DeleteRange_ShouldRemoveMultipleKeys()
    {
        // Arrange
        var prefix = $"test-prefix-{Guid.NewGuid()}";
        var key1 = $"{prefix}/key1";
        var key2 = $"{prefix}/key2";
        var key3 = $"{prefix}/key3";

        // Act
        _client.Put(key1, "value1");
        _client.Put(key2, "value2");
        _client.Put(key3, "value3");

        var beforeDelete = _client.GetRange(prefix);
        var deleteResponse = _client.DeleteRange(prefix);
        var afterDelete = _client.GetRange(prefix);

        // Assert
        Assert.NotNull(beforeDelete);
        Assert.Equal(3, beforeDelete.Kvs.Count);

        Assert.NotNull(deleteResponse);
        Assert.Equal(3, deleteResponse.Deleted);

        Assert.NotNull(afterDelete);
        Assert.Empty(afterDelete.Kvs);
    }
}