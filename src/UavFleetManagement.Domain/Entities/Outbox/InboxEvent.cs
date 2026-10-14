namespace UavFleetManagement.Domain.Entities.Outbox;

public class InboxEvent
{
    public Guid EventId { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
