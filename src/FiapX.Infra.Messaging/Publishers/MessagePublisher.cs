using Amazon.SQS;
using Amazon.SQS.Model;
using FiapX.Infra.Messaging.Helpers;
using FiapX.Infra.Messaging.Models;
using System.Text.Json;

namespace FiapX.Infra.Messaging.Publishers;

public class MessagePublisher(IAmazonSQS sqsClient, QueueUrlResolver urlResolver) : IMessagePublisher
{
    public async Task PublishAsync<T, TEvent>(T message, CancellationToken cancellationToken = default)
        where T : MessageBase<TEvent> where TEvent : class, new()
    {
        var queueUrl = await urlResolver.ResolveAsync<TEvent>(cancellationToken);

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
        }, cancellationToken);
    }
}
