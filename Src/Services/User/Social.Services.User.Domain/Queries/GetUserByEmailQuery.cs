using MediatR;
using Social.Shared;

namespace Social.Services.User.Domain.Queries;

public sealed class GetUserByEmailQuery(string email) : IRequest<Result<Models.User>>
{
    public string Email { get; } = email;
}