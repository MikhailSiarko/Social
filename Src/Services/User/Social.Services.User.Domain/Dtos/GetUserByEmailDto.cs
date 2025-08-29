namespace Social.Services.User.Domain.Dtos;

public sealed class GetUserByEmailDto(string email)
{
    public string Email { get; } = email;
}