namespace Social.Services.User.Application.Models;

public class CreateUserModel(string email)
{
    public string Email { get; } = email;
}