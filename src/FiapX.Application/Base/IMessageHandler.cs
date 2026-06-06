namespace FiapX.Application.Base;

public interface IMessageHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}
