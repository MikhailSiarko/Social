using MediatR;
using Social.Shared;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Domain.Commands;

public sealed class UpdateUserCommand(Guid userId, string? userName, string? firstName, string? lastName)
    : IRequest<Result<Unit>>
{
    public string? UserName { get; } = userName;
    public string? FirstName { get; } = firstName;
    public string? LastName { get; } = lastName;
    public Guid UserId { get; } = userId;
}