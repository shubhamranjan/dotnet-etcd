using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthClientCoverageTests
{
    private static (EtcdClient client, Mock<Auth.AuthClient> mock) CreateClient()
    {
        var mock = new Mock<Auth.AuthClient>();
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_authClient");
        return (client, mock);
    }

    // ----- Authenticate -----

    [Fact]
    public void Authenticate_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthenticateResponse());

        client.Authenticate(new AuthenticateRequest { Name = "user", Password = "pass" });

        mock.Verify(x => x.Authenticate(
            It.Is<AuthenticateRequest>(r => r.Name == "user" && r.Password == "pass"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.AuthenticateAsync(It.IsAny<AuthenticateRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthenticateResponse()));

        await client.AuthenticateAsync(new AuthenticateRequest { Name = "user", Password = "pass" });

        mock.Verify(x => x.AuthenticateAsync(
            It.Is<AuthenticateRequest>(r => r.Name == "user" && r.Password == "pass"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- AuthEnable -----

    [Fact]
    public void AuthEnable_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.AuthEnable(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthEnableResponse());

        client.AuthEnable(new AuthEnableRequest(), new Metadata(), DateTime.UtcNow, CancellationToken.None);

        mock.Verify(x => x.AuthEnable(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthEnableAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.AuthEnableAsync(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthEnableResponse()));

        await client.AuthEnableAsync(new AuthEnableRequest(), new Metadata(), DateTime.UtcNow, CancellationToken.None);

        mock.Verify(x => x.AuthEnableAsync(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- AuthDisable -----

    [Fact]
    public void AuthDisable_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.AuthDisable(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthDisableResponse());

        client.AuthDisable(new AuthDisableRequest());

        mock.Verify(x => x.AuthDisable(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthDisableAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.AuthDisableAsync(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthDisableResponse()));

        await client.AuthDisableAsync(new AuthDisableRequest());

        mock.Verify(x => x.AuthDisableAsync(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserAdd -----

    [Fact]
    public void UserAdd_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserAdd(It.IsAny<AuthUserAddRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserAddResponse());

        client.UserAdd(new AuthUserAddRequest { Name = "user", Password = "pass" });

        mock.Verify(x => x.UserAdd(
            It.Is<AuthUserAddRequest>(r => r.Name == "user" && r.Password == "pass"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserAddAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserAddAsync(It.IsAny<AuthUserAddRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserAddResponse()));

        await client.UserAddAsync(new AuthUserAddRequest { Name = "user" });

        mock.Verify(x => x.UserAddAsync(
            It.Is<AuthUserAddRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserGet -----

    [Fact]
    public void UserGet_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserGet(It.IsAny<AuthUserGetRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserGetResponse());

        client.UserGet(new AuthUserGetRequest { Name = "user" });

        mock.Verify(x => x.UserGet(
            It.Is<AuthUserGetRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserGetAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserGetAsync(It.IsAny<AuthUserGetRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserGetResponse()));

        await client.UserGetAsync(new AuthUserGetRequest { Name = "user" });

        mock.Verify(x => x.UserGetAsync(
            It.Is<AuthUserGetRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserList -----

    [Fact]
    public void UserList_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserList(It.IsAny<AuthUserListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserListResponse());

        client.UserList(new AuthUserListRequest());

        mock.Verify(x => x.UserList(It.IsAny<AuthUserListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserListAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserListAsync(It.IsAny<AuthUserListRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserListResponse()));

        await client.UserListAsync(new AuthUserListRequest());

        mock.Verify(x => x.UserListAsync(It.IsAny<AuthUserListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserDelete -----

    [Fact]
    public void UserDelete_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserDelete(It.IsAny<AuthUserDeleteRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserDeleteResponse());

        client.UserDelete(new AuthUserDeleteRequest { Name = "user" });

        mock.Verify(x => x.UserDelete(
            It.Is<AuthUserDeleteRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserDeleteAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserDeleteAsync(It.IsAny<AuthUserDeleteRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserDeleteResponse()));

        await client.UserDeleteAsync(new AuthUserDeleteRequest { Name = "user" });

        mock.Verify(x => x.UserDeleteAsync(
            It.Is<AuthUserDeleteRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserChangePassword -----

    [Fact]
    public void UserChangePassword_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserChangePassword(It.IsAny<AuthUserChangePasswordRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserChangePasswordResponse());

        client.UserChangePassword(new AuthUserChangePasswordRequest { Name = "user", Password = "new" });

        mock.Verify(x => x.UserChangePassword(
            It.Is<AuthUserChangePasswordRequest>(r => r.Name == "user" && r.Password == "new"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserChangePasswordAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserChangePasswordAsync(It.IsAny<AuthUserChangePasswordRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserChangePasswordResponse()));

        await client.UserChangePasswordAsync(new AuthUserChangePasswordRequest { Name = "user" });

        mock.Verify(x => x.UserChangePasswordAsync(
            It.Is<AuthUserChangePasswordRequest>(r => r.Name == "user"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserGrantRole -----

    [Fact]
    public void UserGrantRole_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserGrantRole(It.IsAny<AuthUserGrantRoleRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserGrantRoleResponse());

        client.UserGrantRole(new AuthUserGrantRoleRequest { User = "user", Role = "role" });

        mock.Verify(x => x.UserGrantRole(
            It.Is<AuthUserGrantRoleRequest>(r => r.User == "user" && r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserGrantRoleAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserGrantRoleAsync(It.IsAny<AuthUserGrantRoleRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserGrantRoleResponse()));

        await client.UserGrantRoleAsync(new AuthUserGrantRoleRequest { User = "user", Role = "role" });

        mock.Verify(x => x.UserGrantRoleAsync(
            It.Is<AuthUserGrantRoleRequest>(r => r.User == "user" && r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- UserRevokeRole -----

    [Fact]
    public void UserRevokeRole_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserRevokeRole(It.IsAny<AuthUserRevokeRoleRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthUserRevokeRoleResponse());

        client.UserRevokeRole(new AuthUserRevokeRoleRequest { Name = "user", Role = "role" });

        mock.Verify(x => x.UserRevokeRole(
            It.Is<AuthUserRevokeRoleRequest>(r => r.Name == "user" && r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserRevokeRoleAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.UserRevokeRoleAsync(It.IsAny<AuthUserRevokeRoleRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthUserRevokeRoleResponse()));

        await client.UserRevokeRoleAsync(new AuthUserRevokeRoleRequest { Name = "user", Role = "role" });

        mock.Verify(x => x.UserRevokeRoleAsync(
            It.Is<AuthUserRevokeRoleRequest>(r => r.Name == "user" && r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleAdd -----

    [Fact]
    public void RoleAdd_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleAdd(It.IsAny<AuthRoleAddRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleAddResponse());

        client.RoleAdd(new AuthRoleAddRequest { Name = "role" });

        mock.Verify(x => x.RoleAdd(
            It.Is<AuthRoleAddRequest>(r => r.Name == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleAddAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleAddAsync(It.IsAny<AuthRoleAddRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleAddResponse()));

        await client.RoleAddAsync(new AuthRoleAddRequest { Name = "role" });

        mock.Verify(x => x.RoleAddAsync(
            It.Is<AuthRoleAddRequest>(r => r.Name == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleGet -----

    [Fact]
    public void RoleGet_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleGet(It.IsAny<AuthRoleGetRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleGetResponse());

        client.RoleGet(new AuthRoleGetRequest { Role = "role" });

        mock.Verify(x => x.RoleGet(
            It.Is<AuthRoleGetRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleGetAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleGetAsync(It.IsAny<AuthRoleGetRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleGetResponse()));

        await client.RoleGetAsync(new AuthRoleGetRequest { Role = "role" });

        mock.Verify(x => x.RoleGetAsync(
            It.Is<AuthRoleGetRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleList -----

    [Fact]
    public void RoleList_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleList(It.IsAny<AuthRoleListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleListResponse());

        client.RoleList(new AuthRoleListRequest());

        mock.Verify(x => x.RoleList(It.IsAny<AuthRoleListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleListAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleListAsync(It.IsAny<AuthRoleListRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleListResponse()));

        await client.RoleListAsync(new AuthRoleListRequest());

        mock.Verify(x => x.RoleListAsync(It.IsAny<AuthRoleListRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleDelete -----

    [Fact]
    public void RoleDelete_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleDelete(It.IsAny<AuthRoleDeleteRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleDeleteResponse());

        client.RoleDelete(new AuthRoleDeleteRequest { Role = "role" });

        mock.Verify(x => x.RoleDelete(
            It.Is<AuthRoleDeleteRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleDeleteAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleDeleteAsync(It.IsAny<AuthRoleDeleteRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleDeleteResponse()));

        await client.RoleDeleteAsync(new AuthRoleDeleteRequest { Role = "role" });

        mock.Verify(x => x.RoleDeleteAsync(
            It.Is<AuthRoleDeleteRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleGrantPermission -----

    [Fact]
    public void RoleGrantPermission_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleGrantPermission(It.IsAny<AuthRoleGrantPermissionRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleGrantPermissionResponse());

        client.RoleGrantPermission(new AuthRoleGrantPermissionRequest { Name = "role" });

        mock.Verify(x => x.RoleGrantPermission(
            It.Is<AuthRoleGrantPermissionRequest>(r => r.Name == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleGrantPermissionAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleGrantPermissionAsync(It.IsAny<AuthRoleGrantPermissionRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleGrantPermissionResponse()));

        await client.RoleGrantPermissionAsync(new AuthRoleGrantPermissionRequest { Name = "role" });

        mock.Verify(x => x.RoleGrantPermissionAsync(
            It.Is<AuthRoleGrantPermissionRequest>(r => r.Name == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ----- RoleRevokePermission -----

    [Fact]
    public void RoleRevokePermission_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleRevokePermission(It.IsAny<AuthRoleRevokePermissionRequest>(), It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(new AuthRoleRevokePermissionResponse());

        client.RoleRevokePermission(new AuthRoleRevokePermissionRequest { Role = "role" });

        mock.Verify(x => x.RoleRevokePermission(
            It.Is<AuthRoleRevokePermissionRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RoleRevokePermissionAsync_ShouldCallGrpcClient()
    {
        var (client, mock) = CreateClient();
        mock.Setup(x => x.RoleRevokePermissionAsync(It.IsAny<AuthRoleRevokePermissionRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(TestHelper.CreateAsyncUnaryCall(new AuthRoleRevokePermissionResponse()));

        await client.RoleRevokePermissionAsync(new AuthRoleRevokePermissionRequest { Role = "role" });

        mock.Verify(x => x.RoleRevokePermissionAsync(
            It.Is<AuthRoleRevokePermissionRequest>(r => r.Role == "role"),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
