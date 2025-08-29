using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Validators;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Services;

internal sealed class UserFollowService(
    IUserRepository userRepository,
    IUserFollowRepository userFollowRepository) : IUserFollowService
{
    public async Task<Result<Unit>> CreateUserFollowAsync(CreateUserFollowDto dto, CancellationToken cancellationToken)
    {
        var validator = new CreateUserFollowValidator();
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        if (await userRepository.ExistsAsync(dto.FollowsToUserId, cancellationToken) is { IsOk: false })
            return GetNotFoundError(dto.FollowsToUserId);

        var addResult = await userFollowRepository.AddAsync(dto.UserId, dto.FollowsToUserId, cancellationToken);
        if (addResult.IsError)
            return addResult.Error;

        return await userRepository.UpdateFollowInfoAsync(dto.UserId, dto.FollowsToUserId,
            cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> DeleteUserFollowAsync(DeleteUserFollowDto dto, CancellationToken cancellationToken)
    {
        var validator = new DeleteUserFollowValidator();
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var deleteResult = await userFollowRepository.DeleteAsync(dto.UserId, dto.FollowsToUserId, cancellationToken);
        if (deleteResult.IsError)
            return deleteResult.Error;
        
        return await userRepository.UpdateFollowInfoAsync(dto.UserId, dto.FollowsToUserId, unfollow: true, cancellationToken: cancellationToken);
    }
    
    private static NotFound GetNotFoundError(Guid userId)
    {
        return new NotFound("User with ID {UserId} not found", userId);
    }
}