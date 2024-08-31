namespace WordFLux.ClientApp.Services;

public static class SemaphoreExtensions
{
    public class SemaphoreScope(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
    
    public static async Task<SemaphoreScope> CreateLockScopeAsync(this SemaphoreSlim semaphore)
    {
        var scope = new SemaphoreScope(semaphore);

        await semaphore.WaitAsync();

        return scope;
    }
}