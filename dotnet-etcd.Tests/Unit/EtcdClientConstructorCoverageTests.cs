using System.Net.Security;
using Grpc.Core;
using Moq;

namespace dotnet_etcd.Tests.Unit;

/// <summary>
///     Covers EtcdClient constructor argument validation and the secondary constructor
///     overloads (SSL options, credentials) that are otherwise only hit by integration tests.
/// </summary>
[Trait("Category", "Unit")]
public class EtcdClientConstructorCoverageTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyConnectionString_Throws(string connectionString)
    {
        Assert.Throws<ArgumentNullException>(() => new EtcdClient(connectionString));
    }

    [Fact]
    public void Constructor_WithCallInvoker_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new EtcdClient((CallInvoker)null));
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("", "pass")]
    [InlineData("   ", "pass")]
    [InlineData("user", null)]
    [InlineData("user", "")]
    [InlineData("user", "   ")]
    public void Constructor_WithCredentials_InvalidUserOrPassword_Throws(string username, string password)
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EtcdClient("127.0.0.1:2379", username, password));
    }

    [Fact]
    public void Constructor_WithCredentials_Valid_DoesNotThrow()
    {
        // Lazy auth: no etcd contact at construction time.
        using var client = new EtcdClient("127.0.0.1:2379", "user", "pass",
            tokenCacheDuration: TimeSpan.FromMinutes(2));
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithSslOptions_DoesNotThrow()
    {
        // Exercises the configureSslOptions constructor overload (SSL credentials path).
        using var client = new EtcdClient(
            "https://127.0.0.1:2379",
            configureSslOptions: (SslClientAuthenticationOptions _) => { });
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithConnectionString_DoesNotThrow()
    {
        using var client = new EtcdClient("127.0.0.1:2379");
        Assert.NotNull(client);
    }
}
