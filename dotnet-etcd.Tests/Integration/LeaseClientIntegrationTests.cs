using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
[Trait("Category", "Integration")]
public class LeaseClientIntegrationTests : IDisposable
{
    private readonly EtcdClient _client;

    public LeaseClientIntegrationTests(EtcdClusterFixture fixture) =>
        _client = new EtcdClient(fixture.ConnectionString,
            configureChannelOptions: options => { options.Credentials = ChannelCredentials.Insecure; });

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task LeaseLeases_ShouldListGrantedLease()
    {
        var lease = await _client.LeaseGrantAsync(new LeaseGrantRequest { TTL = 30 });
        try
        {
            var listed = _client.LeaseLeases();
            Assert.Contains(listed.Leases, l => l.ID == lease.ID);

            var listedAsync = await _client.LeaseLeasesAsync(new LeaseLeasesRequest());
            Assert.Contains(listedAsync.Leases, l => l.ID == lease.ID);
        }
        finally
        {
            await _client.LeaseRevokeAsync(new LeaseRevokeRequest { ID = lease.ID });
        }

        var afterRevoke = await _client.LeaseLeasesAsync();
        Assert.DoesNotContain(afterRevoke.Leases, l => l.ID == lease.ID);
    }
}
