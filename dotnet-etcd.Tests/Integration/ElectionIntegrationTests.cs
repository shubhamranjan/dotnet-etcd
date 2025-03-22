using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using V3Electionpb;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class ElectionIntegrationTests : IDisposable
{
    private readonly EtcdClient _client;
    private readonly EtcdClusterFixture _fixture;
    private readonly long _leaseId;

    public ElectionIntegrationTests(EtcdClusterFixture fixture)
    {
        _fixture = fixture;
        Console.WriteLine($"Connecting to {_fixture.ClusterType} etcd cluster at {_fixture.ConnectionString}");

        _client = new EtcdClient(_fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });

        // Create a lease for election tests
        var leaseResponse = _client.LeaseGrant(new LeaseGrantRequest { TTL = 30 });
        _leaseId = leaseResponse.ID;
    }

    public void Dispose()
    {
        // Revoke the lease if it exists
        if (_leaseId > 0)
            try
            {
                _client.LeaseRevoke(new LeaseRevokeRequest { ID = _leaseId });
            }
            catch
            {
                // Ignore errors during cleanup
            }

        _client?.Dispose();
    }

    [Fact]
    public void Campaign_ShouldBecomeLeader()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";

        try
        {
            // Act
            var request = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            var response = _client.Campaign(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Leader);
            Assert.Equal(electionName, response.Leader.Name.ToStringUtf8());
            Assert.Equal(_leaseId, response.Leader.Lease);

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = response.Leader
            };
            _client.Resign(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public async Task CampaignAsync_ShouldBecomeLeader()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";

        try
        {
            // Act
            var request = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            var response = await _client.CampaignAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Leader);
            Assert.Equal(electionName, response.Leader.Name.ToStringUtf8());
            Assert.Equal(_leaseId, response.Leader.Lease);

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = response.Leader
            };
            await _client.ResignAsync(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public void Leader_ShouldReturnCurrentLeader()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";
        CampaignResponse campaignResponse = null;

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            campaignResponse = _client.Campaign(campaignRequest);

            // Act
            var response = _client.Leader(electionName);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Kv);
            Assert.StartsWith(electionName, response.Kv.Key.ToStringUtf8());
            Assert.Equal("test-value", response.Kv.Value.ToStringUtf8());

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            _client.Resign(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public async Task LeaderAsync_ShouldReturnCurrentLeader()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";
        CampaignResponse campaignResponse = null;

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            campaignResponse = await _client.CampaignAsync(campaignRequest);

            // Act
            var response = await _client.LeaderAsync(electionName);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Kv);
            Assert.StartsWith(electionName, response.Kv.Key.ToStringUtf8());
            Assert.Equal("test-value", response.Kv.Value.ToStringUtf8());

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            await _client.ResignAsync(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public void Proclaim_ShouldUpdateLeaderValue()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";
        CampaignResponse campaignResponse = null;

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            campaignResponse = _client.Campaign(campaignRequest);

            // Act
            var proclaimRequest = new ProclaimRequest
            {
                Leader = campaignResponse.Leader,
                Value = ByteString.CopyFromUtf8("updated-value")
            };
            var proclaimResponse = _client.Proclaim(proclaimRequest);

            // Verify the value was updated
            var leaderResponse = _client.Leader(electionName);

            // Assert
            Assert.NotNull(proclaimResponse);
            Assert.NotNull(leaderResponse);
            Assert.NotNull(leaderResponse.Kv);
            Assert.StartsWith(electionName, leaderResponse.Kv.Key.ToStringUtf8());
            Assert.Equal("updated-value", leaderResponse.Kv.Value.ToStringUtf8());

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            _client.Resign(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public async Task ProclaimAsync_ShouldUpdateLeaderValue()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";
        CampaignResponse campaignResponse = null;

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            campaignResponse = await _client.CampaignAsync(campaignRequest);

            // Act
            var proclaimRequest = new ProclaimRequest
            {
                Leader = campaignResponse.Leader,
                Value = ByteString.CopyFromUtf8("updated-value")
            };
            var proclaimResponse = await _client.ProclaimAsync(proclaimRequest);

            // Verify the value was updated
            var leaderResponse = await _client.LeaderAsync(electionName);

            // Assert
            Assert.NotNull(proclaimResponse);
            Assert.NotNull(leaderResponse);
            Assert.NotNull(leaderResponse.Kv);
            Assert.StartsWith(electionName, leaderResponse.Kv.Key.ToStringUtf8());
            Assert.Equal("updated-value", leaderResponse.Kv.Value.ToStringUtf8());

            // Cleanup - Resign leadership
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            await _client.ResignAsync(resignRequest);
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public void Resign_ShouldReleaseLeadership()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            var campaignResponse = _client.Campaign(campaignRequest);

            // Act
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            var resignResponse = _client.Resign(resignRequest);

            // Assert
            Assert.NotNull(resignResponse);

            // Verify leadership is released by trying to become leader again
            var newCampaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("new-value")
            };
            var newCampaignResponse = _client.Campaign(newCampaignRequest);
            Assert.NotNull(newCampaignResponse);
            Assert.NotNull(newCampaignResponse.Leader);

            // Cleanup
            _client.Resign(new ResignRequest { Leader = newCampaignResponse.Leader });
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }

    [Fact]
    public async Task ResignAsync_ShouldReleaseLeadership()
    {
        // Arrange
        var electionName = $"test-election-{Guid.NewGuid()}";

        try
        {
            // First become the leader
            var campaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("test-value")
            };
            var campaignResponse = await _client.CampaignAsync(campaignRequest);

            // Act
            var resignRequest = new ResignRequest
            {
                Leader = campaignResponse.Leader
            };
            var resignResponse = await _client.ResignAsync(resignRequest);

            // Assert
            Assert.NotNull(resignResponse);

            // Verify leadership is released by trying to become leader again
            var newCampaignRequest = new CampaignRequest
            {
                Name = ByteString.CopyFromUtf8(electionName),
                Lease = _leaseId,
                Value = ByteString.CopyFromUtf8("new-value")
            };
            var newCampaignResponse = await _client.CampaignAsync(newCampaignRequest);
            Assert.NotNull(newCampaignResponse);
            Assert.NotNull(newCampaignResponse.Leader);

            // Cleanup
            await _client.ResignAsync(new ResignRequest { Leader = newCampaignResponse.Leader });
        }
        catch (RpcException ex)
        {
            // If the test fails due to connection issues, skip it
            // This allows the tests to run even if etcd is not available
            if (ex.StatusCode == StatusCode.Unavailable) throw new SkipException("etcd server is not available");
            throw;
        }
    }
}

// Helper class for skipping tests