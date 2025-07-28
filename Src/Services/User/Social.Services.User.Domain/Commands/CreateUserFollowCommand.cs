using MediatR;
using Social.Shared;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Domain.Commands;

public sealed class CreateUserFollowCommand(Guid userId, Guid followsToUserId) : IRequest<Result<Unit>>
{
    public Guid UserId { get; } = userId;
    public Guid FollowsToUserId { get; } = followsToUserId;
}