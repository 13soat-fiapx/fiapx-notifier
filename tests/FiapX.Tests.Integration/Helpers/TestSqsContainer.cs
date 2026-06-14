using Amazon.Runtime;
using Amazon.SQS;
using Testcontainers.LocalStack;

namespace FiapX.Tests.Integration.Helpers;

public sealed class TestSqsContainer : IAsyncDisposable
{
    private readonly LocalStackContainer _container = new LocalStackBuilder("localstack/localstack:3")
        .WithName($"testcontainers-sqs-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    public IAmazonSQS SqsClient { get; private set; } = null!;
    public string Endpoint { get; private set; } = null!;

    public async Task StartAsync()
    {
        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(4566);
        Endpoint = $"http://localhost:{port}";

        SqsClient = new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig { ServiceURL = Endpoint });
    }

    public async Task<string> CreateQueueAsync(string queueName)
    {
        var response = await SqsClient.CreateQueueAsync(queueName);
        return response.QueueUrl;
    }

    public async ValueTask DisposeAsync()
    {
        SqsClient.Dispose();
        await _container.DisposeAsync();
    }
}
