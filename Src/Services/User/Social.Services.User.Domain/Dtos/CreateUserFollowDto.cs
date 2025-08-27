namespace Social.Services.User.Domain.Dtos;

public sealed class CreateUserFollowDto(Guid userId, Guid followsToUserId)
{
    public Guid UserId { get; } = userId;
    public Guid FollowsToUserId { get; } = followsToUserId;
}