using System.Text;
using Authpb;
using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthClientUnitTests
{
    [Fact]
    public void AuthEnable_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        mockAuthClient
            .Setup(x => x.AuthEnable(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthEnableResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        client.AuthEnable(new AuthEnableRequest());

        // Assert
        mockAuthClient.Verify(x => x.AuthEnable(
            It.IsAny<AuthEnableRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task AuthEnableAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var expectedResponse = new AuthEnableResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockAuthClient
            .Setup(x => x.AuthEnableAsync(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        await client.AuthEnableAsync(new AuthEnableRequest());

        // Assert
        mockAuthClient.Verify(x => x.AuthEnableAsync(
            It.IsAny<AuthEnableRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void AuthDisable_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        mockAuthClient
            .Setup(x => x.AuthDisable(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthDisableResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        client.AuthDisable(new AuthDisableRequest());

        // Assert
        mockAuthClient.Verify(x => x.AuthDisable(
            It.IsAny<AuthDisableRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Authenticate_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var username = "user";
        var password = "password";

        mockAuthClient
            .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthenticateResponse { Token = "token" });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthenticateRequest
        {
            Name = username,
            Password = password
        };
        var response = client.Authenticate(request);

        // Assert
        mockAuthClient.Verify(x => x.Authenticate(
            It.Is<AuthenticateRequest>(r => r.Name == username && r.Password == password),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("token", response.Token);
    }

    [Fact]
    public void UserAdd_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var username = "user";
        var password = "password";

        mockAuthClient
            .Setup(x => x.UserAdd(It.IsAny<AuthUserAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthUserAddResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthUserAddRequest
        {
            Name = username,
            Password = password
        };
        client.UserAdd(request);

        // Assert
        mockAuthClient.Verify(x => x.UserAdd(
            It.Is<AuthUserAddRequest>(r => r.Name == username && r.Password == password),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void UserDelete_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var username = "user";

        mockAuthClient
            .Setup(x => x.UserDelete(It.IsAny<AuthUserDeleteRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthUserDeleteResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthUserDeleteRequest { Name = username };
        client.UserDelete(request);

        // Assert
        mockAuthClient.Verify(x => x.UserDelete(
            It.Is<AuthUserDeleteRequest>(r => r.Name == username),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task UserDeleteAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var username = "user";
        var expectedResponse = new AuthUserDeleteResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockAuthClient
            .Setup(x => x.UserDeleteAsync(It.IsAny<AuthUserDeleteRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthUserDeleteRequest { Name = username };
        await client.UserDeleteAsync(request);

        // Assert
        mockAuthClient.Verify(x => x.UserDeleteAsync(
            It.Is<AuthUserDeleteRequest>(r => r.Name == username),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void UserList_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        mockAuthClient
            .Setup(x => x.UserList(It.IsAny<AuthUserListRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthUserListResponse { Users = { "user1", "user2" } });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var response = client.UserList(new AuthUserListRequest());

        // Assert
        mockAuthClient.Verify(x => x.UserList(
            It.IsAny<AuthUserListRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(2, response.Users.Count);
        Assert.Contains("user1", response.Users);
        Assert.Contains("user2", response.Users);
    }

    [Fact]
    public void RoleAdd_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var roleName = "admin";

        mockAuthClient
            .Setup(x => x.RoleAdd(It.IsAny<AuthRoleAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthRoleAddResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthRoleAddRequest { Name = roleName };
        client.RoleAdd(request);

        // Assert
        mockAuthClient.Verify(x => x.RoleAdd(
            It.Is<AuthRoleAddRequest>(r => r.Name == roleName),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void AuthEnable_WithCustomMetadata_ShouldPassMetadata()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var customMetadata = new Metadata { { "key", "value" } };

        mockAuthClient
            .Setup(x => x.AuthEnable(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AuthEnableResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        client.AuthEnable(new AuthEnableRequest(), customMetadata);

        // Assert
        mockAuthClient.Verify(x => x.AuthEnable(
            It.IsAny<AuthEnableRequest>(),
            It.Is<Metadata>(m => m.Get("key") != null && m.Get("key")!.Value == "value"),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task AuthEnable_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        using var cts = new CancellationTokenSource();

        mockAuthClient
            .Setup(x => x.AuthEnableAsync(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns<AuthEnableRequest, Metadata, DateTime?, CancellationToken>((_, _, _, _) =>
            {
                var tcs = new TaskCompletionSource<AuthEnableResponse>();
                tcs.SetException(new OperationCanceledException());
                return new AsyncUnaryCall<AuthEnableResponse>(tcs.Task, null!, null!, null!, null!);
            });

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            client.AuthEnableAsync(new AuthEnableRequest(), cancellationToken: cts.Token));
    }

    [Fact]
    public void RoleGrantPermission_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();
        var roleName = "admin";
        var key = "key";
        var rangeEnd = "rangeEnd";

        mockAuthClient
            .Setup(x => x.RoleGrantPermission(It.IsAny<AuthRoleGrantPermissionRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new AuthRoleGrantPermissionResponse());

        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthRoleGrantPermissionRequest
        {
            Name = roleName,
            Perm = new Permission
            {
                Key = ByteString.CopyFrom(Encoding.UTF8.GetBytes(key)),
                RangeEnd = ByteString.CopyFrom(Encoding.UTF8.GetBytes(rangeEnd))
            }
        };
        client.RoleGrantPermission(request);

        // Assert
        mockAuthClient.Verify(x => x.RoleGrantPermission(
            It.Is<AuthRoleGrantPermissionRequest>(r =>
                r.Name == roleName &&
                Encoding.UTF8.GetString(r.Perm.Key.ToArray()) == key),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}