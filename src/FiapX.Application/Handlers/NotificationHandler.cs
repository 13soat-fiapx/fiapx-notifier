using FiapX.Domain.Handlers;

namespace FiapX.Application.Handlers;

public class NotificationHandler : IMessageHandler
{
    public Task HandleAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
