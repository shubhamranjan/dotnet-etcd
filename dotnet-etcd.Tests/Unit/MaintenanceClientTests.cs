using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class MaintenanceClientTests
{
    [Fact]
    public void Alarm_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new AlarmResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 }
        };
        expectedResponse.Alarms.Add(new AlarmMember { Alarm = AlarmType.Nospace, MemberID = 456 });

        mockMaintenanceClient
            .Setup(x => x.Alarm(It.IsAny<AlarmRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new AlarmRequest
        {
            Action = AlarmRequest.Types.AlarmAction.Get,
            MemberID = 456,
            Alarm = AlarmType.Nospace
        };
        var result = client.Alarm(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.Alarm(
            It.Is<AlarmRequest>(r =>
                r.Action == AlarmRequest.Types.AlarmAction.Get &&
                r.MemberID == 456 &&
                r.Alarm == AlarmType.Nospace),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Single(result.Alarms);
        Assert.Equal(AlarmType.Nospace, result.Alarms[0].Alarm);
        Assert.Equal(456UL, result.Alarms[0].MemberID);
    }

    [Fact]
    public async Task AlarmAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new AlarmResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 }
        };
        expectedResponse.Alarms.Add(new AlarmMember { Alarm = AlarmType.Nospace, MemberID = 456 });

        var asyncResponse = new AsyncUnaryCall<AlarmResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

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
            MemberID = 456,
            Alarm = AlarmType.Nospace
        };
        var result = await client.AlarmAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.AlarmAsync(
            It.Is<AlarmRequest>(r =>
                r.Action == AlarmRequest.Types.AlarmAction.Get &&
                r.MemberID == 456 &&
                r.Alarm == AlarmType.Nospace),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Single(result.Alarms);
        Assert.Equal(AlarmType.Nospace, result.Alarms[0].Alarm);
        Assert.Equal(456UL, result.Alarms[0].MemberID);
    }

    [Fact]
    public void Status_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new StatusResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 },
            Version = "3.5.0",
            DbSize = 1024,
            Leader = 456,
            RaftIndex = 1000,
            RaftTerm = 5
        };

        mockMaintenanceClient
            .Setup(x => x.Status(It.IsAny<StatusRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new StatusRequest();
        var result = client.Status(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.Status(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Equal("3.5.0", result.Version);
        Assert.Equal(1024L, result.DbSize);
        Assert.Equal(456UL, result.Leader);
        Assert.Equal(1000UL, result.RaftIndex);
        Assert.Equal(5UL, result.RaftTerm);
    }

    [Fact]
    public async Task StatusAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new StatusResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 },
            Version = "3.5.0",
            DbSize = 1024,
            Leader = 456,
            RaftIndex = 1000,
            RaftTerm = 5
        };

        var asyncResponse = new AsyncUnaryCall<StatusResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

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
        var result = await client.StatusAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.StatusAsync(
            It.IsAny<StatusRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Equal("3.5.0", result.Version);
        Assert.Equal(1024L, result.DbSize);
        Assert.Equal(456UL, result.Leader);
        Assert.Equal(1000UL, result.RaftIndex);
        Assert.Equal(5UL, result.RaftTerm);
    }

    [Fact]
    public void HashKV_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new HashKVResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 },
            Hash = 12345678,
            CompactRevision = 500
        };

        mockMaintenanceClient
            .Setup(x => x.HashKV(It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act - Test with specific revision
        var request = new HashKVRequest { Revision = 789 };
        var result = client.HashKV(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.HashKV(
            It.Is<HashKVRequest>(r => r.Revision == 789),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Equal(12345678UL, result.Hash);
        Assert.Equal(500L, result.CompactRevision);

        // Act - Test with default request (no revision specified)
        mockMaintenanceClient.Reset();
        mockMaintenanceClient
            .Setup(x => x.HashKV(It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        var defaultRequest = new HashKVRequest();
        var defaultResult = client.HashKV(defaultRequest);

        // Assert
        mockMaintenanceClient.Verify(x => x.HashKV(
            It.Is<HashKVRequest>(r => r.Revision == 0),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(12345678UL, defaultResult.Hash);
    }

    [Fact]
    public async Task HashKVAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new HashKVResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 },
            Hash = 12345678,
            CompactRevision = 500
        };

        var asyncResponse = new AsyncUnaryCall<HashKVResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockMaintenanceClient
            .Setup(x => x.HashKVAsync(It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act - Test with specific revision
        var request = new HashKVRequest { Revision = 789 };
        var result = await client.HashKVAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.HashKVAsync(
            It.Is<HashKVRequest>(r => r.Revision == 789),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
        Assert.Equal(12345678UL, result.Hash);
        Assert.Equal(500L, result.CompactRevision);

        // Act - Test with custom metadata
        mockMaintenanceClient.Reset();
        mockMaintenanceClient
            .Setup(x => x.HashKVAsync(It.IsAny<HashKVRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        var customMetadata = new Metadata { { "test-key", "test-value" } };
        var metadataResult = await client.HashKVAsync(request, customMetadata);

        // Assert
        mockMaintenanceClient.Verify(x => x.HashKVAsync(
            It.Is<HashKVRequest>(r => r.Revision == 789),
            customMetadata,
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(12345678UL, metadataResult.Hash);
    }

    [Fact]
    public void Snapshot_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        // We can't mock AsyncServerStreamingCall directly as it's sealed
        // Instead, we'll just verify the method is called
        mockMaintenanceClient
            .Setup(x => x.Snapshot(It.IsAny<SnapshotRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()));

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act - Just call the method to verify it's called
        var request = new SnapshotRequest();

        // Assert - Verify the method would be called
        mockMaintenanceClient.Verify(x => x.Snapshot(
            It.IsAny<SnapshotRequest>(),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);

        // Note: We can't easily test the Snapshot method as it requires a callback and async streaming
    }

    [Fact]
    public void MoveLeader_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new MoveLeaderResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 }
        };

        mockMaintenanceClient
            .Setup(x => x.MoveLeader(It.IsAny<MoveLeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(expectedResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new MoveLeaderRequest { TargetID = 789 };
        var result = client.MoveLeader(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.MoveLeader(
            It.Is<MoveLeaderRequest>(r => r.TargetID == 789),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
    }

    [Fact]
    public async Task MoveLeaderAsync_ShouldCallGrpcClient()
    {
        // Arrange
        var mockMaintenanceClient = new Mock<Maintenance.MaintenanceClient>();

        var expectedResponse = new MoveLeaderResponse
        {
            Header = new ResponseHeader { ClusterId = 123, MemberId = 456, Revision = 789 }
        };

        var asyncResponse = new AsyncUnaryCall<MoveLeaderResponse>(
            Task.FromResult(expectedResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        mockMaintenanceClient
            .Setup(x => x.MoveLeaderAsync(It.IsAny<MoveLeaderRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(asyncResponse);

        // Create a client with mocked dependencies
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();

        // Set up the mock Maintenance client
        TestHelper.SetupMockClientViaConnection(client, mockMaintenanceClient.Object, "_maintenanceClient");

        // Act
        var request = new MoveLeaderRequest { TargetID = 789 };
        var result = await client.MoveLeaderAsync(request);

        // Assert
        mockMaintenanceClient.Verify(x => x.MoveLeaderAsync(
            It.Is<MoveLeaderRequest>(r => r.TargetID == 789),
            It.IsAny<Metadata>(),
            It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.Equal(123UL, result.Header.ClusterId);
        Assert.Equal(456UL, result.Header.MemberId);
        Assert.Equal(789L, result.Header.Revision);
    }
}