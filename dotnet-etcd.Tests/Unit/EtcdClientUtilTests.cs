using System.Reflection;
using Etcdserverpb;
using Google.Protobuf;
using Mvccpb;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class EtcdClientUtilTests
{
    [Fact]
    public void GetRangeEnd_WithEmptyPrefix_ShouldReturnNullChar()
    {
        // Act
        var result = EtcdClient.GetRangeEnd("");

        // Assert
        Assert.Equal("\0", result);
    }

    [Theory]
    [InlineData("a", "b")]
    [InlineData("test", "tesu")]
    [InlineData("z", "{")]
    [InlineData("foo/bar", "foo/bas")]
    public void GetRangeEnd_WithNonEmptyPrefix_ShouldIncrementLastChar(string prefix, string expected)
    {
        // Act
        var result = EtcdClient.GetRangeEnd(prefix);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("key")]
    [InlineData("prefix/")]
    public void GetStringByteForRangeRequests_ShouldReturnCorrectByteString(string key)
    {
        // Act
        var result = EtcdClient.GetStringByteForRangeRequests(key);

        // Assert
        if (key.Length == 0)
            Assert.Equal(ByteString.CopyFrom(0), result);
        else
            Assert.Equal(ByteString.CopyFromUtf8(key), result);
    }

    [Fact]
    public void RangeRespondToDictionary_WithEmptyResponse_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var response = new RangeResponse();

        // Act
        var result = GetRangeRespondToDictionary(response);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void RangeRespondToDictionary_WithKeyValues_ShouldReturnPopulatedDictionary()
    {
        // Arrange
        var response = new RangeResponse
        {
            Kvs =
            {
                new KeyValue
                {
                    Key = ByteString.CopyFromUtf8("key1"),
                    Value = ByteString.CopyFromUtf8("value1")
                },
                new KeyValue
                {
                    Key = ByteString.CopyFromUtf8("key2"),
                    Value = ByteString.CopyFromUtf8("value2")
                }
            }
        };

        // Act
        var result = GetRangeRespondToDictionary(response);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    // Helper method to access the private RangeRespondToDictionary method via reflection
    private Dictionary<string, string> GetRangeRespondToDictionary(RangeResponse response)
    {
        var method = typeof(EtcdClient).GetMethod("RangeRespondToDictionary",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null) return new Dictionary<string, string>();

        return (Dictionary<string, string>)method.Invoke(null, new object[] { response })!;
    }
}