using System;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd.interfaces;
using Etcdserverpb;
using Grpc.Core;
using Moq;
using Xunit;

namespace dotnet_etcd.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class EtcdClientWatchTests
    {
        [Fact]
        public async Task WatchAsync_ShouldReturnWatchId()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            mockWatchManager
                .Setup(x => x.WatchAsync(
                    It.IsAny<WatchRequest>(),
                    It.IsAny<Action<WatchResponse>>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(123);

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act - explicitly cast the delegate to resolve ambiguity
            var watchId = await client.WatchAsync(
                new WatchRequest { CreateRequest = new WatchCreateRequest { Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key") } },
                (Action<WatchResponse>)(response => { }));

            // Assert
            Assert.Equal(123, watchId);
            mockWatchManager.Verify(x => x.WatchAsync(
                It.Is<WatchRequest>(r => r.CreateRequest.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Watch_ShouldDelegateToWatchManager()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            mockWatchManager
                .Setup(x => x.Watch(
                    It.IsAny<WatchRequest>(),
                    It.IsAny<Action<WatchResponse>>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(123);

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act - explicitly cast the delegate to resolve ambiguity
            client.Watch(
                new WatchRequest { CreateRequest = new WatchCreateRequest { Key = Google.Protobuf.ByteString.CopyFromUtf8("test-key") } },
                (Action<WatchResponse>)(response => { }));

            // Assert
            mockWatchManager.Verify(x => x.Watch(
                It.Is<WatchRequest>(r => r.CreateRequest.Key.ToStringUtf8() == "test-key"),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void CancelWatch_ShouldDelegateToWatchManager()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act
            client.CancelWatch(123);

            // Assert
            mockWatchManager.Verify(x => x.CancelWatch(123), Times.Once);
        }

        [Fact]
        public void CancelWatch_WithArray_ShouldDelegateToWatchManager()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act
            client.CancelWatch(new long[] { 123, 456 });

            // Assert
            mockWatchManager.Verify(x => x.CancelWatch(123), Times.Once);
            mockWatchManager.Verify(x => x.CancelWatch(456), Times.Once);
        }

        [Fact]
        public void Dispose_ShouldDisposeWatchManager()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act
            client.Dispose();

            // Assert
            mockWatchManager.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void WatchRange_ShouldDelegateToWatchManager()
        {
            // Arrange
            var mockCallInvoker = new Mock<CallInvoker>();
            var mockWatchManager = new Mock<IWatchManager>();

            // Setup the mock to return a specific watch ID
            long expectedWatchId = 123;
            mockWatchManager.Setup(x => x.Watch(
                It.IsAny<WatchRequest>(),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
                .Returns(expectedWatchId);

            // Use reflection to set the private _watchManager field
            var client = new EtcdClient(mockCallInvoker.Object);
            var watchManagerField = typeof(EtcdClient).GetField("_watchManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            watchManagerField.SetValue(client, mockWatchManager.Object);

            // Act - explicitly cast the delegate to resolve ambiguity
            long watchId = client.WatchRange(
                "test-key",
                (Action<WatchResponse>)(response => { }));

            // Assert
            Assert.Equal(expectedWatchId, watchId);
            mockWatchManager.Verify(x => x.Watch(
                It.Is<WatchRequest>(r => 
                    r.CreateRequest != null && 
                    r.CreateRequest.Key.ToStringUtf8() == "test-key" &&
                    r.CreateRequest.RangeEnd != null),
                It.IsAny<Action<WatchResponse>>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 