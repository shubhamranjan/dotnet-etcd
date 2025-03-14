using dotnet_etcd.interfaces;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AsyncDuplexStreamingCallAdapterTests
{
    [Fact]
    public void Constructor_WithNullCall_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AsyncDuplexStreamingCallAdapter<string, string>(null!));
        Assert.Equal("call", exception.ParamName);
    }

    [Fact]
    public void IAsyncDuplexStreamingCall_Implementation_ShouldBeCorrect()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
        var expectedHeaders = new Metadata();
        var expectedStatus = Status.DefaultSuccess;
        var expectedTrailers = new Metadata();

        var testCall = new TestAsyncDuplexStreamingCall<string, string>
        {
            RequestStream = mockRequestStream.Object,
            ResponseStream = mockResponseStream.Object,
            HeadersTask = Task.FromResult(expectedHeaders),
            Status = expectedStatus,
            Trailers = expectedTrailers
        };

        // Act & Assert
        Assert.Equal(mockRequestStream.Object, testCall.RequestStream);
        Assert.Equal(mockResponseStream.Object, testCall.ResponseStream);
        Assert.Equal(expectedHeaders, testCall.GetHeadersAsync().Result);
        Assert.Equal(expectedStatus, testCall.GetStatus());
        Assert.Equal(expectedTrailers, testCall.GetTrailers());

        testCall.Dispose();
        Assert.True(testCall.WasDisposed);
    }

    [Fact]
    public void RequestStream_ShouldReturnUnderlyingRequestStream()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();

        var call = CreateTestCall(mockRequestStream.Object, mockResponseStream.Object);
        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        var result = adapter.RequestStream;

        // Assert
        Assert.Same(mockRequestStream.Object, result);
    }

    [Fact]
    public void ResponseStream_ShouldReturnUnderlyingResponseStream()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();

        var call = CreateTestCall(mockRequestStream.Object, mockResponseStream.Object);
        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        var result = adapter.ResponseStream;

        // Assert
        Assert.Same(mockResponseStream.Object, result);
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldReturnUnderlyingHeaders()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
        var expectedHeaders = new Metadata();

        var call = CreateTestCall(
            mockRequestStream.Object,
            mockResponseStream.Object,
            Task.FromResult(expectedHeaders));

        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        var result = await adapter.GetHeadersAsync();

        // Assert
        Assert.Same(expectedHeaders, result);
    }

    [Fact]
    public void GetStatus_ShouldReturnUnderlyingStatus()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
        var expectedStatus = new Status(StatusCode.OK, "Success");

        var call = CreateTestCall(
            mockRequestStream.Object,
            mockResponseStream.Object,
            status: expectedStatus);

        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        var result = adapter.GetStatus();

        // Assert
        Assert.Equal(expectedStatus, result);
    }

    [Fact]
    public void GetTrailers_ShouldReturnUnderlyingTrailers()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
        var expectedTrailers = new Metadata();

        var call = CreateTestCall(
            mockRequestStream.Object,
            mockResponseStream.Object,
            trailers: expectedTrailers);

        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        var result = adapter.GetTrailers();

        // Assert
        Assert.Same(expectedTrailers, result);
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingCall()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
        var wasDisposed = false;

        var call = CreateTestCall(
            mockRequestStream.Object,
            mockResponseStream.Object,
            disposeAction: () => wasDisposed = true);

        var adapter = new AsyncDuplexStreamingCallAdapter<string, string>(call);

        // Act
        adapter.Dispose();

        // Assert
        Assert.True(wasDisposed);
    }

    // Helper method to create a test AsyncDuplexStreamingCall
    private AsyncDuplexStreamingCall<TRequest, TResponse> CreateTestCall<TRequest, TResponse>(
        IClientStreamWriter<TRequest> requestStream,
        IAsyncStreamReader<TResponse> responseStream,
        Task<Metadata>? headersTask = null,
        Status? status = null,
        Metadata? trailers = null,
        Action? disposeAction = null)
    {
        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            requestStream,
            responseStream,
            headersTask ?? Task.FromResult(new Metadata()),
            () => status ?? Status.DefaultSuccess,
            () => trailers ?? new Metadata(),
            disposeAction ?? (() => { }));
    }

    /// <summary>
    ///     Test implementation of IAsyncDuplexStreamingCall for testing
    /// </summary>
    private class TestAsyncDuplexStreamingCall<TRequest, TResponse> : IAsyncDuplexStreamingCall<TRequest, TResponse>
    {
        public Task<Metadata> HeadersTask { get; set; } = Task.FromResult(new Metadata());
        public Status Status { get; set; } = Status.DefaultSuccess;
        public Metadata Trailers { get; set; } = new();
        public bool WasDisposed { get; private set; }
        public IClientStreamWriter<TRequest> RequestStream { get; set; } = null!;
        public IAsyncStreamReader<TResponse> ResponseStream { get; set; } = null!;

        public Task<Metadata> GetHeadersAsync()
        {
            return HeadersTask;
        }

        public Status GetStatus()
        {
            return Status;
        }

        public Metadata GetTrailers()
        {
            return Trailers;
        }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }
}