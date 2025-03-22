using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class MaintenanceClientUnitTests
{
    [Fact]
    public void Alarm_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        mockMaintenanceClient
            .Setup(x => x.Alarm(It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new AlarmResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.Nospace
        };
        client.Alarm(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.Alarm(
            It.Is<AlarmRequest>(r =>
                r.Action == AlarmRequest.Types.AlarmAction.Get &&
                r.Alarm == AlarmType.Nospace),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task AlarmAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new AlarmResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockMaintenanceClient
            .Setup(x => x.AlarmAsync(It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            Alarm = AlarmType.Nospace
        };
        await client.AlarmAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.AlarmAsync(
            It.Is<AlarmRequest>(r =>
                r.Action == AlarmRequest.Types.AlarmAction.Get &&
                r.Alarm == AlarmType.Nospace),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Status_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        mockMaintenanceClient
            .Setup(x => x.Status(It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new StatusResponse { Version = "3.4.0" });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new StatusRequest();
        var response = client.Status(request);

        // Assert
        Assert.Equal("3.4.0", response.Version);

        mockMaintenanceClient.Verify(x => x.Status(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task StatusAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new StatusResponse { Version = "3.4.0" };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockMaintenanceClient
            .Setup(x => x.StatusAsync(It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new StatusRequest();
        var response = await client.StatusAsync(request);

        // Assert
        Assert.Equal("3.4.0", response.Version);

        mockMaintenanceClient.Verify(x => x.StatusAsync(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Defragment_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        mockMaintenanceClient
            .Setup(x => x.Defragment(It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new DefragmentResponse());

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new DefragmentRequest();
        client.Defragment(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.Defragment(
            It.IsAny<DefragmentRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task DefragmentAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new DefragmentResponse();
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockMaintenanceClient
            .Setup(x => x.DefragmentAsync(It.IsAny<DefragmentRequest>(), It.IsAny<Metadata>(), null,
                CancellationToken.None))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new DefragmentRequest();
        await client.DefragmentAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.DefragmentAsync(
            It.IsAny<DefragmentRequest>(),
            It.IsAny<Metadata>(),
            null,
            CancellationToken.None
        ), Times.Once);
    }

    [Fact]
    public void Hash_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        mockMaintenanceClient
            .Setup(x => x.Hash(It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(new HashResponse { Hash = 12345 });

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new HashRequest();
        var response = client.Hash(request);

        // Assert
        Assert.Equal(12345u, response.Hash);

        mockMaintenanceClient.Verify(x => x.Hash(
            It.IsAny<HashRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task HashAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new HashResponse { Hash = 12345 };
        var asyncResponse = TestHelper.CreateAsyncUnaryCall(expectedResponse);

        mockMaintenanceClient
            .Setup(x => x.HashAsync(It.IsAny<HashRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new HashRequest();
        var response = await client.HashAsync(request);

        // Assert
        Assert.Equal(12345u, response.Hash);

        mockMaintenanceClient.Verify(x => x.HashAsync(
            It.IsAny<HashRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}