using FiapX.Application.Consumers;
using FiapX.Application.Options;
using FiapX.Application.Services;
using FiapX.Infra.EmailSender;
using FiapX.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Unit.Tests.Consumers;

[TestClass]
[TestCategory("Consumers")]
[TestCategory("Observability")]
[TestCategory("Unit")]
[DoNotParallelize]
public class VideoProcessingCompletedEventConsumerObservabilityTests
{
    public TestContext TestContext { get; set; } = null!;

    private VideoProcessingCompletedEventConsumer _consumer = null!;

    [TestInitialize]
    public void Initialize()
    {
        var emailSenderMock = new Mock<IEmailSenderService>();
        emailSenderMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = Options.Create(new EmailContent
        {
            LogoUrl = "https://example.com/logo.svg",
            DownloadBaseUrl = "https://example.com/videos"
        });

        var service = new NotificationAppService(emailSenderMock.Object, options);
        _consumer = new VideoProcessingCompletedEventConsumer(
            new Mock<ILogger<VideoProcessingCompletedEventConsumer>>().Object, service);
    }

    [TestCleanup]
    public void Cleanup() => Activity.Current = null;

    [TestMethod("Adiciona a tag video.id na activity corrente ao consumir o evento")]
    public async Task It_ShouldTagCurrentActivityWithVideoId_WhenActivityIsActive()
    {
        var message = NotificationEventMocks.BuildSucceededEvent(jobId: "job-observability");
        var activity = new Activity("consume").SetIdFormat(ActivityIdFormat.W3C).Start();

        try
        {
            await _consumer.ConsumeAsync(message, TestContext.CancellationTokenSource.Token);

            AreEqual("job-observability", activity.GetTagItem("video.id"));
        }
        finally
        {
            activity.Stop();
        }
    }

    [TestMethod("Consome o evento normalmente quando não há activity corrente")]
    public async Task It_ShouldNotThrow_WhenNoActivityIsActive()
    {
        Activity.Current = null;
        var message = NotificationEventMocks.BuildSucceededEvent();

        await _consumer.ConsumeAsync(message, TestContext.CancellationTokenSource.Token);

        IsNull(Activity.Current);
    }
}
