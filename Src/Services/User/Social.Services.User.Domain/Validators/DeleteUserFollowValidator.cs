using FluentValidation;
using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;

namespace Social.Services.User.Domain.Validators;

public sealed class DeleteUserFollowValidator : AbstractValidator<DeleteUserFollowDto>
{
    public DeleteUserFollowValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FollowsToUserId).NotEmpty();
    }
}
