using Social.Shared;
using DomainUser = Social.Services.User.Domain.Models.User;

namespace Social.Services.User.Domain.Persistence;

public interface IUserRepository : IDisposable
{
    Task<Result<DomainUser>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DomainUser>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<Unit>> AddAsync(DomainUser? user, CancellationToken cancellationToken = default);
    Task<Result<Unit>> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<Unit>> UpdateFollowInfoAsync(Guid userId, Guid followsToUserId, bool unfollow = false,
        CancellationToken cancellationToken = default);
}