// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace dotnet_etcd.interfaces;

/// <summary>
///     Interface for async duplex streaming call
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IAsyncDuplexStreamingCall<TRequest, TResponse> : IDisposable
{
    /// <summary>
    ///     Gets the request stream for the duplex streaming call
    /// </summary>
    IClientStreamWriter<TRequest> RequestStream { get; }

    /// <summary>
    ///     Gets the response stream for the duplex streaming call
    /// </summary>
    IAsyncStreamReader<TResponse> ResponseStream { get; }

    /// <summary>
    ///     Gets the call headers
    /// </summary>
    /// <returns>Task containing response headers</returns>
    Task<Metadata> GetHeadersAsync();

    /// <summary>
    ///     Gets the call status
    /// </summary>
    /// <returns>The call status when the call has completed</returns>
    Status GetStatus();

    /// <summary>
    ///     Gets the call trailing metadata
    /// </summary>
    /// <returns>The call trailing metadata</returns>
    Metadata GetTrailers();
}
