using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class MaintenanceClientIntegrationTests : IDisposable
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;

    public MaintenanceClientIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    [Fact]
    public void Status_ShouldReturnServerStatus()
    {
        // Act
        var response = _client.Status(new StatusRequest());

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Version);
        Assert.True(response.DbSize >= 0);
    }

    [Fact]
    public async Task StatusAsync_ShouldReturnServerStatus()
    {
        // Act
        var response = await _client.StatusAsync(new StatusRequest());

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Version);
        Assert.True(response.DbSize >= 0);
    }

    [Fact]
    public void Hash_ShouldReturnHash()
    {
        // Act
        var response = _client.Hash(new HashRequest());

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Hash > 0);
    }

    [Fact]
    public async Task HashAsync_ShouldReturnHash()
    {
        // Act
        var response = await _client.HashAsync(new HashRequest());

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Hash > 0);
    }

    [Fact]
    public void HashKV_ShouldReturnHashKV()
    {
        // Arrange - Add some test keys to ensure there's data to hash
        var testPrefix = $"hash_test_{Guid.NewGuid()}";
        var keysToCleanup = new List<string>();

        try
        {
            // Add multiple keys with the same prefix
            for (var i = 1; i <= 5; i++)
            {
                var key = $"{testPrefix}/key{i}";
                _client.Put(key, $"value{i}");
                keysToCleanup.Add(key);
            }

            // Get the current revision
            var statusResponse = _client.Status(new StatusRequest());
            var currentRevision = statusResponse.Header.Revision;

            // Act - Calculate hash at the current revision
            var hashResponse1 = _client.HashKV(new HashKVRequest { Revision = currentRevision });

            // Calculate hash again at the same revision - should be the same
            var hashResponse2 = _client.HashKV(new HashKVRequest { Revision = currentRevision });

            // Assert
            Assert.NotNull(hashResponse1);
            Assert.NotNull(hashResponse2);
            Assert.True(hashResponse1.Hash > 0, "Hash value should be greater than 0");
            Assert.Equal(hashResponse1.Hash, hashResponse2.Hash);

            // Add another key and verify hash changes
            var newKey = $"{testPrefix}/key6";
            _client.Put(newKey, "value6");
            keysToCleanup.Add(newKey);

            // Get the new revision
            statusResponse = _client.Status(new StatusRequest());
            var newRevision = statusResponse.Header.Revision;

            // Calculate hash at the new revision
            var hashResponse3 = _client.HashKV(new HashKVRequest { Revision = newRevision });

            // Hash should be different after adding a key
            Assert.NotEqual(hashResponse1.Hash, hashResponse3.Hash);
        }
        finally
        {
            // Cleanup - delete all test keys individually
            foreach (var key in keysToCleanup) _client.Delete(key);
        }
    }

    [Fact]
    public async Task HashKVAsync_ShouldReturnHashKV()
    {
        // Arrange - Add some test keys to ensure there's data to hash
        var testPrefix = $"hash_async_test_{Guid.NewGuid()}";
        var keysToCleanup = new List<string>();

        try
        {
            // Add multiple keys with the same prefix
            for (var i = 1; i <= 5; i++)
            {
                var key = $"{testPrefix}/key{i}";
                await _client.PutAsync(key, $"value{i}");
                keysToCleanup.Add(key);
            }

            // Get the current revision
            var statusResponse = await _client.StatusAsync(new StatusRequest());
            var currentRevision = statusResponse.Header.Revision;

            // Act - Calculate hash at the current revision
            var hashResponse1 = await _client.HashKVAsync(new HashKVRequest { Revision = currentRevision });

            // Calculate hash again at the same revision - should be the same
            var hashResponse2 = await _client.HashKVAsync(new HashKVRequest { Revision = currentRevision });

            // Assert
            Assert.NotNull(hashResponse1);
            Assert.NotNull(hashResponse2);
            Assert.True(hashResponse1.Hash > 0, "Hash value should be greater than 0");
            Assert.Equal(hashResponse1.Hash, hashResponse2.Hash);

            // Add another key and verify hash changes
            var newKey = $"{testPrefix}/key6";
            await _client.PutAsync(newKey, "value6");
            keysToCleanup.Add(newKey);

            // Get the new revision
            statusResponse = await _client.StatusAsync(new StatusRequest());
            var newRevision = statusResponse.Header.Revision;

            // Calculate hash at the new revision
            var hashResponse3 = await _client.HashKVAsync(new HashKVRequest { Revision = newRevision });

            // Hash should be different after adding a key
            Assert.NotEqual(hashResponse1.Hash, hashResponse3.Hash);
        }
        finally
        {
            // Cleanup - delete all test keys individually
            foreach (var key in keysToCleanup) await _client.DeleteAsync(key);
        }
    }

    [Fact(Skip = "This test is potentially destructive and should be run manually")]
    public void Alarm_ShouldManageAlarms()
    {
        // Arrange
        var getRequest = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.None
        };

        // Act - Get current alarms
        var getResponse = _client.Alarm(getRequest);

        // Assert
        Assert.NotNull(getResponse);

        // Note: We don't actually set or deactivate alarms in automated tests
        // as this could affect the etcd server's operation
    }

    [Fact(Skip = "This test is potentially destructive and should be run manually")]
    public async Task AlarmAsync_ShouldManageAlarms()
    {
        // Arrange
        var getRequest = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.None
        };

        // Act - Get current alarms
        var getResponse = await _client.AlarmAsync(getRequest);

        // Assert
        Assert.NotNull(getResponse);

        // Note: We don't actually set or deactivate alarms in automated tests
        // as this could affect the etcd server's operation
    }

    [Fact(Skip = "This test is potentially destructive and should be run manually")]
    public void Defragment_ShouldDefragmentDatabase()
    {
        // Act
        var response = _client.Defragment(new DefragmentRequest());

        // Assert
        Assert.NotNull(response);

        // Note: Defragmentation is a potentially long-running operation
        // and should be run manually in production environments
    }

    [Fact(Skip = "This test is potentially destructive and should be run manually")]
    public async Task DefragmentAsync_ShouldDefragmentDatabase()
    {
        // Act
        var response = await _client.DefragmentAsync(new DefragmentRequest());

        // Assert
        Assert.NotNull(response);

        // Note: Defragmentation is a potentially long-running operation
        // and should be run manually in production environments
    }
}