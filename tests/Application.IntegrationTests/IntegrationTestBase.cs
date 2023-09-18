namespace SearchService.Application.IntegrationTests;

using AutoFixture;
using Nito.AsyncEx;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private static readonly AsyncLock Mutex = new();

    private static bool initialized;

    protected Fixture Fixture { get; set; } = new Fixture();

    public virtual async Task InitializeAsync()
    {
        if (initialized)
        {
            return;
        }

        using (await Mutex.LockAsync())
        {
            if (initialized)
            {
                return;
            }

            //await SliceFixture.ResetCheckpointAsync();
            initialized = true;
        }
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
