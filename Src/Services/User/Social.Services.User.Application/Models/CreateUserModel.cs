namespace Social.Services.User.Application.Models;

public sealed class CreateUserModel(string email)
{
    public string Email { get; } = email;
}