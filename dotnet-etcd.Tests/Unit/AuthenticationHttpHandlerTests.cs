using System.Net;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthenticationHttpHandlerTests
{
    private const string AuthorizationHeader = "authorization";
    private const string TestToken = "test-token-12345";
    private const string AuthenticatePath = "/etcdserverpb.Auth/Authenticate";
    private const string NonAuthPath = "/etcdserverpb.KV/Range";

    #region Constructor

    [Fact]
    public void Constructor_WithNullTokenProvider_Throws()
    {
        using var inner = new CapturingHandler();
        Assert.Throws<ArgumentNullException>(() =>
            new AuthenticationHttpHandler(null!, TimeSpan.FromMinutes(4), inner)
        );
    }

    #endregion

    #region Header injection

    [Fact]
    public async Task SendAsync_AddsAuthorizationHeader_WhenTokenProvided()
    {
        using var inner = new CapturingHandler();
        using var invoker = BuildInvoker(inner, _ => Task.FromResult<string?>(TestToken));

        await invoker.SendAsync(Request(NonAuthPath), default);

        var request = Assert.Single(inner.Requests);
        Assert.True(request.Headers.TryGetValues(AuthorizationHeader, out var values));
        Assert.Equal(TestToken, Assert.Single(values));
    }

    [Fact]
    public async Task SendAsync_DoesNotAddHeader_WhenTokenIsNull()
    {
        using var inner = new CapturingHandler();
        using var invoker = BuildInvoker(inner, _ => Task.FromResult<string?>(null));

        await invoker.SendAsync(Request(NonAuthPath), default);

        Assert.False(inner.Requests.Single().Headers.Contains(AuthorizationHeader));
    }

    [Fact]
    public async Task SendAsync_DoesNotAddHeader_WhenTokenIsWhitespace()
    {
        using var inner = new CapturingHandler();
        using var invoker = BuildInvoker(inner, _ => Task.FromResult<string?>("   "));

        await invoker.SendAsync(Request(NonAuthPath), default);

        Assert.False(inner.Requests.Single().Headers.Contains(AuthorizationHeader));
    }

    #endregion

    #region Authenticate bypass

    [Fact]
    public async Task SendAsync_ToAuthenticatePath_DoesNotInvokeTokenProvider()
    {
        using var inner = new CapturingHandler();
        int providerCalls = 0;
        using var invoker = BuildInvoker(
            inner,
            _ =>
            {
                Interlocked.Increment(ref providerCalls);
                return Task.FromResult<string?>(TestToken);
            }
        );

        await invoker.SendAsync(Request(AuthenticatePath), default);

        Assert.Equal(0, providerCalls);
        Assert.False(inner.Requests.Single().Headers.Contains(AuthorizationHeader));
    }

    #endregion

    #region Caching

    [Fact]
    public async Task SendAsync_CachesToken_SecondRequestDoesNotRefetch()
    {
        using var inner = new CapturingHandler();
        int providerCalls = 0;
        using var invoker = BuildInvoker(
            inner,
            _ =>
            {
                Interlocked.Increment(ref providerCalls);
                return Task.FromResult<string?>(TestToken);
            }
        );

        await invoker.SendAsync(Request(NonAuthPath), default);
        await invoker.SendAsync(Request(NonAuthPath), default);

        Assert.Equal(1, providerCalls);
        Assert.Equal(2, inner.Requests.Count);
        Assert.All(
            inner.Requests,
            r => Assert.Equal(TestToken, r.Headers.GetValues(AuthorizationHeader).Single())
        );
    }

    [Fact]
    public async Task SendAsync_RefetchesToken_AfterExpiry()
    {
        using var inner = new CapturingHandler();
        int providerCalls = 0;
        using var invoker = BuildInvoker(
            inner,
            _ =>
            {
                int n = Interlocked.Increment(ref providerCalls);
                return Task.FromResult<string?>($"token-{n}");
            },
            cacheDuration: TimeSpan.Zero
        );

        await invoker.SendAsync(Request(NonAuthPath), default);
        await invoker.SendAsync(Request(NonAuthPath), default);

        Assert.Equal(2, providerCalls);
        Assert.Equal("token-1", inner.Requests[0].Headers.GetValues(AuthorizationHeader).Single());
        Assert.Equal("token-2", inner.Requests[1].Headers.GetValues(AuthorizationHeader).Single());
    }

    [Fact]
    public async Task SendAsync_ConcurrentRequests_FetchTokenOnce()
    {
        using var inner = new CapturingHandler();
        int providerCalls = 0;
        var gate = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var invoker = BuildInvoker(
            inner,
            async _ =>
            {
                Interlocked.Increment(ref providerCalls);
                return await gate.Task.ConfigureAwait(false);
            }
        );

        Task send1 = invoker.SendAsync(Request(NonAuthPath), default);
        Task send2 = invoker.SendAsync(Request(NonAuthPath), default);
        Task send3 = invoker.SendAsync(Request(NonAuthPath), default);

        gate.SetResult(TestToken);
        await Task.WhenAll(send1, send2, send3);

        Assert.Equal(1, providerCalls);
        Assert.Equal(3, inner.Requests.Count);
        Assert.All(
            inner.Requests,
            r => Assert.Equal(TestToken, r.Headers.GetValues(AuthorizationHeader).Single())
        );
    }

    #endregion

    #region Error and cancellation

    [Fact]
    public async Task SendAsync_WhenTokenProviderThrows_ExceptionPropagates()
    {
        using var inner = new CapturingHandler();
        using var invoker = BuildInvoker(
            inner,
            _ => Task.FromException<string?>(new InvalidOperationException("boom"))
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invoker.SendAsync(Request(NonAuthPath), default)
        );
        Assert.Equal("boom", ex.Message);
        Assert.Empty(inner.Requests);
    }

    [Fact]
    public async Task SendAsync_PassesCancellationTokenToProvider()
    {
        using var inner = new CapturingHandler();
        using var cts = new CancellationTokenSource();
        CancellationToken? observed = null;
        using var invoker = BuildInvoker(
            inner,
            ct =>
            {
                observed = ct;
                return Task.FromResult<string?>(TestToken);
            }
        );

        await invoker.SendAsync(Request(NonAuthPath), cts.Token);

        Assert.True(observed.HasValue);
        Assert.Equal(cts.Token, observed!.Value);
    }

    #endregion

    #region Helpers

    private static HttpMessageInvoker BuildInvoker(
        HttpMessageHandler inner,
        Func<CancellationToken, Task<string?>> tokenProvider,
        TimeSpan? cacheDuration = null
    ) =>
        new(
            new AuthenticationHttpHandler(
                tokenProvider,
                cacheDuration ?? TimeSpan.FromMinutes(4),
                inner
            ),
            disposeHandler: true
        );

    private static HttpRequestMessage Request(string path) =>
        new(HttpMethod.Post, new Uri("http://localhost" + path));

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    #endregion
}
