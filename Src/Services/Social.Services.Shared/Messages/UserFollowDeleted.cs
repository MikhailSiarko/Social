using Social.Infrastructure.Communication.Abstractions;

namespace Social.Services.Shared.Messages;

public class UserFollowDeleted(Guid userId, Guid followsToUserId) : Message
{
    public Guid UserId { get; set; } = userId;
    public Guid FollowsToUserId { get; set; } = followsToUserId;
    public override Guid CorrelationId { get; protected set; } = userId;
}
