using Grpc.Core;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     Factory class for creating AsyncServerStreamingCall instances for testing
/// </summary>
public static class AsyncStreamingCallFactory
{
    /// <summary>
    ///     Creates an AsyncServerStreamingCall that returns the specified responses
    /// </summary>
    /// <typeparam name="T">The type of the response</typeparam>
    /// <param name="responses">The responses to return</param>
    /// <returns>An AsyncServerStreamingCall that returns the specified responses</returns>
    public static AsyncServerStreamingCall<T> Create<T>(IEnumerable<T> responses)
    {
        var reader = new TestAsyncStreamReader<T>(responses);

        return new AsyncServerStreamingCall<T>(
            reader,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}