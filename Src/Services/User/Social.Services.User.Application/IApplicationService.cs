using Social.Services.User.Application.Models;
using Social.Shared;

namespace Social.Services.User.Application;

public interface IApplicationService
{
    Task<Result<UserModel>> CreateUserAsync(string email, CancellationToken token = default);
    Task<Result<UserModel>> GetUserAsync(string email, CancellationToken token = default);
    Task<Result<UserModel>> GetUserAsync(Guid id, CancellationToken token = default);
    Task<Result<Unit>> FollowUserAsync(Guid userId, Guid followToUserId, CancellationToken token = default);
    Task<Result<Unit>> UnfollowUserAsync(Guid userId, Guid unfollowToUserId, CancellationToken token = default);
    Task<Result<Unit>> UpdateUserAsync(Guid userId, PatchUserModel model, CancellationToken token = default);
}