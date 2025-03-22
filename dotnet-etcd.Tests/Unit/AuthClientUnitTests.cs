using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
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
}