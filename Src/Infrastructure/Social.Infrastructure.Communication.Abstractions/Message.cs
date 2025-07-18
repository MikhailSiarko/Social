namespace Social.Infrastructure.Communication.Abstractions;

public abstract class Message
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}