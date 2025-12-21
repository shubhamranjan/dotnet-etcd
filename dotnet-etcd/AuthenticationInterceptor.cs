#nullable enable

using System;
using System.Threading.Tasks;
using Etcdserverpb;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace dotnet_etcd;

/// <summary>
///     Interceptor that automatically adds authentication tokens to all gRPC calls
/// </summary>
internal class AuthenticationInterceptor : Interceptor
{
    private const string AuthorizationHeader = "authorization";

    private readonly Func<string?> _getToken;

    public AuthenticationInterceptor(Func<string?> getToken)
    {
        _getToken = getToken ?? throw new ArgumentNullException(nameof(getToken));
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = AddAuthToken(context.Options.Headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = AddAuthToken(context.Options.Headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(newContext);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = AddAuthToken(context.Options.Headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = AddAuthToken(context.Options.Headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(newContext);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = AddAuthToken(context.Options.Headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method, context.Host, newOptions);

        return continuation(request, newContext);
    }

    private Metadata AddAuthToken(Metadata? headers)
    {
        // Create new metadata or use existing
        var newHeaders = headers == null ? new Metadata() : new Metadata();
        
        // Preserve all existing headers (except Authorization, which we'll replace if needed)
        if (headers != null)
        {
            foreach (var entry in headers)
            {
                // Skip authorization header - we'll add it if needed
                if (!string.Equals(entry.Key, AuthorizationHeader, StringComparison.OrdinalIgnoreCase))
                {
                    newHeaders.Add(entry);
                }
            }
        }

        // Get token from provider (this is now just a simple field read, no method calls)
        var token = _getToken?.Invoke();
        
        // Add the authorization header with the token (only if it's not empty or whitespace)
        if (!string.IsNullOrWhiteSpace(token))
        {
            newHeaders.Add(AuthorizationHeader, token);
        }
        
        return newHeaders;
    }
}
