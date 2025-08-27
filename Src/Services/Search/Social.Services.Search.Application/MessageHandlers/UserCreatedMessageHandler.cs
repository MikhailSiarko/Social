using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;
using Social.Services.Search.Application.Models;
using Social.Services.Shared.Messages;
using Social.Shared;

namespace Social.Services.Search.Application.MessageHandlers;

public sealed class UserCreatedMessageHandler(
    ILogger<UserCreatedMessageHandler> logger,
    ISearchRepository repository) : IMessageHandler<UserCreated>
{
    public async Task<Result<Unit>> HandleAsync(UserCreated message, CancellationToken cancellationToken = default)
    {
        var result = await repository.AddAsync(new User
        {
            Id = message.UserId,
            Email = message.Email,
        }, cancellationToken);
        
        if (result.IsError)
            logger.LogError(result.Error);
        
        return result;
    }
}