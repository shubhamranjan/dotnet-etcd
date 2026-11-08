using dotnet_etcd.Tests.Infrastructure;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     EtcdClient.GetRangeEnd must follow etcd's own algorithm (clientv3 op.go getPrefix): operate on
///     the key's bytes, increment the last byte below 0xFF and truncate after it. Issue #319 reported
///     the old implementation incrementing UTF-16 chars instead, so "aÿ" (61 C3 BF) became "aĀ"
///     (61 C4 80) rather than 61 C3 C0.
/// </summary>
[Trait("Category", "Unit")]
public class RangeEndTests
{
    private static byte[] Bytes(params int[] b) => b.Select(x => (byte)x).ToArray();

    [Theory]
    [InlineData("a", new byte[] { 0x62 })]
    [InlineData("abc/", new byte[] { 0x61, 0x62, 0x63, 0x30 })]
    [InlineData("aÿ", new byte[] { 0x61, 0xC3, 0xC0 })]
    [InlineData("", new byte[] { 0x00 })]
    public void GetRangeEnd_ByteString_FollowsEtcdAlgorithm(string prefix, byte[] expected)
    {
        ByteString end = EtcdClient.GetRangeEnd(ByteString.CopyFromUtf8(prefix));

        Assert.Equal(expected, end.ToByteArray());
    }

    [Fact]
    public void GetRangeEnd_ByteString_Trailing0xFFBytesAreDropped()
    {
        ByteString end = EtcdClient.GetRangeEnd(ByteString.CopyFrom(Bytes(0x61, 0xFF, 0xFF)));

        Assert.Equal(Bytes(0x62), end.ToByteArray());
    }

    [Fact]
    public void GetRangeEnd_ByteString_All0xFFHasNoUpperBound()
    {
        ByteString end = EtcdClient.GetRangeEnd(ByteString.CopyFrom(Bytes(0xFF, 0xFF)));

        Assert.Equal(Bytes(0x00), end.ToByteArray());
    }

    [Fact]
    public void GetRangeEnd_ByteString_DoesNotMutateInput()
    {
        ByteString prefix = ByteString.CopyFromUtf8("abc");

        EtcdClient.GetRangeEnd(prefix);

        Assert.Equal("abc", prefix.ToStringUtf8());
    }

    [Theory]
    [InlineData("a", "b")]
    [InlineData("config/", "config0")]
    [InlineData("", "\0")]
    public void GetRangeEnd_String_ReturnsStringWhenRepresentable(string prefix, string expected)
    {
        Assert.Equal(expected, EtcdClient.GetRangeEnd(prefix));
    }

    [Theory]
    [InlineData("aÿ")]      // 61 C3 BF -> 61 C3 C0, not valid UTF-8
    [InlineData("a￿")] // EF BF BF -> EF BF C0, not valid UTF-8
    public void GetRangeEnd_String_ThrowsWhenRangeEndIsNotValidUtf8(string prefix)
    {
        var ex = Assert.Throws<ArgumentException>(() => EtcdClient.GetRangeEnd(prefix));

        Assert.Contains("GetRangeEnd(ByteString)", ex.Message);
    }

    [Fact]
    public void GetRangeEnd_NullInputs_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => EtcdClient.GetRangeEnd((string)null!));
        Assert.Throws<ArgumentNullException>(() => EtcdClient.GetRangeEnd((ByteString)null!));
    }

    [Fact]
    public void GetRange_WithNonAsciiPrefix_SendsByteCorrectRangeEnd()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse());
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        client.GetRange("aÿ");

        mock.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToStringUtf8() == "aÿ" &&
                                     r.RangeEnd.ToByteArray().SequenceEqual(Bytes(0x61, 0xC3, 0xC0))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void GetRange_WithEmptyPrefix_SendsWholeKeySpace()
    {
        var mock = new Mock<KV.KVClient>();
        mock.Setup(x => x.Range(It.IsAny<RangeRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>())).Returns(new RangeResponse());
        var client = TestHelper.CreateEtcdClientWithMockCallInvoker();
        TestHelper.SetupMockClientViaConnection(client, mock.Object, "_kvClient");

        client.GetRange("");

        // etcd's "all keys" range is key = "\0", range_end = "\0".
        mock.Verify(x => x.Range(
            It.Is<RangeRequest>(r => r.Key.ToByteArray().SequenceEqual(Bytes(0x00)) &&
                                     r.RangeEnd.ToByteArray().SequenceEqual(Bytes(0x00))),
            It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
