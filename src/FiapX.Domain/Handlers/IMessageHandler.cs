namespace FiapX.Domain.Handlers;

public interface IMessageHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}
