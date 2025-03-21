using System;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using Xunit;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class EtcdClientAuthTests
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<Auth.AuthClient> _mockAuthClient;
        private readonly EtcdClient _client;

        public EtcdClientAuthTests()
        {
            _mockConnection = new Mock<IConnection>();
            _mockAuthClient = new Mock<Auth.AuthClient>();
            _mockConnection.SetupGet(x => x.AuthClient).Returns(_mockAuthClient.Object);
            _client = new EtcdClient(_mockConnection.Object);
        }

        [Fact]
        public void UserList_ShouldCallUserListWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserListResponse();
            _mockAuthClient
                .Setup(x => x.UserList(
                    It.IsAny<AuthUserListRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthUserListRequest();
            var result = _client.UserList(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserList(
                It.Is<AuthUserListRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UserListAsync_ShouldCallUserListAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserListResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserListAsync(
                    It.IsAny<AuthUserListRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthUserListRequest();
            var asyncResult = await _client.UserListAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserListAsync(
                It.Is<AuthUserListRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void UserDelete_ShouldCallUserDeleteWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserDeleteResponse();
            _mockAuthClient
                .Setup(x => x.UserDelete(
                    It.IsAny<AuthUserDeleteRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var deleteRequest = new AuthUserDeleteRequest { Name = "test-user" };
            _client.UserDelete(deleteRequest);

            // Assert
            _mockAuthClient.Verify(x => x.UserDelete(
                It.Is<AuthUserDeleteRequest>(r => r.Name == "test-user"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UserDeleteAsync_ShouldCallUserDeleteAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserDeleteResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserDeleteAsync(
                    It.IsAny<AuthUserDeleteRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var deleteRequest = new AuthUserDeleteRequest { Name = "test-user" };
            await _client.UserDeleteAsync(deleteRequest);

            // Assert
            _mockAuthClient.Verify(x => x.UserDeleteAsync(
                It.Is<AuthUserDeleteRequest>(r => r.Name == "test-user"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void UserChangePassword_ShouldCallUserChangePasswordWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserChangePasswordResponse();
            _mockAuthClient
                .Setup(x => x.UserChangePassword(
                    It.IsAny<AuthUserChangePasswordRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthUserChangePasswordRequest
            {
                Name = "test-user",
                Password = "new-password"
            };
            _client.UserChangePassword(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserChangePassword(
                It.Is<AuthUserChangePasswordRequest>(r => 
                    r.Name == "test-user" && 
                    r.Password == "new-password"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UserChangePasswordAsync_ShouldCallUserChangePasswordAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserChangePasswordResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserChangePasswordAsync(
                    It.IsAny<AuthUserChangePasswordRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthUserChangePasswordRequest
            {
                Name = "test-user",
                Password = "new-password"
            };
            await _client.UserChangePasswordAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserChangePasswordAsync(
                It.Is<AuthUserChangePasswordRequest>(r => 
                    r.Name == "test-user" && 
                    r.Password == "new-password"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void UserGrantRole_ShouldCallUserGrantRoleWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserGrantRoleResponse();
            _mockAuthClient
                .Setup(x => x.UserGrantRole(
                    It.IsAny<AuthUserGrantRoleRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthUserGrantRoleRequest
            {
                User = "test-user",
                Role = "test-role"
            };
            _client.UserGrantRole(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserGrantRole(
                It.Is<AuthUserGrantRoleRequest>(r => 
                    r.User == "test-user" && 
                    r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UserGrantRoleAsync_ShouldCallUserGrantRoleAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserGrantRoleResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserGrantRoleAsync(
                    It.IsAny<AuthUserGrantRoleRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthUserGrantRoleRequest
            {
                User = "test-user",
                Role = "test-role"
            };
            await _client.UserGrantRoleAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserGrantRoleAsync(
                It.Is<AuthUserGrantRoleRequest>(r => 
                    r.User == "test-user" && 
                    r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void UserGet_ShouldCallUserGetWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserGetResponse();
            _mockAuthClient
                .Setup(x => x.UserGet(
                    It.IsAny<AuthUserGetRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthUserGetRequest { Name = "test-user" };
            var result = _client.UserGet(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserGet(
                It.Is<AuthUserGetRequest>(r => r.Name == "test-user"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UserGetAsync_ShouldCallUserGetAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserGetResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserGetAsync(
                    It.IsAny<AuthUserGetRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthUserGetRequest { Name = "test-user" };
            var result = await _client.UserGetAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserGetAsync(
                It.Is<AuthUserGetRequest>(r => r.Name == "test-user"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void UserRevokeRole_ShouldCallUserRevokeRoleWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserRevokeRoleResponse();
            _mockAuthClient
                .Setup(x => x.UserRevokeRole(
                    It.IsAny<AuthUserRevokeRoleRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthUserRevokeRoleRequest
            {
                Name = "test-user",
                Role = "test-role"
            };
            var result = _client.UserRevokeRole(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserRevokeRole(
                It.Is<AuthUserRevokeRoleRequest>(r => 
                    r.Name == "test-user" && 
                    r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UserRevokeRoleAsync_ShouldCallUserRevokeRoleAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthUserRevokeRoleResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.UserRevokeRoleAsync(
                    It.IsAny<AuthUserRevokeRoleRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthUserRevokeRoleRequest
            {
                Name = "test-user",
                Role = "test-role"
            };
            var result = await _client.UserRevokeRoleAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.UserRevokeRoleAsync(
                It.Is<AuthUserRevokeRoleRequest>(r => 
                    r.Name == "test-user" && 
                    r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void AuthDisable_ShouldCallAuthDisableWithCorrectParameters()
        {
            // Arrange
            var response = new AuthDisableResponse();
            _mockAuthClient
                .Setup(x => x.AuthDisable(
                    It.IsAny<AuthDisableRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthDisableRequest();
            var result = _client.AuthDisable(request);

            // Assert
            _mockAuthClient.Verify(x => x.AuthDisable(
                It.Is<AuthDisableRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task AuthDisableAsync_ShouldCallAuthDisableAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthDisableResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.AuthDisableAsync(
                    It.IsAny<AuthDisableRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthDisableRequest();
            var result = await _client.AuthDisableAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.AuthDisableAsync(
                It.Is<AuthDisableRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void RoleGet_ShouldCallRoleGetWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleGetResponse();
            _mockAuthClient
                .Setup(x => x.RoleGet(
                    It.IsAny<AuthRoleGetRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthRoleGetRequest { Role = "test-role" };
            var result = _client.RoleGet(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleGet(
                It.Is<AuthRoleGetRequest>(r => r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RoleGetAsync_ShouldCallRoleGetAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleGetResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.RoleGetAsync(
                    It.IsAny<AuthRoleGetRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthRoleGetRequest { Role = "test-role" };
            var result = await _client.RoleGetAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleGetAsync(
                It.Is<AuthRoleGetRequest>(r => r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void RoleList_ShouldCallRoleListWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleListResponse();
            _mockAuthClient
                .Setup(x => x.RoleList(
                    It.IsAny<AuthRoleListRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthRoleListRequest();
            var result = _client.RoleList(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleList(
                It.Is<AuthRoleListRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RoleListAsync_ShouldCallRoleListAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleListResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.RoleListAsync(
                    It.IsAny<AuthRoleListRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthRoleListRequest();
            var result = await _client.RoleListAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleListAsync(
                It.Is<AuthRoleListRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void RoleDelete_ShouldCallRoleDeleteWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleDeleteResponse();
            _mockAuthClient
                .Setup(x => x.RoleDelete(
                    It.IsAny<AuthRoleDeleteRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthRoleDeleteRequest { Role = "test-role" };
            var result = _client.RoleDelete(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleDelete(
                It.Is<AuthRoleDeleteRequest>(r => r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RoleDeleteAsync_ShouldCallRoleDeleteAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleDeleteResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.RoleDeleteAsync(
                    It.IsAny<AuthRoleDeleteRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthRoleDeleteRequest { Role = "test-role" };
            var result = await _client.RoleDeleteAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleDeleteAsync(
                It.Is<AuthRoleDeleteRequest>(r => r.Role == "test-role"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void RoleGrantPermission_ShouldCallRoleGrantPermissionWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleGrantPermissionResponse();
            _mockAuthClient
                .Setup(x => x.RoleGrantPermission(
                    It.IsAny<AuthRoleGrantPermissionRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthRoleGrantPermissionRequest
            {
                Name = "test-role",
                Perm = new Authpb.Permission
                {
                    Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key"),
                    PermType = Authpb.Permission.Types.Type.Read
                }
            };
            var result = _client.RoleGrantPermission(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleGrantPermission(
                It.Is<AuthRoleGrantPermissionRequest>(r => 
                    r.Name == "test-role" && 
                    r.Perm.Key.ToStringUtf8() == "test-key" &&
                    r.Perm.PermType == Authpb.Permission.Types.Type.Read),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RoleGrantPermissionAsync_ShouldCallRoleGrantPermissionAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleGrantPermissionResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.RoleGrantPermissionAsync(
                    It.IsAny<AuthRoleGrantPermissionRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthRoleGrantPermissionRequest
            {
                Name = "test-role",
                Perm = new Authpb.Permission
                {
                    Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key"),
                    PermType = Authpb.Permission.Types.Type.Read
                }
            };
            var result = await _client.RoleGrantPermissionAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleGrantPermissionAsync(
                It.Is<AuthRoleGrantPermissionRequest>(r => 
                    r.Name == "test-role" && 
                    r.Perm.Key.ToStringUtf8() == "test-key" &&
                    r.Perm.PermType == Authpb.Permission.Types.Type.Read),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void RoleRevokePermission_ShouldCallRoleRevokePermissionWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleRevokePermissionResponse();
            _mockAuthClient
                .Setup(x => x.RoleRevokePermission(
                    It.IsAny<AuthRoleRevokePermissionRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthRoleRevokePermissionRequest
            {
                Role = "test-role",
                Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key")
            };
            var result = _client.RoleRevokePermission(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleRevokePermission(
                It.Is<AuthRoleRevokePermissionRequest>(r => 
                    r.Role == "test-role" && 
                    r.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RoleRevokePermissionAsync_ShouldCallRoleRevokePermissionAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthRoleRevokePermissionResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.RoleRevokePermissionAsync(
                    It.IsAny<AuthRoleRevokePermissionRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthRoleRevokePermissionRequest
            {
                Role = "test-role",
                Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key")
            };
            var result = await _client.RoleRevokePermissionAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.RoleRevokePermissionAsync(
                It.Is<AuthRoleRevokePermissionRequest>(r => 
                    r.Role == "test-role" && 
                    r.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void AuthEnable_ShouldCallAuthEnableWithCorrectParameters()
        {
            // Arrange
            var response = new AuthEnableResponse();
            _mockAuthClient
                .Setup(x => x.AuthEnable(
                    It.IsAny<AuthEnableRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthEnableRequest();
            var result = _client.AuthEnable(request);

            // Assert
            _mockAuthClient.Verify(x => x.AuthEnable(
                It.Is<AuthEnableRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task AuthEnableAsync_ShouldCallAuthEnableAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthEnableResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.AuthEnableAsync(
                    It.IsAny<AuthEnableRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthEnableRequest();
            var result = await _client.AuthEnableAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.AuthEnableAsync(
                It.Is<AuthEnableRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal(response, result);
        }

        [Fact]
        public void Authenticate_ShouldCallAuthenticateWithCorrectParameters()
        {
            // Arrange
            var response = new AuthenticateResponse { Token = "test-token" };
            _mockAuthClient
                .Setup(x => x.Authenticate(
                    It.IsAny<AuthenticateRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(response);

            // Act
            var request = new AuthenticateRequest
            {
                Name = "test-user",
                Password = "test-password"
            };
            var result = _client.Authenticate(request);

            // Assert
            _mockAuthClient.Verify(x => x.Authenticate(
                It.Is<AuthenticateRequest>(r => 
                    r.Name == "test-user" && 
                    r.Password == "test-password"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal("test-token", result.Token);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldCallAuthenticateAsyncWithCorrectParameters()
        {
            // Arrange
            var response = new AuthenticateResponse { Token = "test-token" };
            var asyncCall = AsyncUnaryCallFactory.Create(response);
            _mockAuthClient
                .Setup(x => x.AuthenticateAsync(
                    It.IsAny<AuthenticateRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var request = new AuthenticateRequest
            {
                Name = "test-user",
                Password = "test-password"
            };
            var result = await _client.AuthenticateAsync(request);

            // Assert
            _mockAuthClient.Verify(x => x.AuthenticateAsync(
                It.Is<AuthenticateRequest>(r => 
                    r.Name == "test-user" && 
                    r.Password == "test-password"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Equal("test-token", result.Token);
        }
    }
}
