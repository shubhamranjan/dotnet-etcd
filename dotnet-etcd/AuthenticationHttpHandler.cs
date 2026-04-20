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
    private readonly Func<CancellationToken, Task<string?>> _tokenProvider;
    private readonly TimeSpan _tokenCacheDuration;

    private string? _cachedToken;
    private DateTime _tokenExpiresAtUtc;

    public AuthenticationHttpHandler(
        Func<CancellationToken, Task<string?>> tokenProvider,
        TimeSpan tokenCacheDuration,
        HttpMessageHandler innerHandler
    )
        : base(innerHandler)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _tokenCacheDuration = tokenCacheDuration;
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
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAtUtc)
            return _cachedToken;

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAtUtc)
                return _cachedToken;

            _cachedToken = await _tokenProvider(cancellationToken).ConfigureAwait(false);
            _tokenExpiresAtUtc = DateTime.UtcNow + _tokenCacheDuration;
            return _cachedToken;
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
}
