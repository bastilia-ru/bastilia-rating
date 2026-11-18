using System.Diagnostics.Metrics;

namespace Bastilia.Rating.Tests;

/// <summary>
/// Собирает измерения инструментов meter'а с заданным именем.
/// Ассерты — на наличие измерения с точным набором тегов, не на суммы:
/// статические инструменты разделяются между параллельно идущими тест-классами.
/// Потокобезопасен: callback измерений прилетает с потоков параллельных тестов.
/// </summary>
internal sealed class MeterListenerCollector : IDisposable
{
    private readonly object sync = new();
    private readonly MeterListener listener = new();
    private readonly List<(string Instrument, object? Value, IReadOnlyDictionary<string, object?> Tags)> measurements = [];

    public MeterListenerCollector(string meterName)
    {
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == meterName)
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, _) =>
        {
            AddMeasurement(instrument.Name, measurement, tags);
        });
        listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _) =>
        {
            AddMeasurement(instrument.Name, measurement, tags);
        });
        listener.Start();
    }

    private void AddMeasurement(string instrument, object? measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var tagSnapshot = new Dictionary<string, object?>();
        foreach (var tag in tags)
        {
            tagSnapshot[tag.Key] = tag.Value;
        }

        lock (sync)
        {
            measurements.Add((instrument, measurement, tagSnapshot));
        }
    }

    public IReadOnlyList<(string Instrument, object? Value, IReadOnlyDictionary<string, object?> Tags)> Measurements
    {
        get
        {
            lock (sync)
            {
                return measurements.ToArray();
            }
        }
    }

    public void Dispose() => listener.Dispose();
}
