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
    public TestContext? TestContext { get; set; }

    [TestMethod("Processa evento de sucesso e remove mensagem da fila")]
    public async Task It_ShouldDeleteMessageFromQueue_WhenSucceededEventIsProcessed()
    {
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.SucceededEvent,
            TestContext!.CancellationTokenSource.Token);

        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName);
        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        await AssertQueueIsEmpty(TestContext.CancellationTokenSource.Token);
    }

    [TestMethod("Processa evento de falha e remove mensagem da fila")]
    public async Task It_ShouldDeleteMessageFromQueue_WhenFailedEventIsProcessed()
    {
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.FailedEvent,
            TestContext!.CancellationTokenSource.Token);

        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName);
        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        await AssertQueueIsEmpty(TestContext.CancellationTokenSource.Token);
    }

    private static async Task AssertQueueIsEmpty(CancellationToken cancellationToken)
    {
        var attributes = await TestProperties.SqsClient.GetQueueAttributesAsync(
            new GetQueueAttributesRequest
            {
                QueueUrl = TestProperties.QueueUrl,
                AttributeNames = ["ApproximateNumberOfMessages", "ApproximateNumberOfMessagesNotVisible"]
            },
            cancellationToken);

        AreEqual(0, attributes.ApproximateNumberOfMessages, "Mensagens visíveis na fila");
        AreEqual(0, attributes.ApproximateNumberOfMessagesNotVisible, "Mensagens em voo na fila");
    }
}
