using MailKit.Net.Smtp;
using MimeKit;

namespace FiapX.Infra.EmailSender;

public class EmailSenderService(EmailSenderOptions options) : IEmailSenderService
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
            return;

        using var client = new SmtpClient();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            await client.ConnectAsync(options.SmtpServer, options.SmtpPort, options.SslRequired, cts.Token);
            if (!string.IsNullOrWhiteSpace(options.UserName) && !string.IsNullOrWhiteSpace(options.Password))
                await client.AuthenticateAsync(options.UserName, options.Password, cts.Token);

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(options.SenderInformation.Name, options.SenderInformation.Address));
            mimeMessage.To.Add(new MailboxAddress("", message.Recipient));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new BodyBuilder { HtmlBody = message.Body }.ToMessageBody();

            await client.SendAsync(mimeMessage, cts.Token);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
