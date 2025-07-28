namespace Social.Services.User.Persistence.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? AvatarUrl { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Followers { get; set; }
    public int Followings { get; set; }
}