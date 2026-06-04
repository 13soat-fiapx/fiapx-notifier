using FiapX.Infra.Messaging.Models;

namespace FiapX.Infra.Messaging.Publishers;

public interface IMessagePublisher
{
    Task PublishAsync<T, TEvent>(T message, CancellationToken cancellationToken = default)
        where T : MessageBase<TEvent> where TEvent : class, new();
}
