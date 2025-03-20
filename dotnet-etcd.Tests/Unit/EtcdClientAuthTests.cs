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
    }
}
