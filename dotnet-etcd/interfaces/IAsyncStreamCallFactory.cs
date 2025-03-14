// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Grpc.Core;

namespace dotnet_etcd.interfaces;

/// <summary>
///     Interface for creating async streaming calls
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IAsyncStreamCallFactory<TRequest, TResponse>
{
    /// <summary>
    ///     Creates an asynchronous duplex streaming call
    /// </summary>
    /// <param name="headers">The initial metadata to send with the call</param>
    /// <param name="deadline">An optional deadline for the call</param>
    /// <param name="cancellationToken">An optional token for canceling the call</param>
    /// <returns>A duplex streaming call adapter</returns>
    IAsyncDuplexStreamingCall<TRequest, TResponse> CreateDuplexStreamingCall(
        Metadata headers,
        DateTime? deadline,
        CancellationToken cancellationToken);
}
