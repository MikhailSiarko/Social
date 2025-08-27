using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;
using Social.Services.Shared.Messages;
using Social.Services.User.Application.Models;
using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Services;
using Social.Shared;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Application;

public sealed class ApplicationService(
    IUserService userService,
    IUserFollowService userFollowService,
    ILogger<ApplicationService> logger,
    [FromKeyedServices("user")] IServiceBus serviceBus) : IApplicationService
{
    public async Task<Result<UserModel>> CreateUserAsync(string email, CancellationToken cancellationToken = default)
    {
        var registeredUserResult = await userService.CreateUserAsync(new CreateUserDto(email), cancellationToken);
        if (registeredUserResult.IsError)
        {
            logger.LogError(registeredUserResult.Error);
            return registeredUserResult.Error;
        }

        var registeredUser = registeredUserResult.Value;
        var publishResult = await serviceBus.PublishAsync(new UserCreated(registeredUser.Id, registeredUser.Email),
            cancellationToken);
        if (!publishResult.IsOk)
            logger.LogError(publishResult.Error);

        return new UserModel(
            registeredUser.Id,
            registeredUser.Email,
            registeredUser.UserName,
            registeredUser.FirstName,
            registeredUser.LastName);
    }

    public async Task<Result<UserModel>> GetUserAsync(string email, CancellationToken cancellationToken = default)
    {
        var getUserResult = await userService.GetUserAsync(new GetUserByEmailDto(email), cancellationToken);
        if (getUserResult.IsError)
        {
            logger.LogError(getUserResult.Error);
            return getUserResult.Error;
        }

        var user = getUserResult.Value!;
        return new UserModel(
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.FollowersCount,
            user.FollowingsCount);
    }

    public async Task<Result<UserModel>> GetUserAsync(Guid id, CancellationToken token = default)
    {
        var getUserResult = await userService.GetUserAsync(id, token);
        if (getUserResult.IsError)
        {
            logger.LogError(getUserResult.Error);
            return getUserResult.Error;
        }

        var user = getUserResult.Value!;
        return new UserModel(
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.FollowersCount,
            user.FollowingsCount);
    }

    public async Task<Result<Unit>> FollowUserAsync(Guid userId, Guid followToUserId, CancellationToken token = default)
    {
        var result =
            await userFollowService.CreateUserFollowAsync(new CreateUserFollowDto(userId, followToUserId), token);
        if (!result.IsError)
        {
            await serviceBus.PublishAsync(new UserFollowCreated(userId, followToUserId), token);
            return Unit.Value;
        }

        logger.LogError(result.Error);
        return result.Error;
    }

    public async Task<Result<Unit>> UnfollowUserAsync(Guid userId, Guid unfollowToUserId,
        CancellationToken token = default)
    {
        var result =
            await userFollowService.DeleteUserFollowAsync(new DeleteUserFollowDto(userId, unfollowToUserId), token);
        if (!result.IsError)
        {
            await serviceBus.PublishAsync(new UserFollowDeleted(userId, unfollowToUserId), token);
            return Unit.Value;
        }

        logger.LogError(result.Error);
        return result.Error;
    }

    public async Task<Result<Unit>> UpdateUserAsync(Guid userId, PatchUserModel model, CancellationToken token = default)
    {
        var result = await userService.UpdateUserAsync(
            new UpdateUserDto(userId, model.UserName, model.FirstName, model.LastName),
            token);

        if (!result.IsError)
        {
            await serviceBus.PublishAsync(new UserUpdated(userId, model.UserName, model.FirstName, model.LastName),
                token);
            return Unit.Value;
        }

        logger.LogError(result.Error);
        return result.Error;
    }
}