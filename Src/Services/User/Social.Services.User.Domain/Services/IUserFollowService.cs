using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;
using Social.Shared;

namespace Social.Services.User.Domain.Services;

public interface IUserFollowService
{
    Task<Result<Unit>> CreateUserFollowAsync(CreateUserFollowDto dto, CancellationToken cancellationToken);
    Task<Result<Unit>> DeleteUserFollowAsync(DeleteUserFollowDto dto, CancellationToken cancellationToken);
}