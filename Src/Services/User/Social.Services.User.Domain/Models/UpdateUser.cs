namespace Social.Services.User.Domain.Models;

public class UpdateUser
{
    public Guid Id { get; set; }
    public string? AvatarUrl { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}