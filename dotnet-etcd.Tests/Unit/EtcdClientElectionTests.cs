using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using V3Electionpb;
using Xunit;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class EtcdClientElectionTests
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<Election.ElectionClient> _mockElectionClient;
        private readonly EtcdClient _client;

        public EtcdClientElectionTests()
        {
            _mockConnection = MockConnection.Create();
            _mockElectionClient = new Mock<Election.ElectionClient>();
            _mockConnection.SetupGet(x => x.ElectionClient).Returns(_mockElectionClient.Object);
            _client = new EtcdClient(_mockConnection.Object);
        }

        [Fact]
        public async Task CampaignAsync_WithStringNameAndValue_ShouldCallElectionClientWithCorrectParameters()
        {
            // Arrange
            var name = "test-election";
            var value = "test-value";
            var expectedResponse = new CampaignResponse
            {
                Leader = new LeaderKey
                {
                    Name = ByteString.CopyFromUtf8(name),
                    Key = ByteString.CopyFromUtf8("some-key"),
                    Rev = 1,
                    Lease = 123
                }
            };

            var asyncCall = AsyncUnaryCallFactory.Create(expectedResponse);
            _mockElectionClient
                .Setup(x => x.CampaignAsync(
                    It.IsAny<CampaignRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.CampaignAsync(name, value);

            // Assert
            _mockElectionClient.Verify(x => x.CampaignAsync(
                It.Is<CampaignRequest>(r => 
                    r.Name.ToStringUtf8() == name && 
                    r.Value.ToStringUtf8() == value),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task ProclaimAsync_WithLeaderKeyAndValue_ShouldCallElectionClientWithCorrectParameters()
        {
            // Arrange
            var leaderKey = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("leader-key"),
                Rev = 1,
                Lease = 123
            };
            var value = "new-value";
            var expectedResponse = new ProclaimResponse();

            var asyncCall = AsyncUnaryCallFactory.Create(expectedResponse);
            _mockElectionClient
                .Setup(x => x.ProclaimAsync(
                    It.IsAny<ProclaimRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.ProclaimAsync(leaderKey, value);

            // Assert
            _mockElectionClient.Verify(x => x.ProclaimAsync(
                It.Is<ProclaimRequest>(r => 
                    r.Leader == leaderKey && 
                    r.Value.ToStringUtf8() == value),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task LeaderAsync_WithStringName_ShouldCallElectionClientWithCorrectParameters()
        {
            // Arrange
            var name = "test-election";
            var expectedResponse = new LeaderResponse
            {
                Kv = new KeyValue
                {
                    Key = ByteString.CopyFromUtf8("leader-key"),
                    Value = ByteString.CopyFromUtf8("leader-value")
                }
            };

            var asyncCall = AsyncUnaryCallFactory.Create(expectedResponse);
            _mockElectionClient
                .Setup(x => x.LeaderAsync(
                    It.IsAny<LeaderRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.LeaderAsync(name);

            // Assert
            _mockElectionClient.Verify(x => x.LeaderAsync(
                It.Is<LeaderRequest>(r => r.Name.ToStringUtf8() == name),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task ObserveAsync_WithLeaderRequest_ShouldStreamResponses()
        {
            // Arrange
            var request = new LeaderRequest
            {
                Name = ByteString.CopyFromUtf8("test-election")
            };
            
            var responseStream = AsyncStreamingCallFactory.Create<LeaderResponse>(
                new List<LeaderResponse>
                {
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key1"), Value = ByteString.CopyFromUtf8("value1") } },
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key2"), Value = ByteString.CopyFromUtf8("value2") } }
                });

            _mockElectionClient
                .Setup(x => x.Observe(
                    It.IsAny<LeaderRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(responseStream);

            // Act
            var results = new List<LeaderResponse>();
            await foreach (var item in _client.ObserveAsync(request))
            {
                results.Add(item);
            }

            // Assert
            _mockElectionClient.Verify(x => x.Observe(
                It.Is<LeaderRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(2, results.Count);
            Assert.Equal("key1", results[0].Kv.Key.ToStringUtf8());
            Assert.Equal("value1", results[0].Kv.Value.ToStringUtf8());
            Assert.Equal("key2", results[1].Kv.Key.ToStringUtf8());
            Assert.Equal("value2", results[1].Kv.Value.ToStringUtf8());
        }

        [Fact]
        public async Task ObserveAsync_WithStringName_ShouldStreamResponses()
        {
            // Arrange
            var name = "test-election";
            
            var responseStream = AsyncStreamingCallFactory.Create<LeaderResponse>(
                new List<LeaderResponse>
                {
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key1"), Value = ByteString.CopyFromUtf8("value1") } },
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key2"), Value = ByteString.CopyFromUtf8("value2") } }
                });

            _mockElectionClient
                .Setup(x => x.Observe(
                    It.IsAny<LeaderRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(responseStream);

            // Act
            var results = new List<LeaderResponse>();
            await foreach (var item in _client.ObserveAsync(name))
            {
                results.Add(item);
            }

            // Assert
            _mockElectionClient.Verify(x => x.Observe(
                It.Is<LeaderRequest>(r => r.Name.ToStringUtf8() == name),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task ResignAsync_WithLeaderKey_ShouldCallElectionClientWithCorrectParameters()
        {
            // Arrange
            var leaderKey = new LeaderKey
            {
                Name = ByteString.CopyFromUtf8("test-election"),
                Key = ByteString.CopyFromUtf8("leader-key"),
                Rev = 1,
                Lease = 123
            };
            var expectedResponse = new ResignResponse();

            var asyncCall = AsyncUnaryCallFactory.Create(expectedResponse);
            _mockElectionClient
                .Setup(x => x.ResignAsync(
                    It.IsAny<ResignRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.ResignAsync(leaderKey);

            // Assert
            _mockElectionClient.Verify(x => x.ResignAsync(
                It.Is<ResignRequest>(r => r.Leader == leaderKey),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task Observe_WithLeaderRequest_ShouldStreamResponses()
        {
            // Arrange
            var request = new LeaderRequest
            {
                Name = ByteString.CopyFromUtf8("test-election")
            };
            var callbackCount = 0;

            var responseStream = AsyncStreamingCallFactory.Create<LeaderResponse>(
                new List<LeaderResponse>
                {
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key1"), Value = ByteString.CopyFromUtf8("value1") } },
                    new LeaderResponse { Kv = new KeyValue { Key = ByteString.CopyFromUtf8("key2"), Value = ByteString.CopyFromUtf8("value2") } }
                });
            _mockElectionClient
                .Setup(x => x.Observe(
                    It.IsAny<LeaderRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(responseStream);
            // Act
            await foreach (var response in _client.ObserveAsync(request))
            {
                callbackCount++;
            }
            // Assert
            _mockElectionClient.Verify(x => x.Observe(
                It.Is<LeaderRequest>(r => r == request),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(2, callbackCount);
        }
    }
}