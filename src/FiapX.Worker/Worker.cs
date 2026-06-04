using FiapX.Application.Handlers;

namespace FiapX.Worker;

public class Worker(NotificationHandler handler, IHostApplicationLifetime lifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);
        lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
