using FiapX.Application.Base;
using FiapX.Domain.Events;
using FiapX.Domain.Templates;
using FiapX.Infra.EmailSender;
using Microsoft.Extensions.Configuration;

namespace FiapX.Application.Services;

public class NotificationAppService(IEmailSenderService emailSender, IConfiguration configuration) : IAppService
{
    private readonly string _downloadBaseUrl = configuration.GetValue<string>("DownloadBaseUrl")?.TrimEnd('/') ??
                                               throw new InvalidOperationException("DownloadBaseUrl is not configured.");

    public async Task SendEmailMessage(VideoProcessingCompletedEvent message, CancellationToken cancellationToken)
    {
        var emailMessage = message.Status switch
        {
            "succeeded" => VideoProcessingEmailTemplate.Success(
                recipient: message.User.Email,
                userName: message.User.Name,
                downloadUrl: $"{_downloadBaseUrl}/{message.ProcessingJobId}"),

            "failed" => VideoProcessingEmailTemplate.Failure(
                recipient: message.User.Email,
                userName: message.User.Name,
                reason: message.Messages.FirstOrDefault(m => m.Severity == "error")?.Message),

            _ => throw new InvalidOperationException($"Unexpected status '{message.Status}'.")
        };

        await emailSender.SendAsync(emailMessage, cancellationToken);
    }
}
