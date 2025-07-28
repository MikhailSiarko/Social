using Social.Services.User.Application.Models;
using Social.Shared;

namespace Social.Services.User.Application;

public interface IApplicationService
{
    Task<Result<AuthUserModel>> RegisterUserAsync(string email, string password, CancellationToken token = default);
    Task<Result<AuthUserModel>> LoginUserAsync(string email, string password, CancellationToken token = default);
    Task<Result<Unit>> FollowUserAsync(Guid followToUserId, CancellationToken token = default);
    Task<Result<Unit>> UnfollowUserAsync(Guid unfollowToUserId, CancellationToken token = default);
}