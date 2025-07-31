using Social.Infrastructure.Communication.Abstractions;

namespace Social.Services.Shared.Messages;

public sealed class UserCreated(Guid userId, string email) : Message
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
    public override Guid CorrelationId { get; protected set; } = userId;
}