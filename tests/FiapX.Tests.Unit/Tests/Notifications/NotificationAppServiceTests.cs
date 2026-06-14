using FiapX.Application.Options;
using FiapX.Application.Services;
using FiapX.Domain.Events;
using FiapX.Infra.EmailSender;
using FiapX.Tests.Unit.Mocks;
using Microsoft.Extensions.Options;
using Moq;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notifications")]
[TestCategory("Unit")]
public class NotificationAppServiceTests
{
    public TestContext TestContext { get; set; } = null!;

    private Mock<IEmailSenderService> _emailSenderMock = null!;
    private NotificationAppService _service = null!;

    private const string LogoUrl = "https://example.com/logo.svg";
    private const string DownloadBaseUrl = "https://example.com/videos";

    [TestInitialize]
    public void Initialize()
    {
        _emailSenderMock = new Mock<IEmailSenderService>();
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = Options.Create(new EmailContent { LogoUrl = LogoUrl, DownloadBaseUrl = DownloadBaseUrl });
        _service = new NotificationAppService(_emailSenderMock.Object, options);
    }

    #region Sucesso

    [TestMethod("Envia email de sucesso para o destinatário correto com assunto adequado")]
    public async Task It_ShouldSendSuccessEmail_WhenStatusIsSucceeded()
    {
        var message = NotificationEventMocks.BuildSucceededEvent(userEmail: "joao@example.com");

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        _emailSenderMock.Verify(s => s.SendAsync(
            It.Is<EmailMessage>(e =>
                e.Recipient == "joao@example.com" &&
                e.Subject == "Seu vídeo está pronto"),
            TestContext.CancellationTokenSource.Token),
            Times.Once());
    }

    [TestMethod("Constrói URL de download correta concatenando base URL com ID do job")]
    public async Task It_ShouldBuildCorrectDownloadUrl_WhenStatusIsSucceeded()
    {
        var message = NotificationEventMocks.BuildSucceededEvent(jobId: "job-abc");
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains($"{DownloadBaseUrl}/job-abc", captured.Body, $"Corpo não contém a URL '{DownloadBaseUrl}/job-abc'");
    }

    [TestMethod("Inclui logo e nome do usuário no corpo do email de sucesso")]
    public async Task It_ShouldIncludeLogoAndUserName_WhenStatusIsSucceeded()
    {
        var message = NotificationEventMocks.BuildSucceededEvent(userName: "Maria Oliveira");
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains(LogoUrl, captured.Body);
        Contains("Maria Oliveira", captured.Body);
    }

    #endregion

    #region Falha

    [TestMethod("Envia email de falha para o destinatário correto com assunto adequado")]
    public async Task It_ShouldSendFailureEmail_WhenStatusIsFailed()
    {
        var message = NotificationEventMocks.BuildFailedEvent(userEmail: "joao@example.com");

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        _emailSenderMock.Verify(s => s.SendAsync(
            It.Is<EmailMessage>(e =>
                e.Recipient == "joao@example.com" &&
                e.Subject == "Falha no processamento do vídeo"),
            TestContext.CancellationTokenSource.Token),
            Times.Once());
    }

    [TestMethod("Inclui a mensagem de erro no corpo do email de falha")]
    public async Task It_ShouldIncludeErrorReason_WhenErrorMessageExists()
    {
        var messages = new List<ProcessingMessage>
        {
            new() { Code = "ERR001", Message = "Codec não suportado", Severity = "error" }
        };
        var message = NotificationEventMocks.BuildFailedEvent(messages: messages);
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains("Codec não suportado", captured.Body);
    }

    [TestMethod("Usa apenas a primeira mensagem de erro quando há múltiplas com severity error")]
    public async Task It_ShouldUseFirstErrorMessage_WhenMultipleErrorMessagesExist()
    {
        var messages = new List<ProcessingMessage>
        {
            new() { Code = "ERR001", Message = "Primeiro erro", Severity = "error" },
            new() { Code = "ERR002", Message = "Segundo erro", Severity = "error" }
        };
        var message = NotificationEventMocks.BuildFailedEvent(messages: messages);
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains("Primeiro erro", captured.Body);
        DoesNotContain("Segundo erro", captured.Body);
    }

    [TestMethod("Usa texto padrão no corpo quando nenhuma mensagem possui severity error")]
    public async Task It_ShouldUseDefaultReason_WhenNoErrorMessageExists()
    {
        var messages = new List<ProcessingMessage>
        {
            new() { Code = "WARN001", Message = "Aviso de performance", Severity = "warning" },
            new() { Code = "INFO001", Message = "Iniciando processamento", Severity = "info" }
        };
        var message = NotificationEventMocks.BuildFailedEvent(messages: messages);
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains("Nenhum detalhe adicional disponível.", captured.Body);
    }

    [TestMethod("Usa texto padrão no corpo quando a lista de mensagens está vazia")]
    public async Task It_ShouldUseDefaultReason_WhenMessagesListIsEmpty()
    {
        var message = NotificationEventMocks.BuildFailedEvent(messages: []);
        EmailMessage? captured = null;
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        IsNotNull(captured);
        Contains("Nenhum detalhe adicional disponível.", captured.Body);
    }

    #endregion

    #region Status inválido

    [TestMethod("Lança InvalidOperationException para status desconhecido")]
    public async Task It_ShouldThrowInvalidOperationException_WhenStatusIsUnknown()
    {
        var message = NotificationEventMocks.BuildEventWithStatus("processing");

        await ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token));
    }

    [TestMethod("Não chama o serviço de email quando o status é desconhecido")]
    public async Task It_ShouldNotCallEmailSender_WhenStatusIsUnknown()
    {
        var message = NotificationEventMocks.BuildEventWithStatus("processing");

        try { await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token); }
        catch (InvalidOperationException) { }

        _emailSenderMock.Verify(
            s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    #endregion

    #region CancellationToken

    [TestMethod("Repassa o CancellationToken recebido ao serviço de email")]
    public async Task It_ShouldPassCancellationToken_ToEmailSender()
    {
        var message = NotificationEventMocks.BuildSucceededEvent();
        var token = TestContext.CancellationTokenSource.Token;

        await _service.SendEmailMessage(message, token);

        _emailSenderMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), token), Times.Once());
    }

    #endregion
}
