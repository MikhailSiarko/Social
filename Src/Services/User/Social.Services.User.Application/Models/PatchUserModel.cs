namespace Social.Services.User.Application.Models;

public sealed class PatchUserModel(
    string? userName,
    string? firstName,
    string? lastName)
{
    public string? UserName { get; } = userName;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
}