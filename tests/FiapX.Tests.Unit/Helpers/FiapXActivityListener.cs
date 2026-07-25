using FiapX.Infra.Observability;
using System.Diagnostics;

namespace FiapX.Tests.Unit.Helpers;

public sealed class FiapXActivityListener : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly List<Activity> _startedActivities = [];

    public FiapXActivityListener()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == FiapXTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                lock (_startedActivities)
                    _startedActivities.Add(activity);
            },
        };

        ActivitySource.AddActivityListener(_listener);
    }

    public IReadOnlyList<Activity> StartedActivities
    {
        get
        {
            lock (_startedActivities)
                return _startedActivities.ToList();
        }
    }

    public void Dispose() => _listener.Dispose();
}
