using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AuthClientTests
{
    [Fact]
    public void AuthEnable_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        var expectedResponse = new AuthEnableResponse();

        mockAuthClient
            .Setup(x => x.AuthEnable(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthEnableRequest();
        var result = client.AuthEnable(request);

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

        var asyncResponse = new AsyncUnaryCall<AuthEnableResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockAuthClient
            .Setup(x => x.AuthEnableAsync(It.IsAny<AuthEnableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthEnableRequest();
        var result = await client.AuthEnableAsync(request);

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

        var expectedResponse = new AuthDisableResponse();

        mockAuthClient
            .Setup(x => x.AuthDisable(It.IsAny<AuthDisableRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthDisableRequest();
        var result = client.AuthDisable(request);

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

        var expectedResponse = new AuthenticateResponse
        {
            Token = "test-token"
        };

        mockAuthClient
            .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthenticateRequest
        {
            Name = "test-user",
            Password = "test-password"
        };
        var result = client.Authenticate(request);

        // Assert
        mockAuthClient.Verify(x => x.Authenticate(
            It.Is<AuthenticateRequest>(r =>
                r.Name == "test-user" &&
                r.Password == "test-password"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal("test-token", result.Token);
    }

    [Fact]
    public void UserAdd_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        var expectedResponse = new AuthUserAddResponse();

        mockAuthClient
            .Setup(x => x.UserAdd(It.IsAny<AuthUserAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthUserAddRequest
        {
            Name = "test-user",
            Password = "test-password"
        };
        var result = client.UserAdd(request);

        // Assert
        mockAuthClient.Verify(x => x.UserAdd(
            It.Is<AuthUserAddRequest>(r =>
                r.Name == "test-user" &&
                r.Password == "test-password"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void RoleAdd_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        var expectedResponse = new AuthRoleAddResponse();

        mockAuthClient
            .Setup(x => x.RoleAdd(It.IsAny<AuthRoleAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthRoleAddRequest
        {
            Name = "test-role"
        };
        var result = client.RoleAdd(request);

        // Assert
        mockAuthClient.Verify(x => x.RoleAdd(
            It.Is<AuthRoleAddRequest>(r => r.Name == "test-role"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task RoleAddAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        var expectedResponse = new AuthRoleAddResponse();

        var asyncResponse = new AsyncUnaryCall<AuthRoleAddResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockAuthClient
            .Setup(x => x.RoleAddAsync(It.IsAny<AuthRoleAddRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthRoleAddRequest
        {
            Name = "test-role"
        };
        var result = await client.RoleAddAsync(request);

        // Assert
        mockAuthClient.Verify(x => x.RoleAddAsync(
            It.Is<AuthRoleAddRequest>(r => r.Name == "test-role"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void UserGrantRole_ShouldCallGrpcClient()
    {
        // Arrange
        var mockAuthClient = new Mock<Auth.AuthClient>();

        var expectedResponse = new AuthUserGrantRoleResponse();

        mockAuthClient
            .Setup(x => x.UserGrantRole(It.IsAny<AuthUserGrantRoleRequest>(), It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Auth client
        TestHelper.SetupMockClientViaConnection(client, mockAuthClient.Object, "_authClient");

        // Act
        var request = new AuthUserGrantRoleRequest
        {
            User = "test-user",
            Role = "test-role"
        };
        var result = client.UserGrantRole(request);

        // Assert
        mockAuthClient.Verify(x => x.UserGrantRole(
            It.Is<AuthUserGrantRoleRequest>(r =>
                r.User == "test-user" &&
                r.Role == "test-role"),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}