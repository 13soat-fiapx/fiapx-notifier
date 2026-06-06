using FiapX.Application.Base;
using FiapX.Domain.Events;

namespace FiapX.Application.Services;

public class NotificationAppService : IAppService
{
    public async Task ProcessMessage(VideoProcessingCompletedEvent message, CancellationToken cancellationToken)
    {

    }
}
