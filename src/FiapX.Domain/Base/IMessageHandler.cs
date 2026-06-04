namespace FiapX.Domain.Base;

public interface IMessageHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}
