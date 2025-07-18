using MediatR;
using Social.Shared;

namespace Social.Services.User.Domain.Commands;

public sealed class DeleteUserFollowCommand(Guid userId, Guid followsToUserId) : IRequest<Result<Shared.Unit>>
{
    public Guid UserId { get; set; } = userId;
    public Guid FollowsToUserId { get; set; } = followsToUserId;
}
