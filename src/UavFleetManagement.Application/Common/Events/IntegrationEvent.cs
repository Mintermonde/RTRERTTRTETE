namespace UavFleetManagement.Application.Common.Events;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string TraceId { get; init; } = Guid.NewGuid().ToString("N");
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public string? CausationId { get; init; }
    public string? UserId { get; init; }
    public abstract string AggregateType { get; }
    public abstract string AggregateId { get; }
    public abstract string EventType { get; }
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
    IAsyncEnumerable<IntegrationEvent> SubscribeAllAsync(CancellationToken cancellationToken = default);
}
