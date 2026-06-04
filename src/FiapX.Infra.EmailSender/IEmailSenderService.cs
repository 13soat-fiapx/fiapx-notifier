namespace FiapX.Infra.EmailSender;

public interface IEmailSenderService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
