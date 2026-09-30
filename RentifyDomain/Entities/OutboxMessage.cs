namespace RentifyDomain.Entities;

public sealed class OutboxMessage
{
    public long Id { get; set; }
    public int Type { get; set; }
    public int AggregateType { get; set; }
    public long? AggregateId { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}