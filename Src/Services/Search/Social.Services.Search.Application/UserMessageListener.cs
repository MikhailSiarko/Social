using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Infrastructure.Communication.Abstractions;
using Social.Services.Shared.Messages;

namespace Social.Services.Search.Application;

public sealed class UserMessageListener(
    [FromKeyedServices("user")] IServiceBus serviceBus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            serviceBus.SubscribeAsync<UserCreated, IMessageHandler<UserCreated>>(stoppingToken),
            serviceBus.SubscribeAsync<UserUpdated, IMessageHandler<UserUpdated>>(stoppingToken));
    }
}