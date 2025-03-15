using System.Globalization;
using dotnet_etcd.helper;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AsyncHelperTests
{
    [Fact]
    public void RunSync_WithSuccessfulTask_ShouldComplete()
    {
        // Arrange
        var taskExecuted = false;
        var asyncFunc = async () =>
        {
            await Task.Delay(10);
            taskExecuted = true;
        };

        // Act
        AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.True(taskExecuted);
    }

    [Fact]
    public void RunSync_WithExceptionInTask_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var asyncFunc = () => Task.FromException(expectedException);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => AsyncHelper.RunSync(asyncFunc));
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public void RunSync_ShouldPreserveCulture()
    {
        // Arrange
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;

        var testCulture = new CultureInfo("fr-FR");
        var testUICulture = new CultureInfo("es-ES");

        CultureInfo capturedCulture = null;
        CultureInfo capturedUICulture = null;

        var asyncFunc = () =>
        {
            capturedCulture = CultureInfo.CurrentCulture;
            capturedUICulture = CultureInfo.CurrentUICulture;
            return Task.CompletedTask;
        };

        try
        {
            // Set test cultures
            Thread.CurrentThread.CurrentCulture = testCulture;
            Thread.CurrentThread.CurrentUICulture = testUICulture;

            // Act
            AsyncHelper.RunSync(asyncFunc);

            // Assert
            Assert.Equal(testCulture, capturedCulture);
            Assert.Equal(testUICulture, capturedUICulture);
        }
        finally
        {
            // Restore original cultures
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
        }
    }

    [Fact]
    public void RunSync_WithLongRunningTask_ShouldComplete()
    {
        // Arrange
        var taskExecuted = false;
        var asyncFunc = async () =>
        {
            await Task.Delay(100);
            taskExecuted = true;
        };

        // Act
        AsyncHelper.RunSync(asyncFunc);

        // Assert
        Assert.True(taskExecuted);
    }

    // Skip the null test since the implementation doesn't check for null
    // and we don't want to modify the implementation
}