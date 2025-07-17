namespace Social.Infrastructure.Communication.Abstractions;

public interface IMessageHandler<in TMessage> where TMessage : Message
{
    Task HandleAsync(TMessage message);
}