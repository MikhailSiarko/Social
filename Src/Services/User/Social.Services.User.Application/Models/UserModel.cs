namespace Social.Services.User.Application.Models;

public sealed class UserModel(
    Guid id,
    string email,
    string? userName,
    string? firstName,
    string? lastName,
    int followers = 0,
    int followings = 0)
{
    public Guid Id { get; } = id;
    public string Email { get; } = email;
    public string? UserName { get; } = userName;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public int Followers { get; } = followers;
    public int Followings { get; } = followings;
}