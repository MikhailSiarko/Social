using MediatR;
using Social.Services.User.Domain.Commands;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Validators;
using Social.Shared;
using Social.Shared.Errors;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Domain;

public sealed class UserFollowCommandHandler(IUserFollowRepository userFollowRepository, IUserRepository userRepository)
    : IRequestHandler<CreateUserFollowCommand, Result<Unit>>,
        IRequestHandler<DeleteUserFollowCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CreateUserFollowCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserFollowCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        if (await userRepository.ExistsAsync(request.FollowsToUserId, cancellationToken) is { IsOk: false })
            return GetNotFoundError(request.FollowsToUserId);

        var addResult = await userFollowRepository.AddAsync(request.UserId, request.FollowsToUserId, cancellationToken);
        if (addResult.IsError)
            return addResult.Error;

        return await userRepository.UpdateFollowInfoAsync(request.UserId, request.FollowsToUserId, cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> Handle(DeleteUserFollowCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteUserFollowCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var deleteResult = await userFollowRepository.DeleteAsync(request.UserId, request.FollowsToUserId, cancellationToken);
        if (deleteResult.IsError)
            return deleteResult.Error;
        
        return await userRepository.UpdateFollowInfoAsync(request.UserId, request.FollowsToUserId, unfollow: true, cancellationToken: cancellationToken);
    }

    private static NotFound GetNotFoundError(Guid userId)
    {
        return new NotFound("User with ID {UserId} not found", userId);
    }
}