using Amazon.SQS;
using FiapX.Tests.Integration.Helpers;

namespace FiapX.Tests.Integration;

[TestClass]
public static class TestProperties
{
    private static TestSqsContainer _sqsContainer = null!;

    public const string QueueName = "test-video-processing-completed";

    public static IAmazonSQS SqsClient => _sqsContainer.SqsClient;
    public static string QueueUrl { get; private set; } = null!;
    public static string SqsEndpoint => _sqsContainer.Endpoint;

    [AssemblyInitialize]
    public static async Task Setup(TestContext context)
    {
        _sqsContainer = new TestSqsContainer();
        await _sqsContainer.StartAsync();
        QueueUrl = await _sqsContainer.CreateQueueAsync(QueueName);
    }
}
