using FiapX.Domain.Events;
using FiapX.Infra.Messaging.Consumers;

namespace FiapX.Application.Consumers;

public class VideoProcessingCompletedEventConsumer : IEventConsumer<VideoProcessingCompletedEvent>
{
    public Task ConsumeAsync(VideoProcessingCompletedEvent message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
