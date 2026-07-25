using FiapX.Infra.CrossCutting;
using FiapX.Infra.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FiapX.Tests.Integration.Helpers;

public record SmtpTestConfig(string Host, int Port, string UserName, string Password);

public static class WorkerHost
{
    public static IHost Build(string sqsEndpoint, string queueName, SmtpTestConfig? smtp = null)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(BuildConfig(sqsEndpoint, queueName, smtp)))
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

    public static IHost BuildWithObservability(
        string sqsEndpoint,
        string queueName,
        Dictionary<string, string?>? datadogConfig = null,
        SmtpTestConfig? smtp = null)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(BuildConfig(sqsEndpoint, queueName, smtp));

        if (datadogConfig is not null)
            builder.Configuration.AddInMemoryCollection(datadogConfig);

        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.AddObservability(ObservabilityProfile.Worker);

        builder.Services
            .AddEmailSender(builder.Configuration)
            .AddAppServices(builder.Configuration)
            .AddMessageConsumer(builder.Configuration);

        return builder.Build();
    }

    private static Dictionary<string, string?> BuildConfig(string sqsEndpoint, string queueName, SmtpTestConfig? smtp) =>
        new()
        {
            ["EmailSenderOptions:Enabled"] = smtp is not null ? "true" : "false",
            ["EmailSenderOptions:SmtpServer"] = smtp?.Host ?? "localhost",
            ["EmailSenderOptions:SmtpPort"] = smtp?.Port.ToString() ?? "1025",
            ["EmailSenderOptions:SslRequired"] = "false",
            ["EmailSenderOptions:SenderInformation:Name"] = "Test",
            ["EmailSenderOptions:SenderInformation:Address"] = "test@test.com",
            ["EmailSenderOptions:UserName"] = smtp?.UserName,
            ["EmailSenderOptions:Password"] = smtp?.Password,
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
}
