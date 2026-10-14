using System.Threading.Channels;
using UavFleetManagement.Application.Common.Events;

namespace UavFleetManagement.Infrastructure.Outbox;

public class ChannelEventBus : IEventBus
{
    private readonly Channel<IntegrationEvent> _channel;

    public ChannelEventBus()
    {
        // Неограниченный или емкий канал для сообщений
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<IntegrationEvent>(options);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }

    public IAsyncEnumerable<IntegrationEvent> SubscribeAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
