using System;
using Grpc.Core;
using Xunit;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class ConnectionStringParserTests
{
    private readonly ConnectionStringParser _parser;

    public ConnectionStringParserTests()
    {
        _parser = new ConnectionStringParser();
    }

    [Fact]
    public void ParseConnectionString_WithNullString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseConnectionString(null, 2379, ChannelCredentials.Insecure));
    }

    [Fact]
    public void ParseConnectionString_WithEmptyString_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.ParseConnectionString(string.Empty, 2379, ChannelCredentials.Insecure));
    }

    [Fact]
    public void ParseConnectionString_WithDnsPrefix_ShouldReturnDnsConnection()
    {
        // Arrange
        var connectionString = "dns://localhost";

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2379, ChannelCredentials.Insecure);

        // Assert
        Assert.True(isDnsConnection);
        Assert.Single(uris);
        Assert.Equal("dns://localhost/", uris[0].ToString());
    }

    [Fact]
    public void ParseConnectionString_WithAlternateDnsPrefix_ShouldConvertToDnsPrefix()
    {
        // Arrange
        var connectionString = "discovery-srv://localhost";

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2379, ChannelCredentials.Insecure);

        // Assert
        Assert.True(isDnsConnection);
        Assert.Single(uris);
        Assert.Equal("dns://localhost/", uris[0].ToString());
    }

    [Fact]
    public void ParseConnectionString_WithSingleHost_ShouldReturnCorrectUri()
    {
        // Arrange
        var connectionString = "localhost";

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2379, ChannelCredentials.Insecure);

        // Assert
        Assert.False(isDnsConnection);
        Assert.Single(uris);
        Assert.Equal("http://localhost:2379/", uris[0].ToString());
    }

    [Fact]
    public void ParseConnectionString_WithMultipleHosts_ShouldReturnCorrectUris()
    {
        // Arrange
        var connectionString = "localhost,127.0.0.1";

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2379, ChannelCredentials.Insecure);

        // Assert
        Assert.False(isDnsConnection);
        Assert.Equal(2, uris.Length);
        Assert.Equal("http://localhost:2379/", uris[0].ToString());
        Assert.Equal("http://127.0.0.1:2379/", uris[1].ToString());
    }

    [Fact]
    public void ParseConnectionString_WithFullUrl_ShouldPreserveUrl()
    {
        // Arrange
        var connectionString = "https://localhost:2379";

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2380, ChannelCredentials.Insecure);

        // Assert
        Assert.False(isDnsConnection);
        Assert.Single(uris);
        Assert.Equal("https://localhost:2379/", uris[0].ToString());
    }

    [Fact]
    public void ParseConnectionString_WithSecureCredentials_ShouldUseHttps()
    {
        // Arrange
        var connectionString = "localhost";
        var credentials = new SslCredentials();

        // Act
        var (uris, isDnsConnection) = _parser.ParseConnectionString(connectionString, 2379, credentials);

        // Assert
        Assert.False(isDnsConnection);
        Assert.Single(uris);
        Assert.Equal("https://localhost:2379/", uris[0].ToString());
    }
} 