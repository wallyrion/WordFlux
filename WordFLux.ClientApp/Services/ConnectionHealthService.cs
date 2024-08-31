namespace WordFLux.ClientApp.Services;

public class ConnectionHealthService(InMemoryMessageQueue messageQueue, ILogger<ConnectionHealthService> logger)
{
    public bool IsOnline { get; private set; }

    public event Action? OnStatusChanged;

    public void StatusChanged(bool isOnline)
    {
        IsOnline = isOnline;
        
        var type = isOnline ? Type.ConnectionOnline : Type.ConnectionOffline;

        try
        {
            OnStatusChanged?.Invoke();

            messageQueue.Writer.TryWrite(new ApplicationEvent(type));

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            logger.LogError(e, "Something went wrong ");
        }
    }
}