using dotnet_etcd.interfaces;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit.Mocks;

/// <summary>
///     Helper to create mock async calls for testing
/// </summary>
public static class MockAsyncCalls
{
    /// <summary>
    ///     Creates a mock for IAsyncDuplexStreamingCall
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <returns>A mock of IAsyncDuplexStreamingCall</returns>
    public static Mock<IAsyncDuplexStreamingCall<TRequest, TResponse>> CreateDuplexStreamingCall<TRequest, TResponse>()
    {
        var mockCall = new Mock<IAsyncDuplexStreamingCall<TRequest, TResponse>>();

        var mockRequestStream = new Mock<IClientStreamWriter<TRequest>>();
        mockCall.Setup(c => c.RequestStream).Returns(mockRequestStream.Object);

        var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
        mockCall.Setup(c => c.ResponseStream).Returns(mockResponseStream.Object);

        mockCall.Setup(c => c.GetHeadersAsync()).Returns(Task.FromResult(new Metadata()));
        mockCall.Setup(c => c.GetStatus()).Returns(Status.DefaultSuccess);
        mockCall.Setup(c => c.GetTrailers()).Returns(new Metadata());

        return mockCall;
    }

    /// <summary>
    ///     Creates a mock of IAsyncStreamCallFactory
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="mockCall">Optional mock call to return from the factory</param>
    /// <returns>A mock of IAsyncStreamCallFactory</returns>
    public static Mock<IAsyncStreamCallFactory<TRequest, TResponse>> CreateStreamCallFactory<TRequest, TResponse>(
        Mock<IAsyncDuplexStreamingCall<TRequest, TResponse>> mockCall = null)
    {
        var factory = new Mock<IAsyncStreamCallFactory<TRequest, TResponse>>();

        mockCall ??= CreateDuplexStreamingCall<TRequest, TResponse>();

        factory.Setup(f => f.CreateDuplexStreamingCall(
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(mockCall.Object);

        return factory;
    }

    /// <summary>
    ///     Creates a mock for IWatchManager
    /// </summary>
    /// <returns>A mock of IWatchManager</returns>
    public static Mock<IWatchManager> CreateWatchManager()
    {
        var mockWatchManager = new Mock<IWatchManager>();

        // Set up the watch methods to return a predictable watch ID
        mockWatchManager.Setup(w => w.Watch(
                It.IsAny<string>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(1L);

        mockWatchManager.Setup(w => w.WatchRange(
                It.IsAny<string>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(2L);

        mockWatchManager.Setup(w => w.Watch(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(3L);

        mockWatchManager.Setup(w => w.WatchRange(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(4L);

        // Set up the async watch methods
        mockWatchManager.Setup(w => w.WatchAsync(
                It.IsAny<string>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(5L);

        mockWatchManager.Setup(w => w.WatchRangeAsync(
                It.IsAny<string>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(6L);

        mockWatchManager.Setup(w => w.WatchAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(7L);

        mockWatchManager.Setup(w => w.WatchRangeAsync(
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<Action<WatchEvent>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(8L);

        return mockWatchManager;
    }
}