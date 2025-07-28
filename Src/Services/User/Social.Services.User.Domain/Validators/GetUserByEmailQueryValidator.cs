using FluentValidation;
using Social.Services.User.Domain.Queries;

namespace Social.Services.User.Domain.Validators;

public sealed class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().NotNull().EmailAddress();
    }
}