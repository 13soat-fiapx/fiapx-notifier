using FiapX.Application.Base;
using FiapX.Application.Observability;
using FiapX.Application.Options;
using FiapX.Domain.Events;
using FiapX.Domain.Templates;
using FiapX.Infra.EmailSender;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FiapX.Application.Services;

public class NotificationAppService(IEmailSenderService emailSender, IOptions<EmailContent> contentOptions) : IAppService
{
    private readonly string _downloadBaseUrl = contentOptions.Value.DownloadBaseUrl;

    public async Task SendEmailMessage(VideoProcessingCompletedEvent message, CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        var channel = new KeyValuePair<string, object?>("channel", "email");

        try
        {
            var emailMessage = message.Status switch
            {
                "succeeded" => VideoProcessingEmailTemplate.Success(
                    recipient: message.User.Email,
                    userName: message.User.Name,
                    downloadUrl: _downloadBaseUrl,
                    contentOptions.Value.LogoUrl),

                "failed" => VideoProcessingEmailTemplate.Failure(
                    recipient: message.User.Email,
                    userName: message.User.Name,
                    reason: message.Messages.FirstOrDefault(m => m.Severity == "error")?.Message,
                    contentOptions.Value.LogoUrl),

                _ => throw new InvalidOperationException($"Unexpected status '{message.Status}'."),
            };

            await emailSender.SendAsync(emailMessage, cancellationToken);

            AppMetrics.NotificationsSent.Add(1, channel,
                new KeyValuePair<string, object?>("status", message.Status));
        }
        catch
        {
            AppMetrics.NotificationsFailed.Add(1, channel);
            throw;
        }
        finally
        {
            AppMetrics.ProcessingDurationSeconds.Record(
                Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, channel);
        }
    }
}
