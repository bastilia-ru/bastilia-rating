using System.Diagnostics.Metrics;

namespace Bastilia.Rating.Tests;

/// <summary>
/// Собирает измерения инструментов meter'а с заданным именем.
/// Ассерты — на наличие измерения с точным набором тегов, не на суммы:
/// статические инструменты разделяются между параллельно идущими тест-классами.
/// </summary>
internal sealed class MeterListenerCollector : IDisposable
{
    private readonly MeterListener listener = new();
    private readonly List<(string Instrument, long Value, IReadOnlyDictionary<string, object?> Tags)> measurements = [];

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
            var tagSnapshot = new Dictionary<string, object?>();
            foreach (var tag in tags)
            {
                tagSnapshot[tag.Key] = tag.Value;
            }

            measurements.Add((instrument.Name, measurement, tagSnapshot));
        });
        listener.Start();
    }

    public IReadOnlyList<(string Instrument, long Value, IReadOnlyDictionary<string, object?> Tags)> Measurements => measurements;

    public void Dispose() => listener.Dispose();
}
