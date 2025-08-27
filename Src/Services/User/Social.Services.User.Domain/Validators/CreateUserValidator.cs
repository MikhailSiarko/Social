using FluentValidation;
using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;

namespace Social.Services.User.Domain.Validators;

public sealed class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().NotNull().EmailAddress();
        RuleFor(x => x.UserId).NotEmpty();
    }
}