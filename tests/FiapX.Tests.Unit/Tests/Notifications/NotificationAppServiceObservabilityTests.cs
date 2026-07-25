using FiapX.Application.Options;
using FiapX.Application.Services;
using FiapX.Infra.EmailSender;
using FiapX.Tests.Unit.Helpers;
using FiapX.Tests.Unit.Mocks;
using Microsoft.Extensions.Options;
using Moq;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notifications")]
[TestCategory("Observability")]
[TestCategory("Unit")]
[DoNotParallelize]
public class NotificationAppServiceObservabilityTests
{
    private const string SentMetric = "notifications.sent";
    private const string FailedMetric = "notifications.failed";
    private const string DurationMetric = "notifications.processing_duration_seconds";

    public TestContext TestContext { get; set; } = null!;

    private Mock<IEmailSenderService> _emailSenderMock = null!;
    private NotificationAppService _service = null!;

    [TestInitialize]
    public void Initialize()
    {
        _emailSenderMock = new Mock<IEmailSenderService>();
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = Options.Create(new EmailContent
        {
            LogoUrl = "https://example.com/logo.svg",
            DownloadBaseUrl = "https://example.com/videos"
        });

        _service = new NotificationAppService(_emailSenderMock.Object, options);
    }

    #region Sucesso

    [TestMethod("Incrementa notifications.sent com as tags de canal e status quando o email é enviado")]
    public async Task It_ShouldIncrementNotificationsSent_WhenEmailIsSentSuccessfully()
    {
        using var meterListener = new FiapXMeterListener();
        var message = NotificationEventMocks.BuildSucceededEvent();

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        AreEqual(1, meterListener.TotalFor(SentMetric));
        AreEqual(0, meterListener.TotalFor(FailedMetric));

        var sent = meterListener.MeasurementsFor(SentMetric).Single();
        AreEqual("email", sent.Tags["channel"]);
        AreEqual("succeeded", sent.Tags["status"]);
    }

    [TestMethod("Registra a duração do processamento quando o email é enviado")]
    public async Task It_ShouldRecordProcessingDuration_WhenEmailIsSent()
    {
        using var meterListener = new FiapXMeterListener();
        var message = NotificationEventMocks.BuildSucceededEvent();

        await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token);

        var duration = meterListener.DoubleMeasurementsFor(DurationMetric).Single();
        IsTrue(duration.Value >= 0, "A duração registrada deve ser maior ou igual a zero");
        AreEqual("email", duration.Tags["channel"]);
    }

    #endregion

    #region Falha

    [TestMethod("Incrementa notifications.failed quando o envio do email lança exceção")]
    public async Task It_ShouldIncrementNotificationsFailed_WhenEmailSendingThrows()
    {
        using var meterListener = new FiapXMeterListener();
        _emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp indisponível"));

        var message = NotificationEventMocks.BuildSucceededEvent();

        await ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token));

        AreEqual(1, meterListener.TotalFor(FailedMetric));
        AreEqual(0, meterListener.TotalFor(SentMetric));
        AreEqual(1, meterListener.DoubleMeasurementsFor(DurationMetric).Count);
    }

    [TestMethod("Incrementa notifications.failed quando o status do evento é inesperado")]
    public async Task It_ShouldIncrementNotificationsFailed_WhenStatusIsUnexpected()
    {
        using var meterListener = new FiapXMeterListener();
        var message = NotificationEventMocks.BuildEventWithStatus("processing");

        await ThrowsExactlyAsync<InvalidOperationException>(async () =>
            await _service.SendEmailMessage(message, TestContext.CancellationTokenSource.Token));

        AreEqual(1, meterListener.TotalFor(FailedMetric));
        AreEqual(0, meterListener.TotalFor(SentMetric));

        var failed = meterListener.MeasurementsFor(FailedMetric).Single();
        AreEqual("email", failed.Tags["channel"]);
    }

    #endregion
}
