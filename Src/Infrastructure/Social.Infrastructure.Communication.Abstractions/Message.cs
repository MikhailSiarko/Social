namespace Social.Infrastructure.Communication.Abstractions;

public abstract class Message
{
    public abstract Guid CorrelationId { get; protected set; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}