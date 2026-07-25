using FiapX.Infra.Observability;
using System.Diagnostics.Metrics;

namespace FiapX.Tests.Unit.Helpers;

public sealed class FiapXMeterListener : IDisposable
{
    public sealed record Measurement(string InstrumentName, long Value, IReadOnlyDictionary<string, object?> Tags);

    public sealed record DoubleMeasurement(string InstrumentName, double Value, IReadOnlyDictionary<string, object?> Tags);

    private readonly MeterListener _listener = new();
    private readonly List<Measurement> _measurements = [];
    private readonly List<DoubleMeasurement> _doubleMeasurements = [];

    public FiapXMeterListener()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == FiapXTelemetry.SourceName)
                listener.EnableMeasurementEvents(instrument);
        };

        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            var tagDictionary = BuildTags(tags);

            lock (_measurements)
                _measurements.Add(new Measurement(instrument.Name, value, tagDictionary));
        });

        _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
        {
            var tagDictionary = BuildTags(tags);

            lock (_doubleMeasurements)
                _doubleMeasurements.Add(new DoubleMeasurement(instrument.Name, value, tagDictionary));
        });

        _listener.Start();
    }

    public IReadOnlyList<Measurement> MeasurementsFor(string instrumentName)
    {
        lock (_measurements)
            return _measurements.Where(measurement => measurement.InstrumentName == instrumentName).ToList();
    }

    public IReadOnlyList<DoubleMeasurement> DoubleMeasurementsFor(string instrumentName)
    {
        lock (_doubleMeasurements)
            return _doubleMeasurements.Where(measurement => measurement.InstrumentName == instrumentName).ToList();
    }

    public long TotalFor(string instrumentName) =>
        MeasurementsFor(instrumentName).Sum(measurement => measurement.Value);

    public void Dispose() => _listener.Dispose();

    private static Dictionary<string, object?> BuildTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var tagDictionary = new Dictionary<string, object?>();
        foreach (var tag in tags)
            tagDictionary[tag.Key] = tag.Value;

        return tagDictionary;
    }
}
