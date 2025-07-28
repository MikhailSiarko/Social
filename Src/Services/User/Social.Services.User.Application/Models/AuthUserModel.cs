namespace Social.Services.User.Application.Models;

public sealed class AuthUserModel(string token, UserModel user)
{
    public string Token { get; } = token;
    public UserModel User { get; } = user;
}