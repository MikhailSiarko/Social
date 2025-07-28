using Social.Shared;

namespace Social.Infrastructure.Communication.Abstractions;

public interface IMessageHandler<in TMessage> where TMessage : Message
{
    Task<Result<Unit>> HandleAsync(TMessage message);
}