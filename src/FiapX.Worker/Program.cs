using FiapX.Infra.CrossCutting;
using FiapX.Infra.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

var builder = Host.CreateApplicationBuilder(args);

builder.AddObservability(ObservabilityProfile.Worker);

builder.Services
    .AddEmailSender(builder.Configuration)
    .AddAppServices(builder.Configuration)
    .AddMessageConsumer(builder.Configuration)
    .AddLogging();

using var host = builder.Build();

try
{
    await host.StartAsync();
    await host.WaitForShutdownAsync();
}
finally
{
    host.Services.GetService<TracerProvider>()?.ForceFlush(5000);
    host.Services.GetService<MeterProvider>()?.ForceFlush(5000);
}
