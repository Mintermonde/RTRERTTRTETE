namespace UavFleetManagement.Domain.Entities.Outbox;

public class OutboxEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}"; // JSONB
    public string Metadata { get; set; } = "{}"; // JSONB
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // 'Pending', 'Processing', 'Published', 'Failed'
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
