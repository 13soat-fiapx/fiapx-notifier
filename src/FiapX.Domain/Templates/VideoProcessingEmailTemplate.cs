using FiapX.Infra.EmailSender;

namespace FiapX.Domain.Templates;

public static class VideoProcessingEmailTemplate
{
    public static EmailMessage Success(string recipient, string userName, string downloadUrl, string logoUrl)
    {
        return new EmailMessage
        {
            Recipient = recipient,
            Subject   = "Seu vídeo está pronto",
            Body      = $"""
                         <img src="{logoUrl}" alt="FIAP X" />
                         <h2>Olá, {userName}!</h2>
                         <p>O processamento do seu vídeo foi concluído com sucesso.</p>
                         <p>Acesse o sistema para realizar o download:<br>
                         <a href="{downloadUrl}">{downloadUrl}</a></p>

                         <br />
                         <p>Equipe FIAP X</p>
                         """,
        };
    }

    public static EmailMessage Failure(string recipient, string userName, string? reason, string logoUrl)
    {
        var reasonLine = string.IsNullOrWhiteSpace(reason)
            ? "Nenhum detalhe adicional disponível."
            : reason;

        return new EmailMessage
        {
            Recipient = recipient,
            Subject   = "Falha no processamento do vídeo",
            Body      = $"""
                         <img src="{logoUrl}" alt="FIAP X" />
                         <h2>Olá, {userName}!</h2>
                         <p>Ocorreu um erro ao processar o seu vídeo:</p>
                         <p><code>{reasonLine}</code></p>

                         <br />
                         <p>Tente novamente ou entre em contato com o suporte caso o problema persista.</p>
                         <p>Equipe FIAP X</p>
                         """,
        };
    }
}
