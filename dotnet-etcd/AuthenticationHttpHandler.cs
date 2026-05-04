#nullable enable

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet_etcd;

/// <summary>
///     DelegatingHandler that attaches an etcd auth token to outgoing requests. Fetches
///     the token via the supplied provider, caches it for the configured duration, and
///     bypasses the Authenticate RPC itself so the token-fetch call doesn't recurse.
/// </summary>
internal sealed class AuthenticationHttpHandler : DelegatingHandler
{
    private const string AuthorizationHeader = "authorization";
    private const string AuthenticatePath = "/etcdserverpb.Auth/Authenticate";

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Func<
        CancellationToken,
        Task<(string token, TimeSpan cacheDuration)?>
    > _tokenProvider;

    private CachedToken? _cachedToken;

    public AuthenticationHttpHandler(
        Func<CancellationToken, Task<(string token, TimeSpan cacheDuration)?>> tokenProvider,
        HttpMessageHandler innerHandler
    )
        : base(innerHandler)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <summary>
    ///     Clears any cached token so the next request re-fetches via the token provider.
    ///     Call this after credentials change so in-flight cached tokens don't outlive the change.
    /// </summary>
    public void InvalidateToken()
    {
        _semaphore.Wait();
        try
        {
            _cachedToken = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // If the request is an authorization request, we do nothing to prevent deadlocking
        if (request.RequestUri?.AbsolutePath == AuthenticatePath)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        string? token = await GetValidTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(AuthorizationHeader, token);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> GetValidTokenAsync(CancellationToken cancellationToken)
    {
        var current = _cachedToken;
        if (current is not null && DateTime.UtcNow < current.ExpiresAtUtc)
            return current.Token;

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            current = _cachedToken;
            if (current is not null && DateTime.UtcNow < current.ExpiresAtUtc)
                return current.Token;

            (string token, TimeSpan cacheDuration)? result = await _tokenProvider(cancellationToken)
                .ConfigureAwait(false);

            if (!result.HasValue)
            {
                // Client needs no authentication, cache null token value until the next invalidated call
                _cachedToken = new CachedToken(null, DateTime.MaxValue);
                return null;
            }

            _cachedToken = new CachedToken(
                result.Value.token,
                DateTime.UtcNow + result.Value.cacheDuration
            );
            return result.Value.token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphore.Dispose();
        }

        base.Dispose(disposing);
    }

    private record CachedToken(string? Token, DateTime ExpiresAtUtc);
}
