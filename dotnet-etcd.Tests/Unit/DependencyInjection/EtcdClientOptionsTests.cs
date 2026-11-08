using dotnet_etcd.DependencyInjection;
using Grpc.Core;
using Grpc.Net.Client;

namespace dotnet_etcd.Tests.Unit.DependencyInjection;

[Trait("Category", "Unit")]
public class EtcdClientOptionsTests
{
    [Fact]
    public void ApplyTo_WithInsecureChannel_ShouldUseInsecureCredentials()
    {
        var options = new EtcdClientOptions { UseInsecureChannel = true };
        var channel = new GrpcChannelOptions();

        options.ApplyTo(channel);

        Assert.Same(ChannelCredentials.Insecure, channel.Credentials);
    }

    [Fact]
    public void ApplyTo_WithCallCredentials_ShouldCombineThemWithSsl()
    {
        var callCredentials = CallCredentials.FromInterceptor((_, _) => Task.CompletedTask);
        var options = new EtcdClientOptions { UseInsecureChannel = false, CallCredentials = callCredentials };
        var channel = new GrpcChannelOptions();

        options.ApplyTo(channel);

        Assert.NotNull(channel.Credentials);
        Assert.NotSame(ChannelCredentials.Insecure, channel.Credentials);
    }

    [Fact]
    public void ApplyTo_WithoutInsecureOrCallCredentials_ShouldLeaveCredentialsAlone()
    {
        var options = new EtcdClientOptions();
        var channel = new GrpcChannelOptions();

        options.ApplyTo(channel);

        Assert.Null(channel.Credentials);
    }

    [Fact]
    public void ApplyTo_ShouldInvokeConfigureChannelLast()
    {
        GrpcChannelOptions? seen = null;
        var options = new EtcdClientOptions
        {
            UseInsecureChannel = true,
            ConfigureChannel = o =>
            {
                seen = o;
                o.MaxReceiveMessageSize = 1234;
            }
        };
        var channel = new GrpcChannelOptions();

        options.ApplyTo(channel);

        Assert.Same(channel, seen);
        Assert.Equal(1234, channel.MaxReceiveMessageSize);
    }

    [Fact]
    public void ConfigureSslOptions_RoundTrips()
    {
        Action<System.Net.Security.SslClientAuthenticationOptions> configure = _ => { };
        var options = new EtcdClientOptions { ConfigureSslOptions = configure };

        Assert.Same(configure, options.ConfigureSslOptions);
    }
}
