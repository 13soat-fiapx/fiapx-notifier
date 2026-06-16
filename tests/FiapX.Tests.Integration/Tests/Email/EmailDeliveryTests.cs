using FiapX.Tests.Integration.Helpers;
using FiapX.Tests.Integration.Mocks;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Integration.Tests.Email;

[TestClass]
[TestCategory("Email")]
[TestCategory("Integration")]
public class EmailDeliveryTests
{
    public TestContext? TestContext { get; set; }

    private static readonly HttpClient MailpitClient = new();

    private static readonly SmtpTestConfig SmtpConfig = new(
        Host: TestProperties.SmtpHost,
        Port: TestProperties.SmtpPort,
        UserName: TestSmtpServerContainer.SmtpUserName,
        Password: TestSmtpServerContainer.SmtpPassword);

    [TestMethod("Entrega email de sucesso ao destinatário correto quando evento succeeded é processado")]
    public async Task It_ShouldDeliverSuccessEmail_WhenSucceededEventIsProcessed()
    {
        var countBefore = await GetMessageCountAsync(TestContext!.CancellationTokenSource.Token);
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.SucceededEvent,
            TestContext.CancellationTokenSource.Token);
        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName, SmtpConfig);

        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        var messages = await GetMessagesAsync(TestContext.CancellationTokenSource.Token);
        IsGreaterThan(countBefore, messages.Total);
        var latest = messages.Messages[0];
        AreEqual("joao@example.com", latest.To[0].Address, "Destinatário incorreto");
        Contains("Seu vídeo está pronto", latest.Subject);
    }

    [TestMethod("Entrega email de falha ao destinatário correto quando evento failed é processado")]
    public async Task It_ShouldDeliverFailureEmail_WhenFailedEventIsProcessed()
    {
        var countBefore = await GetMessageCountAsync(TestContext!.CancellationTokenSource.Token);
        await TestProperties.SqsClient.SendMessageAsync(
            TestProperties.QueueUrl,
            VideoProcessingMessageMocks.FailedEvent,
            TestContext.CancellationTokenSource.Token);
        using var host = WorkerHost.Build(TestProperties.SqsEndpoint, TestProperties.QueueName, SmtpConfig);

        await host.RunAsync(TestContext.CancellationTokenSource.Token);

        var messages = await GetMessagesAsync(TestContext.CancellationTokenSource.Token);
        IsGreaterThan(countBefore, messages.Total);
        var latest = messages.Messages[0];
        AreEqual("joao@example.com", latest.To[0].Address, "Destinatário incorreto");
        Contains("Falha no processamento", latest.Subject);
    }

    private static async Task<int> GetMessageCountAsync(CancellationToken cancellationToken)
    {
        var response = await MailpitClient.GetFromJsonAsync<MailpitMessagesResponse>(
            new Uri(TestProperties.MailpitApiUri, "api/v1/messages"),
            cancellationToken);
        return response!.Total;
    }

    private static async Task<MailpitMessagesResponse> GetMessagesAsync(CancellationToken cancellationToken) =>
        (await MailpitClient.GetFromJsonAsync<MailpitMessagesResponse>(
            new Uri(TestProperties.MailpitApiUri, "api/v1/messages"),
            cancellationToken))!;

    private class MailpitMessagesResponse
    {
        public int Total { get; init; }
        public required MailpitMessage[] Messages { get; init; }
    }

    private class MailpitMessage
    {
        public required string Subject { get; init; }
        public required MailpitAddress[] To { get; init; }
    }

    private class MailpitAddress
    {
        public required string Address { get; init; }
    }
}
