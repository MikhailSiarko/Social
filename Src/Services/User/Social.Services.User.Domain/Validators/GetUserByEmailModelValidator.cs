using FluentValidation;
using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;

namespace Social.Services.User.Domain.Validators;

public sealed class GetUserByEmailModelValidator : AbstractValidator<GetUserByEmailDto>
{
    public GetUserByEmailModelValidator()
    {
        RuleFor(x => x.Email).NotEmpty().NotNull().EmailAddress();
    }
}