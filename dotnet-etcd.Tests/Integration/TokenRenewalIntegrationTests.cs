using Etcdserverpb;
using Grpc.Core;

namespace dotnet_etcd.Tests.Integration;

/// <summary>
/// Verifies the client recovers after etcd expires its auth token server-side (issue #283).
/// Runs against the dedicated etcd-authttl node (port 2399, started with --auth-token-ttl=2),
/// whose short TTL lets the token expire within the test instead of after the 5-minute default.
/// </summary>
[Collection("AuthTests")]
[Trait("Category", "Integration")]
public class TokenRenewalIntegrationTests : IDisposable
{
    private const string Endpoint = "127.0.0.1:2399";
    private const string User = "renewuser";
    private const string Pass = "renewpass123";

    private readonly List<EtcdClient> _toDispose = new();

    public void Dispose()
    {
        TryDisableAuth();
        foreach (var client in _toDispose)
        {
            try
            {
                client.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        _toDispose.Clear();
    }

    [Fact]
    public async Task Operations_Recover_AfterServerSideTokenExpiry()
    {
        await EnableAuthWithUser();

        // Cache the token far longer than the server's 2s TTL. With this 1-hour client cache, the
        // client-side expiry can never explain a recovery within the test window — the only way an
        // operation can succeed again after the server evicts the token is the handler invalidating
        // on the Unauthenticated rejection and re-authenticating. This isolates the issue #283 fix.
        var client = new EtcdClient(
            Endpoint,
            User,
            Pass,
            tokenCacheDuration: TimeSpan.FromHours(1)
        );
        _toDispose.Add(client);

        // Authenticated operation succeeds and primes the cached token.
        await client.PutAsync("renew-key", "v1");
        Assert.Equal("v1", await client.GetValAsync("renew-key"));

        // Sit idle longer than the server-side TTL so etcd evicts the token. The client still holds
        // the now-stale token in its 1-hour cache.
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Operations must converge back to success: the first call replays the stale token and is
        // rejected (which purges the cache), and a follow-up re-auths with a fresh token. Retry to
        // absorb that single boundary rejection.
        await RetryUntilSucceedsAsync(
            () => client.PutAsync("renew-key", "v2"),
            timeout: TimeSpan.FromSeconds(15)
        );

        Assert.Equal("v2", await client.GetValAsync("renew-key"));
    }

    private static async Task RetryUntilSucceedsAsync(Func<Task> action, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        RpcException last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await action();
                return;
            }
            catch (RpcException ex)
            {
                last = ex;
                await Task.Delay(250);
            }
        }

        throw new Exception(
            $"Operation never recovered within {timeout}. Last error: {last?.Status}",
            last
        );
    }

    private async Task EnableAuthWithUser()
    {
        // Start from a clean slate in case a previous run left auth enabled.
        TryDisableAuth();

        // User/role creation and AuthEnable happen before auth is active, so an unauthenticated
        // admin client is sufficient here.
        using var admin = new EtcdClient(Endpoint);
        _toDispose.Add(admin);

        await IgnoreAlreadyExistsAsync(
            () => admin.UserAddAsync(new AuthUserAddRequest { Name = "root", Password = "root" })
        );
        await IgnoreAlreadyExistsAsync(
            () => admin.UserGrantRoleAsync(new AuthUserGrantRoleRequest { User = "root", Role = "root" })
        );
        await IgnoreAlreadyExistsAsync(
            () => admin.UserAddAsync(new AuthUserAddRequest { Name = User, Password = Pass })
        );
        await IgnoreAlreadyExistsAsync(
            () => admin.UserGrantRoleAsync(new AuthUserGrantRoleRequest { User = User, Role = "root" })
        );

        await admin.AuthEnableAsync(new AuthEnableRequest());
    }

    private void TryDisableAuth()
    {
        try
        {
            using var rootClient = new EtcdClient(Endpoint, "root", "root");
            rootClient.AuthDisableAsync(new AuthDisableRequest()).GetAwaiter().GetResult();
        }
        catch
        {
            // Auth already disabled, or root user not yet created — nothing to do.
        }
    }

    private static async Task IgnoreAlreadyExistsAsync(Func<Task> op)
    {
        try
        {
            await op();
        }
        catch (RpcException)
        {
            // User/role may already exist from a previous run.
        }
    }
}
