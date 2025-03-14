// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Grpc.Core;

namespace dotnet_etcd;

/// <summary>
///     Adapter for AsyncDuplexStreamingCall to implement IAsyncDuplexStreamingCall
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AsyncDuplexStreamingCallAdapter<TRequest, TResponse> : IAsyncDuplexStreamingCall<TRequest, TResponse>
{
    private readonly AsyncDuplexStreamingCall<TRequest, TResponse> _call;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncDuplexStreamingCallAdapter{TRequest, TResponse}" /> class.
    /// </summary>
    /// <param name="call">The underlying call to wrap</param>
    /// <exception cref="ArgumentNullException">Thrown if call is null</exception>
    public AsyncDuplexStreamingCallAdapter(AsyncDuplexStreamingCall<TRequest, TResponse> call) =>
        _call = call ?? throw new ArgumentNullException(nameof(call));

    /// <inheritdoc />
    public IClientStreamWriter<TRequest> RequestStream => _call.RequestStream;

    /// <inheritdoc />
    public IAsyncStreamReader<TResponse> ResponseStream => _call.ResponseStream;

    /// <inheritdoc />
    public Task<Metadata> GetHeadersAsync() => _call.ResponseHeadersAsync;

    /// <inheritdoc />
    public Status GetStatus() => _call.GetStatus();

    /// <inheritdoc />
    public Metadata GetTrailers() => _call.GetTrailers();

    /// <inheritdoc />
    public void Dispose() => _call.Dispose();
}
