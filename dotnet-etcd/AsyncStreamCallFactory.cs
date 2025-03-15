// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using dotnet_etcd.interfaces;
using Grpc.Core;

namespace dotnet_etcd;

/// <summary>
///     Factory for creating async streaming calls to etcd
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AsyncStreamCallFactory<TRequest, TResponse> : IAsyncStreamCallFactory<TRequest, TResponse>
{
    private readonly Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<TRequest, TResponse>>
        _callFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncStreamCallFactory{TRequest, TResponse}" /> class.
    /// </summary>
    /// <param name="callFactory">Factory function to create the gRPC call</param>
    /// <exception cref="ArgumentNullException">Thrown if callFactory is null</exception>
    public AsyncStreamCallFactory(
        Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<TRequest, TResponse>> callFactory) =>
        _callFactory = callFactory ?? throw new ArgumentNullException(nameof(callFactory));

    /// <inheritdoc />
    public IAsyncDuplexStreamingCall<TRequest, TResponse> CreateDuplexStreamingCall(
        Metadata headers,
        DateTime? deadline,
        CancellationToken cancellationToken)
    {
        AsyncDuplexStreamingCall<TRequest, TResponse> call = _callFactory(headers, deadline, cancellationToken);
        return new AsyncDuplexStreamingCallAdapter<TRequest, TResponse>(call);
    }
}
