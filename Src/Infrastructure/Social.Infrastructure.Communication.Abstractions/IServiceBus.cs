using Social.Shared;

namespace Social.Infrastructure.Communication.Abstractions;

public interface IServiceBus
{
    Task<Result<Unit>> PublishAsync(Message message, CancellationToken cancellationToken = default);

    Task<Result<Unit>> SubscribeAsync<TMessage, TMessageHandler>(CancellationToken cancellationToken = default)
        where TMessageHandler : IMessageHandler<TMessage>
        where TMessage : Message;
}