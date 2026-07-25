using Amazon.SQS.Model;
using FiapX.Tests.Integration.Helpers;
using FiapX.Tests.Integration.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Integration.Tests.Observability;

[TestClass]
[TestCategory("Observability")]
[TestCategory("Integration")]
public class ObservabilityStartupTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod("Sobe o worker sem providers de telemetria quando a API key não está configurada")]
    public void It_ShouldNotRegisterTelemetryProviders_WhenApiKeyIsMissing()
    {
        using var host = WorkerHost.BuildWithObservability(
            TestProperties.SqsEndpoint, TestProperties.QueueName);

        IsNull(host.Services.GetService<TracerProvider>());
        IsNull(host.Services.GetService<MeterProvider>());
    }

    [TestMethod("Processa a mensagem e encerra normalmente mesmo com o endpoint OTLP inacessível")]
    public async Task It_ShouldProcessMessageAndShutdown_WhenOtlpEndpointIsUnreachable()
    {
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.SucceededEvent,
            TestContext!.CancellationTokenSource.Token);

        var datadogConfig = new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "integration-test-key",
            ["Datadog:OtlpEndpoint"] = "http://127.0.0.1:9",
        };

        using var host = WorkerHost.BuildWithObservability(
            TestProperties.SqsEndpoint, TestProperties.QueueName, datadogConfig);

        IsNotNull(host.Services.GetService<TracerProvider>());
        IsNotNull(host.Services.GetService<MeterProvider>());

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
