namespace Social.Services.User.Persistence.Models;

public sealed class UserFollow
{
    public string Id => $"{UserId}:{FollowsToUserId}";
    public Guid UserId { get; set; }
    public Guid FollowsToUserId { get; set; }
    public DateTime StartedFollowAt { get; set; }
}