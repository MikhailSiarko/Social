using FluentValidation;
using Social.Services.User.Domain.Commands;

namespace Social.Services.User.Domain.Validators;

public sealed class DeleteUserFollowCommandValidator : AbstractValidator<DeleteUserFollowCommand>
{
    public DeleteUserFollowCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FollowsToUserId).NotEmpty();
    }
}
