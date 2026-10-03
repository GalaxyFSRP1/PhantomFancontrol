namespace PhantomFanControl.Core;
public sealed record ResponseSettings(double HysteresisC = 2, TimeSpan? RampUpDelay = null, TimeSpan? RampDownDelay = null, double MaxRisePercentPerSecond = 50, double MaxFallPercentPerSecond = 15)
{ public TimeSpan UpDelay => RampUpDelay ?? TimeSpan.Zero; public TimeSpan DownDelay => RampDownDelay ?? TimeSpan.FromSeconds(5); }
public sealed class FanResponseSmoother
{
    private double? _last; private DateTimeOffset _lastChanged; private DateTimeOffset _lastSample;
    public FanResponseSmoother() { _lastChanged = _lastSample = DateTimeOffset.MinValue; }
    public double Apply(double requested, DateTimeOffset now, ResponseSettings settings)
    {
        if (_last is null) { _last = requested; _lastChanged = _lastSample = now; return requested; }
        var current = _last.Value; var elapsed = Math.Max(0.001, (now - _lastSample).TotalSeconds); _lastSample = now;
        var rising = requested > current;
        if (!rising && requested >= current - settings.HysteresisC) return current;
        if (now - _lastChanged < (rising ? settings.UpDelay : settings.DownDelay)) return current;
        var limit = (rising ? settings.MaxRisePercentPerSecond : settings.MaxFallPercentPerSecond) * elapsed;
        var next = rising ? Math.Min(requested, current + limit) : Math.Max(requested, current - limit);
        if (Math.Abs(next - current) > 0.001) _lastChanged = now; _last = next; return next;
    }
}
