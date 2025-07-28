namespace Social.Services.User.Application.Models;

public sealed class UserFollowModel(Guid userId)
{
    public Guid UserId { get; } = userId;
}