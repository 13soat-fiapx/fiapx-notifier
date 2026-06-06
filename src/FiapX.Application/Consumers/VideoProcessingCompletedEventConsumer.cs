using FiapX.Application.Services;
using FiapX.Domain.Events;
using FiapX.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;

namespace FiapX.Application.Consumers;

public class VideoProcessingCompletedEventConsumer(
    ILogger<VideoProcessingCompletedEventConsumer> logger,
    NotificationAppService service) : IEventConsumer<VideoProcessingCompletedEvent>
{
    public async Task ConsumeAsync(VideoProcessingCompletedEvent message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing video processing completed event for video with ID '{VideoId}'", message.ProcessingJobId);
        await service.ProcessMessage(message, cancellationToken);

        logger.LogInformation("Video processing completed event processed successfully for video with ID '{VideoId}'",
            message.ProcessingJobId);
    }
}
