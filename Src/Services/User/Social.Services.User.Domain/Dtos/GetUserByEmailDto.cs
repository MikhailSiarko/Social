namespace Social.Services.User.Domain.Dtos;

public class GetUserByEmailDto(string email)
{
    public string Email { get; } = email;
}