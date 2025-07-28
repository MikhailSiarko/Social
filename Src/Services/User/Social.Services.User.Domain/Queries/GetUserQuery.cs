using MediatR;
using Social.Shared;

namespace Social.Services.User.Domain.Queries;

public sealed class GetUserQuery(Guid id) : IRequest<Result<Models.User>>
{
    public Guid Id { get; } = id;
}