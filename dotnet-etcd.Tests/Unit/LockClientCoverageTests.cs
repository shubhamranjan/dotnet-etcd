using dotnet_etcd.Tests.Infrastructure;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Lockpb;
using Lock = V3Lockpb.Lock;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Coverage-focused unit tests for the Lock client unary methods.
///     Every public UNARY Lock method + overload (string and request based) is exercised
///     in both sync and async form.
/// </summary>
[Trait("Category", "Unit")]
public class LockClientCoverageTests
{
    // ---------------------------------------------------------------------
    // Lock (sync) - string overload
    // ---------------------------------------------------------------------

    [Fact]
    public void Lock_WithName_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var name = "my-lock";
        var expectedKey = ByteString.CopyFromUtf8("my-lock-key");
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse { Key = expectedKey });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var result = client.Lock(name);

        Assert.Equal(expectedKey, result.Key);
        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Lock_WithNameAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var name = "my-lock";
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        client.Lock(name, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Name.Equals(ByteString.CopyFromUtf8(name))),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Lock (sync) - request overload
    // ---------------------------------------------------------------------

    [Fact]
    public void Lock_WithRequest_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var expectedKey = ByteString.CopyFromUtf8("req-lock-key");
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse { Key = expectedKey });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var request = new LockRequest { Name = ByteString.CopyFromUtf8("req-lock"), Lease = 55 };
        var result = client.Lock(request);

        Assert.Equal(expectedKey, result.Key);
        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Lease == 55),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Lock_WithRequestAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        mockLockClient
            .Setup(x => x.Lock(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new LockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var request = new LockRequest { Name = ByteString.CopyFromUtf8("req-lock"), Lease = 66 };
        client.Lock(request, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.Lock(
            It.Is<LockRequest>(r => r.Lease == 66),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LockAsync - string overload
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LockAsync_WithName_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var name = "my-lock";
        var expectedKey = ByteString.CopyFromUtf8("my-lock-key");
        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LockResponse { Key = expectedKey }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var result = await client.LockAsync(name);

        Assert.Equal(expectedKey, result.Key);
        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Name.Equals(ByteString.CopyFromUtf8(name))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LockAsync_WithNameAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var name = "my-lock";
        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        await client.LockAsync(name, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Name.Equals(ByteString.CopyFromUtf8(name))),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // LockAsync - request overload
    // ---------------------------------------------------------------------

    [Fact]
    public async Task LockAsync_WithRequest_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var expectedKey = ByteString.CopyFromUtf8("req-lock-key");
        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LockResponse { Key = expectedKey }));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var request = new LockRequest { Name = ByteString.CopyFromUtf8("req-lock"), Lease = 77 };
        var result = await client.LockAsync(request);

        Assert.Equal(expectedKey, result.Key);
        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Lease == 77),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LockAsync_WithRequestAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        mockLockClient
            .Setup(x => x.LockAsync(It.IsAny<LockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new LockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        var request = new LockRequest { Name = ByteString.CopyFromUtf8("req-lock"), Lease = 88 };
        await client.LockAsync(request, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.LockAsync(
            It.Is<LockRequest>(r => r.Lease == 88),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Unlock (sync) - string overload
    // ---------------------------------------------------------------------

    [Fact]
    public void Unlock_WithKey_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = "my-lock-key";
        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new UnlockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        client.Unlock(key);

        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Unlock_WithKeyAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = "my-lock-key";
        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new UnlockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        client.Unlock(key, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // Unlock (sync) - request overload
    // ---------------------------------------------------------------------

    [Fact]
    public void Unlock_WithRequest_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = ByteString.CopyFromUtf8("req-unlock-key");
        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new UnlockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        client.Unlock(new UnlockRequest { Key = key });

        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.Equals(key)),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Unlock_WithRequestAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = ByteString.CopyFromUtf8("req-unlock-key");
        mockLockClient
            .Setup(x => x.Unlock(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new UnlockResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        client.Unlock(new UnlockRequest { Key = key }, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.Unlock(
            It.Is<UnlockRequest>(r => r.Key.Equals(key)),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // UnlockAsync - string overload
    // ---------------------------------------------------------------------

    [Fact]
    public async Task UnlockAsync_WithKey_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = "my-lock-key";
        mockLockClient
            .Setup(x => x.UnlockAsync(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new UnlockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        await client.UnlockAsync(key);

        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_WithKeyAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = "my-lock-key";
        mockLockClient
            .Setup(x => x.UnlockAsync(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new UnlockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        await client.UnlockAsync(key, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.Equals(ByteString.CopyFromUtf8(key))),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------------------------------------------------------------------
    // UnlockAsync - request overload
    // ---------------------------------------------------------------------

    [Fact]
    public async Task UnlockAsync_WithRequest_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = ByteString.CopyFromUtf8("req-unlock-key");
        mockLockClient
            .Setup(x => x.UnlockAsync(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new UnlockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        await client.UnlockAsync(new UnlockRequest { Key = key });

        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.Equals(key)),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlockAsync_WithRequestAndAllParameters_ShouldCallGrpcClient()
    {
        var mockLockClient = new Mock<Lock.LockClient>();
        var key = ByteString.CopyFromUtf8("req-unlock-key");
        mockLockClient
            .Setup(x => x.UnlockAsync(It.IsAny<UnlockRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new UnlockResponse()));

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockLockClient.Object, "_lockClient");

        var headers = new Metadata { { "key", "value" } };
        var deadline = DateTime.UtcNow.AddSeconds(10);
        await client.UnlockAsync(new UnlockRequest { Key = key }, headers, deadline, CancellationToken.None);

        mockLockClient.Verify(x => x.UnlockAsync(
            It.Is<UnlockRequest>(r => r.Key.Equals(key)),
            headers, deadline, It.IsAny<CancellationToken>()), Times.Once);
    }
}
