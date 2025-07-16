namespace Social.Infrastructure.Communication.Abstractions;

public abstract class Message
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}