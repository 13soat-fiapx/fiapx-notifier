namespace FiapX.Infra.EmailSender;

public class EmailMessage
{
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required string Recipient { get; init; }
}
