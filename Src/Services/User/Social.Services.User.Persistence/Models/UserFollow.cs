namespace Social.Services.User.Persistence.Models;

public sealed class UserFollow
{
    public Guid UserId { get; set; }
    public Guid FollowsToUserId { get; set; }
    public DateTime StartedFollowAt { get; set; }
}