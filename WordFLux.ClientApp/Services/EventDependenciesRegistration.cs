using System.Threading.Channels;

namespace WordFLux.ClientApp.Services;

public enum Type
{
    ConnectionOnline,
    ConnectionOffline
    
}

public interface IApplicationEvent
{
    public Type Type { get; }
}

public record ApplicationEvent(Type Type) : IApplicationEvent;

public sealed class InMemoryMessageQueue
{
    private readonly Channel<IApplicationEvent> _channel =
        Channel.CreateUnbounded<IApplicationEvent>();

    public ChannelReader<IApplicationEvent> Reader => _channel.Reader;

    public ChannelWriter<IApplicationEvent> Writer => _channel.Writer;
}
