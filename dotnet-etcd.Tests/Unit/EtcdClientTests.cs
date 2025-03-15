using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientTests
{
    private readonly EtcdClient _client;
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<KV.KVClient> _mockKvClient;
    private readonly Mock<IWatchManager> _mockWatchManager;

    public EtcdClientTests()
    {
        // Set up connection and client mocks
        _mockConnection = MockConnection.Create();
        _mockKvClient = _mockConnection.SetupKVClient();
        _mockWatchManager = MockAsyncCalls.CreateWatchManager();

        // Create client with mocked dependencies
        _client = new EtcdClient(_mockConnection.Object, _mockWatchManager.Object);
    }

    #region Helper Methods

    [Fact]
    public void GetConnection_ShouldReturnConnection()
    {
        // Arrange
        var client = new EtcdClient(_mockConnection.Object);

        // Act
        var result = client.GetConnection();

        // Assert
        Assert.Same(_mockConnection.Object, result);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullCallInvoker_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient(null));

        // Assert
        Assert.Equal("callInvoker", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidCallInvoker_ShouldCreateClient()
    {
        // Arrange
        var mockCallInvoker = new Mock<CallInvoker>();

        // Act
        using var client = new EtcdClient(mockCallInvoker.Object);

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithNullConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient((string)null));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient(""));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespaceConnectionString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient("  "));
        Assert.Equal("connectionString", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullConnection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EtcdClient((IConnection)null));
        Assert.Equal("connection", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithConnection_ShouldInitializeClient()
    {
        // Arrange
        var mockConnection = MockConnection.Create();

        // Act
        using var client = new EtcdClient(mockConnection.Object);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(mockConnection.Object, client.GetConnection());
    }

    [Fact]
    public void Constructor_WithConnectionAndWatchManager_ShouldInitializeClient()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockWatchManager = MockAsyncCalls.CreateWatchManager();

        // Act
        using var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(mockConnection.Object, client.GetConnection());
        Assert.Equal(mockWatchManager.Object, client.GetWatchManager());
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ShouldDisposeWatchManager()
    {
        // Arrange
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(_mockConnection.Object, mockWatchManager.Object);

        // Act
        client.Dispose();

        // Assert
        mockWatchManager.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldOnlyDisposeOnce()
    {
        // Arrange
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(_mockConnection.Object, mockWatchManager.Object);

        // Act
        client.Dispose();
        client.Dispose(); // Call dispose a second time

        // Assert
        mockWatchManager.Verify(x => x.Dispose(), Times.Once);
    }

    #endregion

    #region Watch Tests

    [Fact]
    public void CancelWatch_ShouldCallWatchManagerCancelWatch()
    {
        // Act
        _client.CancelWatch(123);

        // Assert
        _mockWatchManager.Verify(m => m.CancelWatch(123), Times.Once);
    }

    [Fact]
    public void CancelWatch_WithMockSetup_ShouldCallWatchManagerCancelWatch()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockWatchManager = new Mock<IWatchManager>();
        mockWatchManager.Setup(x => x.CancelWatch(It.IsAny<long>()));

        var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

        // Act
        client.CancelWatch(123);

        // Assert
        mockWatchManager.Verify(x => x.CancelWatch(123), Times.Once);
    }

    [Fact]
    public void CancelWatchArray_ShouldCallWatchManagerCancelWatchForEachId()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockWatchManager = new Mock<IWatchManager>();
        mockWatchManager.Setup(x => x.CancelWatch(It.IsAny<long>()));

        var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

        // Act
        client.CancelWatch(new long[] { 123, 456, 789 });

        // Assert
        mockWatchManager.Verify(x => x.CancelWatch(123), Times.Once);
        mockWatchManager.Verify(x => x.CancelWatch(456), Times.Once);
        mockWatchManager.Verify(x => x.CancelWatch(789), Times.Once);
    }

    [Fact]
    public void GetWatchManager_ShouldReturnWatchManager()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockWatchManager = new Mock<IWatchManager>();
        var client = new EtcdClient(mockConnection.Object, mockWatchManager.Object);

        // Act
        var result = client.GetWatchManager();

        // Assert
        Assert.Same(mockWatchManager.Object, result);
    }

    #endregion

    #region KV Operations Tests

    [Fact]
    public void Put_ShouldCallKVClient()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";

        var expectedRequest = new PutRequest
        {
            Key = ByteString.CopyFromUtf8(key),
            Value = ByteString.CopyFromUtf8(value)
        };

        var expectedResponse = new PutResponse();

        _mockKvClient
            .Setup(c => c.Put(
                It.Is<PutRequest>(r =>
                    r.Key.Equals(expectedRequest.Key) &&
                    r.Value.Equals(expectedRequest.Value)),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Act
        var response = _client.Put(key, value);

        // Assert
        Assert.Equal(expectedResponse, response);
        _mockKvClient.Verify(c => c.Put(
                It.Is<PutRequest>(r =>
                    r.Key.Equals(expectedRequest.Key) &&
                    r.Value.Equals(expectedRequest.Value)),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void GetVal_ShouldReturnValue()
    {
        // Arrange
        var key = "testKey";
        var expectedValue = "testValue";

        var rangeResponse = new RangeResponse
        {
            Count = 1, // Important to set the count
            Kvs =
            {
                new KeyValue
                {
                    Key = ByteString.CopyFromUtf8(key),
                    Value = ByteString.CopyFromUtf8(expectedValue)
                }
            }
        };

        _mockKvClient
            .Setup(c => c.Range(
                It.Is<RangeRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(rangeResponse);

        // Act
        var result = _client.GetVal(key);

        // Assert
        Assert.Equal(expectedValue, result);
        _mockKvClient.Verify(c => c.Range(
                It.Is<RangeRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void GetVal_WhenKeyNotFound_ShouldReturnEmptyString()
    {
        // Arrange
        var key = "nonExistentKey";

        var rangeResponse = new RangeResponse();

        _mockKvClient
            .Setup(c => c.Range(
                It.Is<RangeRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(rangeResponse);

        // Act
        var result = _client.GetVal(key);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Delete_ShouldCallKVClient()
    {
        // Arrange
        var key = "testKey";

        var expectedRequest = new DeleteRangeRequest
        {
            Key = ByteString.CopyFromUtf8(key)
        };

        var expectedResponse = new DeleteRangeResponse
        {
            Deleted = 1
        };

        _mockKvClient
            .Setup(c => c.DeleteRange(
                It.Is<DeleteRangeRequest>(r => r.Key.Equals(expectedRequest.Key)),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Act
        var response = _client.Delete(key);

        // Assert
        Assert.Equal(expectedResponse, response);
        Assert.Equal(1, response.Deleted);
    }

    #endregion

    #region Auth Operations Tests

    [Fact]
    public void Authenticate_ShouldCallAuthClientAuthenticate()
    {
        // Arrange
        var request = new AuthenticateRequest { Name = "user", Password = "password" };
        var expectedResponse = new AuthenticateResponse { Token = "token123" };

        var mockAuthClient = _mockConnection.SetupAuthClient();
        mockAuthClient
            .Setup(x => x.Authenticate(request, null, null, default))
            .Returns(expectedResponse);

        // Act
        var response = _client.Authenticate(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.Authenticate(request, null, null, default), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldCallAuthClientAuthenticateAsync()
    {
        // Arrange
        var request = new AuthenticateRequest { Name = "user", Password = "password" };
        var expectedResponse = new AuthenticateResponse { Token = "token123" };

        var mockAuthClient = _mockConnection.SetupAuthClient();
        var asyncResponse = AsyncUnaryCallFactory.Create(expectedResponse);

        mockAuthClient
            .Setup(x => x.AuthenticateAsync(request, null, null, default))
            .Returns(asyncResponse);

        // Act
        var response = await _client.AuthenticateAsync(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.AuthenticateAsync(request, null, null, default), Times.Once);
    }

    [Fact]
    public void AuthEnable_ShouldCallAuthClientAuthEnable()
    {
        // Arrange
        var request = new AuthEnableRequest();
        var expectedResponse = new AuthEnableResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        mockAuthClient
            .Setup(x => x.AuthEnable(request, null, null, default))
            .Returns(expectedResponse);

        // Act
        var response = _client.AuthEnable(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.AuthEnable(request, null, null, default), Times.Once);
    }

    [Fact]
    public async Task AuthEnableAsync_ShouldCallAuthClientAuthEnableAsync()
    {
        // Arrange
        var request = new AuthEnableRequest();
        var expectedResponse = new AuthEnableResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        var asyncResponse = AsyncUnaryCallFactory.Create(expectedResponse);

        mockAuthClient
            .Setup(x => x.AuthEnableAsync(request, null, null, default))
            .Returns(asyncResponse);

        // Act
        var response = await _client.AuthEnableAsync(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.AuthEnableAsync(request, null, null, default), Times.Once);
    }

    [Fact]
    public void AuthDisable_ShouldCallAuthClientAuthDisable()
    {
        // Arrange
        var request = new AuthDisableRequest();
        var expectedResponse = new AuthDisableResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        mockAuthClient
            .Setup(x => x.AuthDisable(request, null, null, default))
            .Returns(expectedResponse);

        // Act
        var response = _client.AuthDisable(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.AuthDisable(request, null, null, default), Times.Once);
    }

    [Fact]
    public async Task AuthDisableAsync_ShouldCallAuthClientAuthDisableAsync()
    {
        // Arrange
        var request = new AuthDisableRequest();
        var expectedResponse = new AuthDisableResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        var asyncResponse = AsyncUnaryCallFactory.Create(expectedResponse);

        mockAuthClient
            .Setup(x => x.AuthDisableAsync(request, null, null, default))
            .Returns(asyncResponse);

        // Act
        var response = await _client.AuthDisableAsync(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.AuthDisableAsync(request, null, null, default), Times.Once);
    }

    [Fact]
    public void UserAdd_ShouldCallAuthClientUserAdd()
    {
        // Arrange
        var request = new AuthUserAddRequest { Name = "user", Password = "password" };
        var expectedResponse = new AuthUserAddResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        mockAuthClient
            .Setup(x => x.UserAdd(request, null, null, default))
            .Returns(expectedResponse);

        // Act
        var response = _client.UserAdd(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.UserAdd(request, null, null, default), Times.Once);
    }

    [Fact]
    public async Task UserAddAsync_ShouldCallAuthClientUserAddAsync()
    {
        // Arrange
        var request = new AuthUserAddRequest { Name = "user", Password = "password" };
        var expectedResponse = new AuthUserAddResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        var asyncResponse = AsyncUnaryCallFactory.Create(expectedResponse);

        mockAuthClient
            .Setup(x => x.UserAddAsync(request, null, null, default))
            .Returns(asyncResponse);

        // Act
        var response = await _client.UserAddAsync(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.UserAddAsync(request, null, null, default), Times.Once);
    }

    [Fact]
    public void UserGet_ShouldCallAuthClientUserGet()
    {
        // Arrange
        var request = new AuthUserGetRequest { Name = "user" };
        var expectedResponse = new AuthUserGetResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        mockAuthClient
            .Setup(x => x.UserGet(request, null, null, default))
            .Returns(expectedResponse);

        // Act
        var response = _client.UserGet(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.UserGet(request, null, null, default), Times.Once);
    }

    [Fact]
    public async Task UserGetAsync_ShouldCallAuthClientUserGetAsync()
    {
        // Arrange
        var request = new AuthUserGetRequest { Name = "user" };
        var expectedResponse = new AuthUserGetResponse();

        var mockAuthClient = _mockConnection.SetupAuthClient();
        var asyncResponse = AsyncUnaryCallFactory.Create(expectedResponse);

        mockAuthClient
            .Setup(x => x.UserGetAsync(request, null, null, default))
            .Returns(asyncResponse);

        // Act
        var response = await _client.UserGetAsync(request);

        // Assert
        Assert.Equal(expectedResponse, response);
        mockAuthClient.Verify(x => x.UserGetAsync(request, null, null, default), Times.Once);
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public void Transaction_ShouldCallKVClientTxn()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockKvClient = mockConnection.SetupKVClient();
        var testResponse = new TxnResponse();

        mockKvClient
            .Setup(x => x.Txn(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(testResponse);

        var client = new EtcdClient(mockConnection.Object);
        var request = new TxnRequest();

        // Act
        var result = client.Transaction(request);

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.Txn(
            request,
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task TransactionAsync_ShouldCallKVClientTxnAsync()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockKvClient = mockConnection.SetupKVClient();
        var testResponse = new TxnResponse();

        var asyncResponse = new AsyncUnaryCall<TxnResponse>(
            Task.FromResult(testResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.TxnAsync(It.IsAny<TxnRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        var client = new EtcdClient(mockConnection.Object);
        var request = new TxnRequest();

        // Act
        var result = await client.TransactionAsync(request);

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.TxnAsync(
            request,
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion

    #region Compact Tests

    [Fact]
    public void Compact_ShouldCallKVClientCompact()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockKvClient = mockConnection.SetupKVClient();
        var testResponse = new CompactionResponse();

        mockKvClient
            .Setup(x => x.Compact(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(testResponse);

        var client = new EtcdClient(mockConnection.Object);
        var request = new CompactionRequest { Revision = 100 };

        // Act
        var result = client.Compact(request);

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.Compact(
            request,
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CompactAsync_ShouldCallKVClientCompactAsync()
    {
        // Arrange
        var mockConnection = MockConnection.Create();
        var mockKvClient = mockConnection.SetupKVClient();
        var testResponse = new CompactionResponse();

        var asyncResponse = new AsyncUnaryCall<CompactionResponse>(
            Task.FromResult(testResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockKvClient
            .Setup(x => x.CompactAsync(It.IsAny<CompactionRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        var client = new EtcdClient(mockConnection.Object);
        var request = new CompactionRequest { Revision = 100 };

        // Act
        var result = await client.CompactAsync(request);

        // Assert
        Assert.Same(testResponse, result);
        mockKvClient.Verify(x => x.CompactAsync(
            request,
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion
}