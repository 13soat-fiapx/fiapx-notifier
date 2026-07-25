using FiapX.Application.Services;
using FiapX.Domain.Events;
using FiapX.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FiapX.Application.Consumers;

public class VideoProcessingCompletedEventConsumer(
    ILogger<VideoProcessingCompletedEventConsumer> logger,
    NotificationAppService service) : IEventConsumer<VideoProcessingCompletedEvent>
{
    public async Task ConsumeAsync(VideoProcessingCompletedEvent message, CancellationToken cancellationToken = default)
    {
        Activity.Current?.SetTag("video.id", message.ProcessingJobId);

        logger.LogInformation("Processing video processing completed event for video with ID '{VideoId}'", message.ProcessingJobId);
        await service.SendEmailMessage(message, cancellationToken);

        logger.LogInformation("Video processing completed event processed successfully for video with ID '{VideoId}'",
            message.ProcessingJobId);
    }
}
