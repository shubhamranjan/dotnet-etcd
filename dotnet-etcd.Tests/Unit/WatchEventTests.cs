using static Mvccpb.Event.Types;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class WatchEventTests
{
    [Fact]
    public void Constructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var watchEvent = new WatchEvent();

        // Assert
        Assert.Null(watchEvent.Key);
        Assert.Null(watchEvent.Value);
        Assert.Equal(EventType.Put, watchEvent.Type); // Default value for EventType enum is Put (0)
    }

    [Theory]
    [InlineData("test-key", "test-value", EventType.Put)]
    [InlineData("another-key", "", EventType.Delete)]
    [InlineData("", "some-value", EventType.Put)]
    [InlineData("prefix/key", null, EventType.Delete)]
    public void Properties_ShouldSetAndGetCorrectly(string key, string value, EventType type)
    {
        // Arrange
        var watchEvent = new WatchEvent();

        // Act
        watchEvent.Key = key;
        watchEvent.Value = value;
        watchEvent.Type = type;

        // Assert
        Assert.Equal(key, watchEvent.Key);
        Assert.Equal(value, watchEvent.Value);
        Assert.Equal(type, watchEvent.Type);
    }

    [Fact]
    public void Type_ShouldAcceptAllEventTypeValues()
    {
        // Arrange
        var watchEvent = new WatchEvent();

        // Act & Assert - Test all possible enum values
        watchEvent.Type = EventType.Put;
        Assert.Equal(EventType.Put, watchEvent.Type);

        watchEvent.Type = EventType.Delete;
        Assert.Equal(EventType.Delete, watchEvent.Type);
    }
}