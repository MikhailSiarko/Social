namespace Social.Services.Search.Application.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}