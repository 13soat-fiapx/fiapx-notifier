using Amazon.SQS;
using FiapX.Tests.Integration.Helpers;

namespace FiapX.Tests.Integration;

[TestClass]
public static class TestProperties
{
    private static TestSqsContainer _sqsContainer = null!;
    private static TestSmtpServerContainer _smtpContainer = null!;

    public const string QueueName = "test-video-processing-completed";

    public static IAmazonSQS SqsClient => _sqsContainer.SqsClient;
    public static string QueueUrl { get; private set; } = null!;
    public static string SqsEndpoint => _sqsContainer.Endpoint;

    public static string SmtpHost => _smtpContainer.SmtpHost;
    public static int SmtpPort => _smtpContainer.SmtpPort;
    public static Uri MailpitApiUri => _smtpContainer.ApiUri;

    [AssemblyInitialize]
    public static async Task Setup(TestContext context)
    {
        _sqsContainer = new TestSqsContainer();
        _smtpContainer = new TestSmtpServerContainer();

        await Task.WhenAll(
            _sqsContainer.StartAsync(),
            _smtpContainer.StartAsync()
        );

        QueueUrl = await _sqsContainer.CreateQueueAsync(QueueName);
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        await _sqsContainer.DisposeAsync();
        await _smtpContainer.DisposeAsync();
    }
}
