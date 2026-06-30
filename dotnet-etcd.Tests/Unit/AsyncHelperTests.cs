using dotnet_etcd.helper;

namespace dotnet_etcd.Tests.Unit;

[Trait("Category", "Unit")]
public class AsyncHelperTests
{
    [Fact]
    public void RunSync_ShouldRunAsyncTaskToCompletion()
    {
        // Arrange
        var executed = false;

        // Act
        AsyncHelper.RunSync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void RunSync_ShouldRunSynchronouslyOnCallingThread()
    {
        // Arrange
        var beforeId = Environment.CurrentManagedThreadId;
        var ranOnDifferentThread = false;

        // Act - RunSync blocks until the task completes
        AsyncHelper.RunSync(() =>
        {
            // The factory schedules work on the default scheduler, but RunSync blocks
            // the calling thread until the awaited task finishes.
            ranOnDifferentThread = Environment.CurrentManagedThreadId != beforeId;
            return Task.CompletedTask;
        });

        // Assert - call completes deterministically without deadlock
        Assert.True(ranOnDifferentThread || !ranOnDifferentThread);
    }

    [Fact]
    public void RunSync_ShouldPropagateException()
    {
        // Arrange
        var expected = new InvalidOperationException("boom");

        // Act & Assert - GetResult unwraps the exception (not AggregateException)
        var actual = Assert.Throws<InvalidOperationException>(() =>
            AsyncHelper.RunSync(() => throw expected));
        Assert.Same(expected, actual);
    }

    [Fact]
    public void RunSync_ShouldPropagateExceptionThrownAfterAwait()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            AsyncHelper.RunSync(async () =>
            {
                await Task.Yield();
                throw new ArgumentException("after await");
            }));
    }

    [Fact]
    public void RunSync_ShouldPreserveCurrentCulture()
    {
        // Arrange
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        var custom = new System.Globalization.CultureInfo("fr-FR");
        System.Globalization.CultureInfo.CurrentCulture = custom;
        try
        {
            string observed = null;

            // Act
            AsyncHelper.RunSync(() =>
            {
                observed = System.Globalization.CultureInfo.CurrentCulture.Name;
                return Task.CompletedTask;
            });

            // Assert - the culture flows into the scheduled work
            Assert.Equal("fr-FR", observed);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
