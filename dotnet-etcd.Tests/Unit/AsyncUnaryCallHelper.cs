using Grpc.Core;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Helper class to create AsyncUnaryCall instances for testing
/// </summary>
public static class AsyncUnaryCallHelper
{
    /// <summary>
    ///     Creates an AsyncUnaryCall that returns the specified response
    /// </summary>
    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}