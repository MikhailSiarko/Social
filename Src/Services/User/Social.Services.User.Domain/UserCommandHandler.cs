using MediatR;
using Social.Services.User.Domain.Commands;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Validators;
using Social.Shared;
using Social.Shared.Errors;
using Unit = Social.Shared.Unit;

namespace Social.Services.User.Domain;

public sealed class UserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, Result<Models.User>>,
        IRequestHandler<UpdateUserCommand, Result<Unit>>
{
    public async Task<Result<Models.User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        var emailExistsResult = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (emailExistsResult is { IsOk: true, Value: not null })
            return new Error("User with this email already exists: {Message}", request.Email);

        if (emailExistsResult.Error is Failure failure)
            return failure;

        var user = new Models.User
            { Id = request.UserId, Email = request.Email, Password = request.Password };
        var result = await userRepository.AddAsync(user, cancellationToken);
        if (result.IsError)
            return result.Error;
        return user;
    }

    public async Task<Result<Unit>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new Models.UpdateUser
        {
            Id = request.UserId, UserName = request.UserName, FirstName = request.FirstName, LastName = request.LastName
        };
        var result = await userRepository.UpdateAsync(user, cancellationToken);
        if (result.IsError)
            return result.Error;

        return Unit.Value;
    }
}