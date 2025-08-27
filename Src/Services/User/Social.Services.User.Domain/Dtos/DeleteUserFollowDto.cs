namespace Social.Services.User.Domain.Dtos;

public sealed class DeleteUserFollowDto(Guid userId, Guid followsToUserId)
{
    public Guid UserId { get; } = userId;
    public Guid FollowsToUserId { get; } = followsToUserId;
}
