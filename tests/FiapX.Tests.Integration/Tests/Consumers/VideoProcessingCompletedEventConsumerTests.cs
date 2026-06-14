using Amazon.SQS.Model;
using FiapX.Tests.Integration.Helpers;
using FiapX.Tests.Integration.Mocks;
using Microsoft.Extensions.Hosting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Integration.Tests.Consumers;

[TestClass]
[TestCategory("Consumers")]
[TestCategory("Integration")]
public class VideoProcessingCompletedEventConsumerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod("Processa evento de sucesso e remove mensagem da fila")]
    public async Task It_ShouldDeleteMessageFromQueue_WhenSucceededEventIsProcessed()
    {
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.SucceededEvent,
            TestContext.CancellationTokenSource.Token);

        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName);
        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        var response = await TestProperties.SqsClient.ReceiveMessageAsync(
            new ReceiveMessageRequest { QueueUrl = TestProperties.QueueUrl, MaxNumberOfMessages = 1, WaitTimeSeconds = 1 },
            TestContext.CancellationTokenSource.Token);

        AreEqual(0, response.Messages?.Count ?? 0);
    }

    [TestMethod("Processa evento de falha e remove mensagem da fila")]
    public async Task It_ShouldDeleteMessageFromQueue_WhenFailedEventIsProcessed()
    {
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.FailedEvent,
            TestContext.CancellationTokenSource.Token);

        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName);
        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        var response = await TestProperties.SqsClient.ReceiveMessageAsync(
            new ReceiveMessageRequest { QueueUrl = TestProperties.QueueUrl, MaxNumberOfMessages = 1, WaitTimeSeconds = 1 },
            TestContext.CancellationTokenSource.Token);

        AreEqual(0, response.Messages?.Count ?? 0);
    }
}
