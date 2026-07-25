using FiapX.Infra.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace FiapX.Tests.Unit.Tests.Observability;

[TestClass]
[TestCategory("Observability")]
[TestCategory("Unit")]
public class ObservabilityExtensionsTests
{
    private const string FakeOtlpEndpoint = "http://127.0.0.1:9";

    #region Guard

    [TestMethod("Não registra os providers de telemetria quando a API key não está configurada")]
    public void It_ShouldNotRegisterTelemetryProviders_WhenApiKeyIsMissing()
    {
        var builder = CreateBuilder([]);

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        IsNull(host.Services.GetService<TracerProvider>());
        IsNull(host.Services.GetService<MeterProvider>());
        IsFalse(HasOpenTelemetryLoggerProvider(host));
    }

    [TestMethod("Não registra os providers de telemetria quando a API key contém apenas espaços")]
    public void It_ShouldNotRegisterTelemetryProviders_WhenApiKeyIsWhiteSpace()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "   ",
            ["Datadog:OtlpEndpoint"] = FakeOtlpEndpoint,
        });

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        IsNull(host.Services.GetService<TracerProvider>());
        IsNull(host.Services.GetService<MeterProvider>());
    }

    [TestMethod("Não registra os providers de telemetria quando há API key mas falta o endpoint OTLP")]
    public void It_ShouldNotRegisterTelemetryProviders_WhenOtlpEndpointIsMissing()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "test-key",
        });

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        IsNull(host.Services.GetService<TracerProvider>());
        IsNull(host.Services.GetService<MeterProvider>());
        IsFalse(HasOpenTelemetryLoggerProvider(host));
    }

    #endregion

    #region Registro dos providers

    [TestMethod("Registra traces, métricas e logs quando a API key e o endpoint estão configurados")]
    public void It_ShouldRegisterAllTelemetryProviders_WhenApiKeyIsConfigured()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "test-key",
            ["Datadog:OtlpEndpoint"] = $"{FakeOtlpEndpoint}/",
            ["AppInfo:Name"] = "notifier",
            ["AppInfo:Version"] = "9.9.9",
        });

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        IsNotNull(host.Services.GetService<TracerProvider>());
        IsNotNull(host.Services.GetService<MeterProvider>());
        IsTrue(HasOpenTelemetryLoggerProvider(host));
    }

    [TestMethod("Registra os providers de telemetria também no profile Api")]
    public void It_ShouldRegisterTelemetryProviders_WhenProfileIsApi()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "test-key",
            ["Datadog:OtlpEndpoint"] = FakeOtlpEndpoint,
        });

        builder.AddObservability(ObservabilityProfile.Api);

        using var host = builder.Build();
        IsNotNull(host.Services.GetService<TracerProvider>());
        IsNotNull(host.Services.GetService<MeterProvider>());
    }

    #endregion

    #region Resource attributes

    [TestMethod("Compõe o service.name com o prefixo fiapx- a partir da seção AppInfo")]
    public void It_ShouldComposeServiceNameWithFiapXPrefix_WhenAppInfoIsConfigured()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "test-key",
            ["Datadog:OtlpEndpoint"] = FakeOtlpEndpoint,
            ["AppInfo:Name"] = "notifier",
            ["AppInfo:Version"] = "9.9.9",
        });

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        var attributes = GetResourceAttributes(host);

        AreEqual("fiapx-notifier", attributes["service.name"]);
        AreEqual("9.9.9", attributes["service.version"]);
        AreEqual("development", attributes["deployment.environment"]);
    }

    [TestMethod("Usa os valores padrão de AppInfo quando a seção não está presente")]
    public void It_ShouldUseDefaultAppInfo_WhenAppInfoSectionIsMissing()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["Datadog:ApiKey"] = "test-key",
            ["Datadog:OtlpEndpoint"] = FakeOtlpEndpoint,
        });

        builder.AddObservability(ObservabilityProfile.Worker);

        using var host = builder.Build();
        var attributes = GetResourceAttributes(host);

        AreEqual("fiapx-service", attributes["service.name"]);
        AreEqual("1.0.0", attributes["service.version"]);
    }

    #endregion

    private static HostApplicationBuilder CreateBuilder(Dictionary<string, string?> configuration)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Development",
        });

        builder.Configuration.AddInMemoryCollection(configuration);
        return builder;
    }

    private static Dictionary<string, object> GetResourceAttributes(IHost host) =>
        host.Services.GetRequiredService<TracerProvider>().GetResource().Attributes
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value);

    private static bool HasOpenTelemetryLoggerProvider(IHost host) =>
        host.Services.GetServices<ILoggerProvider>()
            .Any(provider => provider.GetType().Name == "OpenTelemetryLoggerProvider");
}
