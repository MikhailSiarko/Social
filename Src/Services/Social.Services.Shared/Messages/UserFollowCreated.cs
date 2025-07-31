using Social.Infrastructure.Communication.Abstractions;

namespace Social.Services.Shared.Messages;

public sealed class UserFollowCreated(Guid userId, Guid followsToUserId) : Message
{
    public Guid UserId { get; } = userId;
    public Guid FollowsToUserId { get; } = followsToUserId;
    public override Guid CorrelationId { get; protected set; } = userId;
}