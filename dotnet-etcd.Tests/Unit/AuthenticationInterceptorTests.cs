using Grpc.Core;
using Grpc.Core.Interceptors;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthenticationInterceptorTests
{
    private const string AuthorizationHeader = "authorization";
    private const string TestToken = "test-token-12345";

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidTokenProvider_ShouldSucceed()
    {
        // Arrange & Act
        var interceptor = new AuthenticationInterceptor(() => TestToken);

        // Assert
        Assert.NotNull(interceptor);
    }

    [Fact]
    public void Constructor_WithNullTokenProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthenticationInterceptor(default(Func<string>)!));
    }

    #endregion

    #region BlockingUnaryCall Tests

    [Fact]
    public void BlockingUnaryCall_WithToken_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public void BlockingUnaryCall_WithoutToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => null);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    [Fact]
    public void BlockingUnaryCall_WithEmptyToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => "");
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    [Fact]
    public void BlockingUnaryCall_WithExistingHeaders_ShouldPreserveNonAuthHeaders()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var originalHeaders = new Metadata
        {
            { "custom-header-1", "value1" },
            { "custom-header-2", "value2" }
        };

        var context = CreateContext<string, string>(originalHeaders);

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == "custom-header-1" && h.Value == "value1");
        Assert.Contains(capturedHeaders, h => h.Key == "custom-header-2" && h.Value == "value2");
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public void BlockingUnaryCall_WithExistingAuthHeader_ShouldReplaceWithNewToken()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var originalHeaders = new Metadata
        {
            { "authorization", "old-token" },
            { "custom-header", "value" }
        };

        var context = CreateContext<string, string>(originalHeaders);

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        // Should have exactly one authorization header with the new token
        var authHeaders = capturedHeaders.Where(h => h.Key.Equals(AuthorizationHeader, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.Single(authHeaders);
        Assert.Equal(TestToken, authHeaders[0].Value);
        // Should preserve other headers
        Assert.Contains(capturedHeaders, h => h.Key == "custom-header" && h.Value == "value");
    }

    [Fact]
    public void BlockingUnaryCall_WithNullHeaders_ShouldCreateNewMetadata()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>(null);

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    #endregion

    #region AsyncUnaryCall Tests

    [Fact]
    public async Task AsyncUnaryCall_WithToken_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return new AsyncUnaryCall<string>(
                Task.FromResult("response"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        var result = interceptor.AsyncUnaryCall("request", context, continuation);
        await result.ResponseAsync;

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public async Task AsyncUnaryCall_WithoutToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => null);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return new AsyncUnaryCall<string>(
                Task.FromResult("response"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        var result = interceptor.AsyncUnaryCall("request", context, continuation);
        await result.ResponseAsync;

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    [Fact]
    public async Task AsyncUnaryCall_WithExistingHeaders_ShouldPreserveNonAuthHeaders()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return new AsyncUnaryCall<string>(
                Task.FromResult("response"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var originalHeaders = new Metadata
        {
            { "custom-header", "custom-value" }
        };

        var context = CreateContext<string, string>(originalHeaders);

        // Act
        var result = interceptor.AsyncUnaryCall("request", context, continuation);
        await result.ResponseAsync;

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == "custom-header" && h.Value == "custom-value");
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    #endregion

    #region AsyncClientStreamingCall Tests

    [Fact]
    public void AsyncClientStreamingCall_WithToken_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncClientStreamingCallContinuation<string, string> continuation = ctx =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockRequestStream = new Mock<IClientStreamWriter<string>>();
            return new AsyncClientStreamingCall<string, string>(
                mockRequestStream.Object,
                Task.FromResult("response"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncClientStreamingCall(context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public void AsyncClientStreamingCall_WithoutToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => null);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncClientStreamingCallContinuation<string, string> continuation = ctx =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockRequestStream = new Mock<IClientStreamWriter<string>>();
            return new AsyncClientStreamingCall<string, string>(
                mockRequestStream.Object,
                Task.FromResult("response"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncClientStreamingCall(context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    #endregion

    #region AsyncServerStreamingCall Tests

    [Fact]
    public void AsyncServerStreamingCall_WithToken_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncServerStreamingCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
            return new AsyncServerStreamingCall<string>(
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncServerStreamingCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public void AsyncServerStreamingCall_WithoutToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => null);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncServerStreamingCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
            return new AsyncServerStreamingCall<string>(
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncServerStreamingCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    #endregion

    #region AsyncDuplexStreamingCall Tests

    [Fact]
    public void AsyncDuplexStreamingCall_WithToken_ShouldAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncDuplexStreamingCallContinuation<string, string> continuation = ctx =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockRequestStream = new Mock<IClientStreamWriter<string>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
            return new AsyncDuplexStreamingCall<string, string>(
                mockRequestStream.Object,
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncDuplexStreamingCall(context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders, h => h.Key == AuthorizationHeader && h.Value == TestToken);
    }

    [Fact]
    public void AsyncDuplexStreamingCall_WithoutToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => null);
        Metadata? capturedHeaders = null;

        Interceptor.AsyncDuplexStreamingCallContinuation<string, string> continuation = ctx =>
        {
            capturedHeaders = ctx.Options.Headers;
            var mockRequestStream = new Mock<IClientStreamWriter<string>>();
            var mockResponseStream = new Mock<IAsyncStreamReader<string>>();
            return new AsyncDuplexStreamingCall<string, string>(
                mockRequestStream.Object,
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.AsyncDuplexStreamingCall(context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    #endregion

    #region Token Provider Behavior Tests

    [Fact]
    public void Interceptor_ShouldCallTokenProviderOnEachRequest()
    {
        // Arrange
        var callCount = 0;
        var interceptor = new AuthenticationInterceptor(() =>
        {
            callCount++;
            return $"token-{callCount}";
        });

        Metadata? firstHeaders = null;
        Metadata? secondHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            if (firstHeaders == null)
                firstHeaders = ctx.Options.Headers;
            else
                secondHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.BlockingUnaryCall("request1", context, continuation);
        interceptor.BlockingUnaryCall("request2", context, continuation);

        // Assert
        Assert.Equal(2, callCount);
        Assert.NotNull(firstHeaders);
        Assert.NotNull(secondHeaders);
        Assert.Contains(firstHeaders, h => h.Key == AuthorizationHeader && h.Value == "token-1");
        Assert.Contains(secondHeaders, h => h.Key == AuthorizationHeader && h.Value == "token-2");
    }

    [Fact]
    public void Interceptor_WithWhitespaceToken_ShouldNotAddAuthorizationHeader()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => "   ");
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var context = CreateContext<string, string>();

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        Assert.DoesNotContain(capturedHeaders, h => h.Key == AuthorizationHeader);
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void Interceptor_ShouldReplaceAuthHeaderRegardlessOfCase()
    {
        // Arrange
        var interceptor = new AuthenticationInterceptor(() => TestToken);
        Metadata? capturedHeaders = null;

        Interceptor.BlockingUnaryCallContinuation<string, string> continuation = (req, ctx) =>
        {
            capturedHeaders = ctx.Options.Headers;
            return "response";
        };

        var originalHeaders = new Metadata
        {
            { "Authorization", "old-token-1" },  // Different case
            { "AUTHORIZATION", "old-token-2" },  // All caps
            { "custom-header", "value" }
        };

        var context = CreateContext<string, string>(originalHeaders);

        // Act
        interceptor.BlockingUnaryCall("request", context, continuation);

        // Assert
        Assert.NotNull(capturedHeaders);
        // Should have exactly one authorization header
        var authHeaders = capturedHeaders.Where(h => h.Key.Equals(AuthorizationHeader, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.Single(authHeaders);
        Assert.Equal(TestToken, authHeaders[0].Value);
        // Should preserve other headers
        Assert.Contains(capturedHeaders, h => h.Key == "custom-header");
    }

    #endregion

    #region Helper Methods

    private static ClientInterceptorContext<TRequest, TResponse> CreateContext<TRequest, TResponse>(Metadata? headers = null)
        where TRequest : class
        where TResponse : class
    {
        var method = new Method<TRequest, TResponse>(
            MethodType.Unary,
            "TestService",
            "TestMethod",
            Marshallers.Create<TRequest>(_ => Array.Empty<byte>(), _ => default!),
            Marshallers.Create<TResponse>(_ => Array.Empty<byte>(), _ => default!));

        var callOptions = new CallOptions(headers: headers);

        return new ClientInterceptorContext<TRequest, TResponse>(
            method,
            "localhost",
            callOptions);
    }

    #endregion
}
