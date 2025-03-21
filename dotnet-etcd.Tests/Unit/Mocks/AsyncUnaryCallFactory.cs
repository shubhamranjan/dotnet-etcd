using Grpc.Core;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     Factory for creating AsyncUnaryCall instances for testing
/// </summary>
public static class AsyncUnaryCallFactory
{
    /// <summary>
    ///     Creates an AsyncUnaryCall that returns the specified response
    /// </summary>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    /// <param name="response">The response to return</param>
    /// <param name="responseHeaders">Optional response headers</param>
    /// <param name="status">Optional status</param>
    /// <param name="trailers">Optional trailers</param>
    /// <returns>An AsyncUnaryCall that returns the specified response</returns>
    public static AsyncUnaryCall<TResponse> Create<TResponse>(
        TResponse response,
        Metadata? responseHeaders = null,
        Status? status = null,
        Metadata? trailers = null)
    {
        responseHeaders ??= new Metadata();
        status ??= Status.DefaultSuccess;
        trailers ??= new Metadata();

        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(responseHeaders),
            () => status.Value,
            () => trailers,
            () => { });
    }

    /// <summary>
    ///     Creates an AsyncUnaryCall that throws an exception
    /// </summary>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    /// <param name="exception">The exception to throw</param>
    /// <returns>An AsyncUnaryCall that throws the specified exception</returns>
    public static AsyncUnaryCall<TResponse> CreateWithException<TResponse>(Exception exception)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromException<TResponse>(exception),
            Task.FromResult(new Metadata()),
            () => Status.DefaultCancelled,
            () => new Metadata(),
            () => { });
    }

    /// <summary>
    ///     Creates an AsyncUnaryCall that returns the specified response after the specified delay
    /// </summary>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    /// <param name="response">The response to return</param>
    /// <param name="delayMs">The delay in milliseconds</param>
    /// <returns>An AsyncUnaryCall that returns the specified response after the specified delay</returns>
    public static AsyncUnaryCall<TResponse> CreateWithDelay<TResponse>(TResponse response, int delayMs)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.Delay(delayMs).ContinueWith(_ => response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}