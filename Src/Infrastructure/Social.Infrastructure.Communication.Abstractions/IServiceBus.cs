namespace Social.Infrastructure.Communication.Abstractions;

public interface IServiceBus
{
    Task PublishAsync(Message message, CancellationToken cancellationToken = default);
    Task SubscribeAsync<TMessage, TMessageHandler>(CancellationToken cancellationToken = default)
        where TMessageHandler : IMessageHandler<TMessage>
        where TMessage : Message;
}