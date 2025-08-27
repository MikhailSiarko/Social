using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Validators;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Services;

internal class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<Result<Models.User>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var emailExistsResult = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (emailExistsResult is { IsOk: true, Value: not null })
            return new Error("User with this email already exists: {Message}", dto.Email);

        if (emailExistsResult.Error is Failure failure)
            return failure;

        var user = new Models.User { Id = dto.UserId, Email = dto.Email };
        var result = await userRepository.AddAsync(user, cancellationToken);
        if (result.IsError)
            return result.Error;
        return user;
    }

    public async Task<Result<Unit>> UpdateUserAsync(UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = new UpdateUser
        {
            Id = dto.UserId, UserName = dto.UserName, FirstName = dto.FirstName, LastName = dto.LastName
        };

        var result = await userRepository.UpdateAsync(user, cancellationToken);
        if (result.IsError)
            return result.Error;

        return Unit.Value;
    }

    public Task<Result<Models.User>> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return userRepository.GetAsync(userId, cancellationToken);
    }

    public async Task<Result<Models.User>> GetUserAsync(GetUserByEmailDto dto, CancellationToken cancellationToken)
    {
        var validator = new GetUserByEmailModelValidator();
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        return await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
    }
}