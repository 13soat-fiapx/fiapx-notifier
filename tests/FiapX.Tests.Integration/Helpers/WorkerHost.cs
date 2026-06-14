using FiapX.Infra.CrossCutting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FiapX.Tests.Integration.Helpers;

public static class WorkerHost
{
    public static IHost Build(string sqsEndpoint, string queueName)
    {
        var config = new Dictionary<string, string?>
        {
            ["EmailSenderOptions:Enabled"] = "false",
            ["EmailSenderOptions:SmtpServer"] = "localhost",
            ["EmailSenderOptions:SmtpPort"] = "1025",
            ["EmailSenderOptions:SslRequired"] = "false",
            ["EmailSenderOptions:SenderInformation:Name"] = "Test",
            ["EmailSenderOptions:SenderInformation:Address"] = "test@test.com",
            ["EmailContent:LogoUrl"] = "https://example.com/logo.svg",
            ["EmailContent:DownloadBaseUrl"] = "https://example.com/videos",
            ["AwsCredentials:Region"] = "us-east-1",
            ["AwsCredentials:UseLocalstack"] = "true",
            ["AwsCredentials:LocalstackUrl"] = sqsEndpoint,
            ["AwsCredentials:AccessKey"] = "test",
            ["AwsCredentials:SecretAccessKey"] = "test",
            ["AwsCredentials:SessionToken"] = "test",
            ["MessagingOptions:DisableConsumers"] = "false",
            ["MessagingOptions:QueueNames:VideoProcessingCompleted"] = queueName,
        };

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(config))
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .ConfigureServices((ctx, services) =>
            {
                services
                    .AddEmailSender(ctx.Configuration)
                    .AddAppServices(ctx.Configuration)
                    .AddMessageConsumer(ctx.Configuration);
            })
            .Build();
    }
}
