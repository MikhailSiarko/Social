using MediatR;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Queries;
using Social.Services.User.Domain.Validators;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Domain;

public sealed class UserQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserQuery, Result<Models.User>>,
        IRequestHandler<GetUserByEmailQuery, Result<Models.User>>
{
    public Task<Result<Models.User>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return userRepository.GetAsync(request.Id, cancellationToken);
    }

    public async Task<Result<Models.User>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetUserByEmailQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationError(validationResult.ToDictionary());

        return await userRepository.GetByEmailAsync(request.Email, cancellationToken);
    }
}