using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;
using Social.Services.Shared.Messages;
using Social.Services.User.Application.Models;
using Social.Services.User.Domain.Commands;
using Social.Services.User.Domain.Queries;
using Social.Shared;
using Social.Shared.Errors;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Application;

public sealed class ApplicationService(
    IMediator mediator,
    IHttpContextAccessor httpContextAccessor,
    IAuthenticationService authService,
    ILogger<ApplicationService> logger,
    [FromKeyedServices("user")] IServiceBus serviceBus) : IApplicationService
{
    public async Task<Result<AuthUserModel>> RegisterUserAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var registeredUserResult = await mediator.Send(new CreateUserCommand(email, hashedPassword), cancellationToken);
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

        var token = authService.Authenticate(registeredUser);
        var userModel = new UserModel(
            registeredUser.Id,
            registeredUser.Email,
            registeredUser.UserName,
            registeredUser.FirstName,
            registeredUser.LastName);

        return new AuthUserModel(token, userModel);
    }

    public async Task<Result<AuthUserModel>> LoginUserAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var getUserResult = await mediator.Send(new GetUserByEmailQuery(email), cancellationToken);
        if (getUserResult.IsError)
        {
            logger.LogError(getUserResult.Error);
            return getUserResult.Error;
        }

        var user = getUserResult.Value!;
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            var error = new ValidationError("Invalid password");
            logger.LogError(error);
            return error;
        }

        var token = authService.Authenticate(user);
        var userModel = new UserModel(
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.FollowersCount,
            user.FollowingsCount);

        return new AuthUserModel(token, userModel);
    }

    public async Task<Result<Unit>> FollowUserAsync(Guid followToUserId, CancellationToken token = default)
    {
        var userIdClaim =
            httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            var error = new Error("Cannot get user ID from http context");
            logger.LogError(error);
            return error;
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var result = await mediator.Send(new CreateUserFollowCommand(userId, followToUserId), token);
        if (!result.IsError)
        {
            await serviceBus.PublishAsync(new UserFollowCreated(userId, followToUserId), token);
            return Unit.Value;
        }

        logger.LogError(result.Error);
        return result.Error;
    }

    public async Task<Result<Unit>> UnfollowUserAsync(Guid unfollowToUserId, CancellationToken token = default)
    {
        var userIdClaim =
            httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            var error = new Error("Cannot get user ID from http context");
            logger.LogError(error);
            return error;
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var result = await mediator.Send(new DeleteUserFollowCommand(userId, unfollowToUserId), token);
        if (!result.IsError)
        {
            await serviceBus.PublishAsync(new UserFollowDeleted(userId, unfollowToUserId), token);
            return Unit.Value;
        }

        logger.LogError(result.Error);
        return result.Error;
    }
}