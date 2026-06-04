using FiapX.Domain.Handlers;
using Microsoft.Extensions.Logging;

namespace FiapX.Application.Handlers;

public class NotificationHandler(ILogger<NotificationHandler> logger) : IMessageHandler
{
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Notification handler started");
        await Task.Delay(2000, cancellationToken);
    }
}
