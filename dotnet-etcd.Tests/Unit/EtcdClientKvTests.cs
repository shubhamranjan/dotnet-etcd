using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using dotnet_etcd.Tests.Unit.Mocks;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Xunit;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class EtcdClientKvTests
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<KV.KVClient> _mockKvClient;
        private readonly EtcdClient _client;

        public EtcdClientKvTests()
        {
            _mockConnection = new Mock<IConnection>();
            _mockKvClient = new Mock<KV.KVClient>();
            _mockConnection.SetupGet(x => x.KVClient).Returns(_mockKvClient.Object);
            _client = new EtcdClient(_mockConnection.Object);
        }

        [Fact]
        public void GetRange_ShouldCallRangeWithCorrectParameters()
        {
            // Arrange
            var rangeResponse = new RangeResponse();
            _mockKvClient
                .Setup(x => x.Range(
                    It.IsAny<RangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(rangeResponse);

            // Act
            var result = _client.GetRange("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.Range(
                It.Is<RangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetRangeAsync_ShouldCallRangeAsyncWithCorrectParameters()
        {
            // Arrange
            var rangeResponse = new RangeResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(rangeResponse);
            _mockKvClient
                .Setup(x => x.RangeAsync(
                    It.IsAny<RangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.GetRangeAsync("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.RangeAsync(
                It.Is<RangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void GetRangeVal_ShouldCallRangeWithCorrectParameters()
        {
            // Arrange
            var rangeResponse = new RangeResponse
            {
                Kvs = { new KeyValue { Key = ByteString.CopyFromUtf8("key1"), Value = ByteString.CopyFromUtf8("value1") } }
            };
            _mockKvClient
                .Setup(x => x.Range(
                    It.IsAny<RangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(rangeResponse);

            // Act
            var result = _client.GetRangeVal("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.Range(
                It.Is<RangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);

        }

        [Fact]
        public async Task GetRangeValAsync_ShouldCallRangeAsyncWithCorrectParameters()
        {
            // Arrange
            var rangeResponse = new RangeResponse
            {
                Kvs = { new KeyValue { Key = ByteString.CopyFromUtf8("key1"), Value = ByteString.CopyFromUtf8("value1") } }
            };
            var asyncCall = AsyncUnaryCallFactory.Create(rangeResponse);
            _mockKvClient
                .Setup(x => x.RangeAsync(
                    It.IsAny<RangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            var result = await _client.GetRangeValAsync("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.RangeAsync(
                It.Is<RangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);

        }

        [Fact]
        public void Put_ShouldCallPutWithCorrectParameters()
        {
            // Arrange
            var putResponse = new PutResponse();
            _mockKvClient
                .Setup(x => x.Put(
                    It.IsAny<PutRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(putResponse);

            // Act
            _client.Put("test-key", "test-value");

            // Assert
            _mockKvClient.Verify(x => x.Put(
                It.Is<PutRequest>(r => 
                    r.Key.ToStringUtf8() == "test-key" && 
                    r.Value.ToStringUtf8() == "test-value"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task PutAsync_ShouldCallPutAsyncWithCorrectParameters()
        {
            // Arrange
            var putResponse = new PutResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(putResponse);
            _mockKvClient
                .Setup(x => x.PutAsync(
                    It.IsAny<PutRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            await _client.PutAsync("test-key", "test-value");

            // Assert
            _mockKvClient.Verify(x => x.PutAsync(
                It.Is<PutRequest>(r => 
                    r.Key.ToStringUtf8() == "test-key" && 
                    r.Value.ToStringUtf8() == "test-value"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void Delete_ShouldCallDeleteWithCorrectParameters()
        {
            // Arrange
            var deleteResponse = new DeleteRangeResponse();
            _mockKvClient
                .Setup(x => x.DeleteRange(
                    It.IsAny<DeleteRangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(deleteResponse);

            // Act
            _client.Delete("test-key");

            // Assert
            _mockKvClient.Verify(x => x.DeleteRange(
                It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteRangeAsyncWithCorrectParameters()
        {
            // Arrange
            var deleteResponse = new DeleteRangeResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(deleteResponse);
            _mockKvClient
                .Setup(x => x.DeleteRangeAsync(
                    It.IsAny<DeleteRangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            await _client.DeleteAsync("test-key");

            // Assert
            _mockKvClient.Verify(x => x.DeleteRangeAsync(
                It.Is<DeleteRangeRequest>(r => r.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public void DeleteRange_ShouldCallDeleteRangeWithCorrectParameters()
        {
            // Arrange
            var deleteResponse = new DeleteRangeResponse();
            _mockKvClient
                .Setup(x => x.DeleteRange(
                    It.IsAny<DeleteRangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(deleteResponse);

            // Act
            _client.DeleteRange("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.DeleteRange(
                It.Is<DeleteRangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task DeleteRangeAsync_ShouldCallDeleteRangeAsyncWithCorrectParameters()
        {
            // Arrange
            var deleteResponse = new DeleteRangeResponse();
            var asyncCall = AsyncUnaryCallFactory.Create(deleteResponse);
            _mockKvClient
                .Setup(x => x.DeleteRangeAsync(
                    It.IsAny<DeleteRangeRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(asyncCall);

            // Act
            await _client.DeleteRangeAsync("test-prefix");

            // Assert
            _mockKvClient.Verify(x => x.DeleteRangeAsync(
                It.Is<DeleteRangeRequest>(r => 
                    r.Key.ToStringUtf8() == "test-prefix" && 
                    r.RangeEnd.ToStringUtf8() == "test-prefiy"),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
