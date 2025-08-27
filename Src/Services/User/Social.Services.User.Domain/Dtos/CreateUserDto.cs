namespace Social.Services.User.Domain.Dtos;

public sealed class CreateUserDto(string email)
{
    public string Email { get; } = email;
    public readonly Guid UserId = Guid.CreateVersion7();
}