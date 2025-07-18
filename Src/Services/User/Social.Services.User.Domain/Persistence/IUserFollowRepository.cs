using Social.Shared;

namespace Social.Services.User.Domain.Persistence;

public interface IUserFollowRepository
{
    Task<Result<Unit>> AddAsync(Guid userId, Guid followedByUserId, CancellationToken cancellationToken = default);
    Task<Result<Unit>> DeleteAsync(Guid userId, Guid followedByUserId, CancellationToken cancellationToken = default);
}