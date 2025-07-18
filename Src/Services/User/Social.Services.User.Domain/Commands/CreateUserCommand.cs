using MediatR;
using Social.Shared;

namespace Social.Services.User.Domain.Commands;

public sealed class CreateUserCommand(string email, string password) : IRequest<Result<Models.User>>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
    public readonly Guid UserId = Guid.CreateVersion7();
}