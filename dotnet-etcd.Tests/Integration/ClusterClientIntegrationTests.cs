using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;

namespace dotnet_etcd.Tests.Integration;

[Collection("EtcdCluster")]
public class ClusterClientIntegrationTests(EtcdClusterFixture fixture) : IDisposable
{
    private readonly EtcdClient _client = new(fixture.ConnectionString);
    private readonly string _memberPeerUrl = "http://localhost:2380";

    public void Dispose()
    {
        _client?.Dispose();
    }

    [Fact]
    public async Task MemberLifecycle_ShouldWorkCorrectly()
    {
        // Add Member
        var addRequest = new MemberAddRequest
        {
            PeerURLs = { _memberPeerUrl }
        };
        var addResponse = await _client.MemberAddAsync(addRequest);
        Assert.NotNull(addResponse);
        Assert.NotEqual(0UL, addResponse.Member.ID);

        // List Members
        var listRequest = new MemberListRequest();
        var listResponse = await _client.MemberListAsync(listRequest);
        Assert.NotNull(listResponse);
        Assert.Contains(listResponse.Members, m => m.PeerURLs.Contains(_memberPeerUrl));

        // Update Member
        var updateRequest = new MemberUpdateRequest
        {
            ID = addResponse.Member.ID,
            PeerURLs = { "http://localhost:2381" }
        };
        var updateResponse = await _client.MemberUpdateAsync(updateRequest);
        Assert.NotNull(updateResponse);

        // Remove Member
        var removeRequest = new MemberRemoveRequest
        {
            ID = addResponse.Member.ID
        };
        var removeResponse = await _client.MemberRemoveAsync(removeRequest);
        Assert.NotNull(removeResponse);

        // Verify Removal
        listResponse = await _client.MemberListAsync(listRequest);
        Assert.DoesNotContain(listResponse.Members, m => m.ID == addResponse.Member.ID);
    }

    [Fact]
    public async Task MemberList_ShouldReturnExistingMembers()
    {
        // Arrange
        var request = new MemberListRequest();

        // Act
        var response = await _client.MemberListAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Members);
    }
}