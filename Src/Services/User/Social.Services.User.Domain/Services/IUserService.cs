using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;
using Social.Shared;

namespace Social.Services.User.Domain.Services;

public interface IUserService
{
    Task<Result<Models.User>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<Result<Unit>> UpdateUserAsync(UpdateUserDto dto, CancellationToken cancellationToken);
    Task<Result<Models.User>> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<Models.User>> GetUserAsync(GetUserByEmailDto dto, CancellationToken cancellationToken);
}