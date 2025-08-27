namespace Social.Services.User.Domain.Dtos;

public sealed class UpdateUserDto(Guid userId, string? userName, string? firstName, string? lastName)
{
    public string? UserName { get; } = userName;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public Guid UserId { get; } = userId;
}