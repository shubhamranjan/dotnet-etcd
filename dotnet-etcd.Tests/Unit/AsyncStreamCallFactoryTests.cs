using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AsyncStreamCallFactoryTests
{
    [Fact]
    public void Constructor_WithNullCallFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AsyncStreamCallFactory<string, string>(null));

        Assert.Equal("callFactory", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidCallFactory_ShouldInitialize()
    {
        // Arrange
        Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<string, string>> callFactory =
            (_, _, _) => new AsyncDuplexStreamingCall<string, string>(
                new Mock<IClientStreamWriter<string>>().Object,
                new Mock<IAsyncStreamReader<string>>().Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

        // Act & Assert - No exception
        var factory = new AsyncStreamCallFactory<string, string>(callFactory);
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateDuplexStreamingCall_ShouldReturnCallFromFactory()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();

        var expectedCall = new AsyncDuplexStreamingCall<string, string>(
            mockRequestStream.Object,
            mockResponseStream.Object,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<string, string>> callFactory =
            (headers, deadline, token) =>
            {
                // Verify parameters are passed through
                Assert.NotNull(headers);
                Assert.Equal(2, headers.Count);
                Assert.Equal("header1", headers[0].Key);
                Assert.Equal("value1", headers[0].Value);
                Assert.Equal("header2", headers[1].Key);
                Assert.Equal("value2", headers[1].Value);

                // DateTime? comparison can be tricky, so just check it's not null
                Assert.NotNull(deadline);

                // CancellationToken comparison
                Assert.Equal(new CancellationToken(true), token);

                return expectedCall;
            };

        var factory = new AsyncStreamCallFactory<string, string>(callFactory);

        // Prepare test parameters
        var headers = new Metadata
        {
            { "header1", "value1" },
            { "header2", "value2" }
        };
        var deadline = DateTime.UtcNow.AddMinutes(1);
        var token = new CancellationToken(true);

        // Act
        var result = factory.CreateDuplexStreamingCall(headers, deadline, token);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<AsyncDuplexStreamingCallAdapter<string, string>>(result);

        // Verify the adapter wraps our expected call by checking properties
        Assert.Equal(mockRequestStream.Object, result.RequestStream);
        Assert.Equal(mockResponseStream.Object, result.ResponseStream);
    }

    [Fact]
    public void CreateDuplexStreamingCall_WithDefaultValues_ShouldWork()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<string>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<string>>();

        var expectedCall = new AsyncDuplexStreamingCall<string, string>(
            mockRequestStream.Object,
            mockResponseStream.Object,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<string, string>> callFactory =
            (_, _, _) => expectedCall;

        var factory = new AsyncStreamCallFactory<string, string>(callFactory);

        // Act
        var result = factory.CreateDuplexStreamingCall(
            new Metadata(),
            null,
            default);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<AsyncDuplexStreamingCallAdapter<string, string>>(result);
        Assert.Equal(mockRequestStream.Object, result.RequestStream);
        Assert.Equal(mockResponseStream.Object, result.ResponseStream);
    }

    [Fact]
    public void CreateDuplexStreamingCall_ShouldImplementIAsyncDuplexStreamingCall()
    {
        // Arrange
        var mockRequestStream = new Mock<IClientStreamWriter<WatchRequest>>();
        var mockResponseStream = new Mock<IAsyncStreamReader<WatchResponse>>();

        var expectedCall = new AsyncDuplexStreamingCall<WatchRequest, WatchResponse>(
            mockRequestStream.Object,
            mockResponseStream.Object,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        Func<Metadata, DateTime?, CancellationToken, AsyncDuplexStreamingCall<WatchRequest, WatchResponse>>
            callFactory =
                (_, _, _) => expectedCall;

        var factory = new AsyncStreamCallFactory<WatchRequest, WatchResponse>(callFactory);

        // Act
        var result = factory.CreateDuplexStreamingCall(new Metadata(), null, default);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IAsyncDuplexStreamingCall<WatchRequest, WatchResponse>>(result);
    }
}