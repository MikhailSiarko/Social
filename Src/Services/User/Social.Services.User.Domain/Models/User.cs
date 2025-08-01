namespace Social.Services.User.Domain.Models;

public sealed class User : UpdateUser
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingsCount { get; set; }
}