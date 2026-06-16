using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace FiapX.Tests.Integration.Helpers;

public sealed class TestSmtpServerContainer : IAsyncDisposable
{
    public const string SmtpUserName = "test@fiapx.com";
    public const string SmtpPassword = "fiapx1234";

    public IContainer Container { get; } = new ContainerBuilder("axllent/mailpit:v1.27.10")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithEnvironment("MP_SMTP_AUTH", $"{SmtpUserName}:{SmtpPassword}")
        .WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "true")
        .WithName($"testcontainers-smtp-{Guid.NewGuid()}")
        .WithCleanUp(true)
        .Build();

    public string SmtpHost => Container.Hostname;
    public int SmtpPort => Container.GetMappedPublicPort(1025);
    public Uri ApiUri => new UriBuilder("http", Container.Hostname, Container.GetMappedPublicPort(8025)).Uri;

    public async Task StartAsync()
    {
        await Container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
