using Amazon.SQS;
using Amazon.SQS.Model;
using FiapX.Infra.Messaging.Helpers;
using FiapX.Infra.Messaging.Models;
using FiapX.Infra.Observability.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace FiapX.Infra.Messaging.Consumers;

public class ConsumerBackgroundService<T>(
    IAmazonSQS sqsClient,
    QueueUrlResolver urlResolver,
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime applicationLifetime,
    ILogger<ConsumerBackgroundService<T>> logger) : BackgroundService where T : class
{
    private string _queueUrl = string.Empty;
    private string _queueName = string.Empty;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _queueUrl = await urlResolver.ResolveAsync<T>(cancellationToken);
        _queueName = _queueUrl[(_queueUrl.LastIndexOf('/') + 1)..];

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Fetching messages from SQS queue '{QueueUrl}'", _queueUrl);

        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 20,
        }, stoppingToken);

        if (response.Messages is null)
        {
            logger.LogWarning("No messages received from SQS queue '{QueueUrl}'", _queueUrl);
            applicationLifetime.StopApplication();
            return;
        }

        await ProcessMessageAsync(response.Messages[0], stoppingToken);
        applicationLifetime.StopApplication();
    }

    private async Task ProcessMessageAsync(Message sqsMessage, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer<T>>();

        try
        {
            logger.LogInformation("Processing message '{MessageId}' for event '{EventType}'",
                sqsMessage.MessageId, typeof(T).Name);

            var message = JsonSerializer.Deserialize<MessageBase<T>>(sqsMessage.Body, _serializerOptions) ??
                          throw new InvalidOperationException($"Failed to deserialize message body to '{typeof(T).Name}'.");

            using var activity = MessageTracing.StartConsumerActivity(_queueName, message.Headers?.Traceparent);

            try
            {
                await consumer.ConsumeAsync(message.Payload, cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                throw;
            }

            await sqsClient.DeleteMessageAsync(_queueUrl, sqsMessage.ReceiptHandle, cancellationToken);

            logger.LogInformation("Message '{MessageId}' for event '{EventType}' processed and deleted",
                sqsMessage.MessageId, typeof(T).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message '{MessageId}' from queue '{QueueUrl}'",
                sqsMessage.MessageId, _queueUrl);
        }
    }
}
