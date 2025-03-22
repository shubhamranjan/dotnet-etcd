using dotnet_etcd.DependencyInjection;

namespace dotnet_etcd.Tests.Unit.DependencyInjection;

[Trait("Category", "Unit")]
public class EtcdClientOptionsValidatorTests
{
    [Fact]
    public void ValidateOptions_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => EtcdClientOptionsValidator.ValidateOptions(null));
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public void ValidateOptions_WithEmptyConnectionString_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "",
            Port = 2379
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Equal("options", exception.ParamName);
        Assert.Contains("ConnectionString must be provided", exception.Message);
    }

    [Fact]
    public void ValidateOptions_WithWhitespaceConnectionString_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "   ",
            Port = 2379
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Equal("options", exception.ParamName);
        Assert.Contains("ConnectionString must be provided", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void ValidateOptions_WithInvalidPort_ShouldThrowArgumentException(int port)
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "localhost",
            Port = port
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Equal("options", exception.ParamName);
        Assert.Contains($"Port must be between 1 and 65535, but was {port}", exception.Message);
    }

    [Fact]
    public void ValidateOptions_WithStaticConnectionStringAndNoServerName_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "static://localhost",
            Port = 2379,
            ServerName = ""
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Equal("options", exception.ParamName);
        Assert.Contains("ServerName must be provided when using static:// connection string format", exception.Message);
    }

    [Fact]
    public void ValidateOptions_WithStaticConnectionStringAndWhitespaceServerName_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "static://localhost",
            Port = 2379,
            ServerName = "   "
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Equal("options", exception.ParamName);
        Assert.Contains("ServerName must be provided when using static:// connection string format", exception.Message);
    }

    [Fact]
    public void ValidateOptions_WithValidOptions_ShouldNotThrow()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "localhost",
            Port = 2379
        };

        // Act & Assert
        var exception = Record.Exception(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateOptions_WithStaticConnectionStringAndValidServerName_ShouldNotThrow()
    {
        // Arrange
        var options = new EtcdClientOptions
        {
            ConnectionString = "static://localhost",
            Port = 2379,
            ServerName = "my-server"
        };

        // Act & Assert
        var exception = Record.Exception(() => EtcdClientOptionsValidator.ValidateOptions(options));
        Assert.Null(exception);
    }
}