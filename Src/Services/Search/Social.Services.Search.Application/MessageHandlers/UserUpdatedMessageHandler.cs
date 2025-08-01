using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;
using Social.Services.Search.Application.Models;
using Social.Services.Shared.Messages;
using Social.Shared;

namespace Social.Services.Search.Application.MessageHandlers;

public sealed class UserUpdatedMessageHandler(
    ILogger<UserCreatedMessageHandler> logger,
    ISearchRepository repository) : IMessageHandler<UserUpdated>
{
    public async Task<Result<Unit>> HandleAsync(UserUpdated message, CancellationToken cancellationToken = default)
    {
        var result = await repository.UpdateAsync(new User
        {
            Id = message.UserId,
            FirstName = message.FirstName,
            LastName = message.LastName,
            UserName = message.UserName
        }, cancellationToken);

        if (result.IsError)
            logger.LogError(result.Error);

        return result;
    }
}