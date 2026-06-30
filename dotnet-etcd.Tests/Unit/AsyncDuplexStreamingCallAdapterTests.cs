using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AsyncDuplexStreamingCallAdapterTests
{
    private static AsyncDuplexStreamingCall<WatchRequest, WatchResponse> CreateUnderlyingCall(
        IClientStreamWriter<WatchRequest> requestStream,
        IAsyncStreamReader<WatchResponse> responseStream,
        Task<Metadata> headers,
        Func<Status> getStatus,
        Func<Metadata> getTrailers,
        Action disposeAction) =>
        new(requestStream, responseStream, headers, getStatus, getTrailers, disposeAction);

    [Fact]
    public void Constructor_WithNullCall_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(null));
    }

    [Fact]
    public void RequestStream_ShouldReturnUnderlyingRequestStream()
    {
        // Arrange
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act & Assert
        Assert.Same(requestStream, adapter.RequestStream);
    }

    [Fact]
    public void ResponseStream_ShouldReturnUnderlyingResponseStream()
    {
        // Arrange
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act & Assert
        Assert.Same(responseStream, adapter.ResponseStream);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldReturnUnderlyingResponseHeaders()
    {
        // Arrange
        var expectedHeaders = new Metadata { { "k", "v" } };
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(expectedHeaders), () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act
        var headers = await adapter.GetHeadersAsync();

        // Assert
        Assert.Same(expectedHeaders, headers);
    }

    [Fact]
    public void GetStatus_ShouldReturnUnderlyingStatus()
    {
        // Arrange
        var expectedStatus = new Status(StatusCode.OK, "all good");
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(new Metadata()), () => expectedStatus, () => new Metadata(), () => { });
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act
        var status = adapter.GetStatus();

        // Assert
        Assert.Equal(expectedStatus.StatusCode, status.StatusCode);
        Assert.Equal(expectedStatus.Detail, status.Detail);
    }

    [Fact]
    public void GetTrailers_ShouldReturnUnderlyingTrailers()
    {
        // Arrange
        var expectedTrailers = new Metadata { { "trailer", "1" } };
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => expectedTrailers, () => { });
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act
        var trailers = adapter.GetTrailers();

        // Assert
        Assert.Same(expectedTrailers, trailers);
    }

    [Fact]
    public void Dispose_ShouldInvokeUnderlyingDisposeAction()
    {
        // Arrange
        var disposed = false;
        var requestStream = new Mock<IClientStreamWriter<WatchRequest>>().Object;
        var responseStream = new Mock<IAsyncStreamReader<WatchResponse>>().Object;
        var call = CreateUnderlyingCall(requestStream, responseStream,
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(),
            () => disposed = true);
        var adapter = new AsyncDuplexStreamingCallAdapter<WatchRequest, WatchResponse>(call);

        // Act
        adapter.Dispose();

        // Assert
        Assert.True(disposed);
    }
}
