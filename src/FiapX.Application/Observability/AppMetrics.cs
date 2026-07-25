using FiapX.Infra.Observability;
using System.Diagnostics.Metrics;

namespace FiapX.Application.Observability;

public static class AppMetrics
{
    public static readonly Counter<long> NotificationsSent =
        FiapXTelemetry.Meter.CreateCounter<long>("notifications.sent");

    public static readonly Counter<long> NotificationsFailed =
        FiapXTelemetry.Meter.CreateCounter<long>("notifications.failed");

    public static readonly Histogram<double> ProcessingDurationSeconds =
        FiapXTelemetry.Meter.CreateHistogram<double>("notifications.processing_duration_seconds");
}
