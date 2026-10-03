namespace PhantomFanControl.Core;

public sealed class FanCurve : IFanCurve
{
    private readonly IReadOnlyList<CurvePoint> _points;
    public FanCurve(IEnumerable<CurvePoint> points, CurveInterpolation interpolation = CurveInterpolation.Linear, double minimumSpeed = 20, double maximumSpeed = 100)
    {
        _points = points.OrderBy(p => p.Temperature).ToArray();
        if (_points.Count < 2 || _points.Select(p => p.Temperature).Distinct().Count() != _points.Count) throw new ArgumentException("A curve requires at least two points with unique temperatures.", nameof(points));
        Interpolation = interpolation; MinimumSpeed = Math.Clamp(minimumSpeed, 0, 100); MaximumSpeed = Math.Clamp(maximumSpeed, MinimumSpeed, 100);
    }
    public CurveInterpolation Interpolation { get; }
    public double MinimumSpeed { get; }
    public double MaximumSpeed { get; }
    public IReadOnlyList<CurvePoint> Points => _points;
    public double Evaluate(double temperature)
    {
        if (temperature <= _points[0].Temperature) return Clamp(_points[0].Speed);
        if (temperature >= _points[^1].Temperature) return Clamp(_points[^1].Speed);
        var rightIndex = _points.Select((point, index) => (point, index)).First(item => item.point.Temperature >= temperature).index; var right = _points[rightIndex]; var left = _points[rightIndex - 1];
        if (Interpolation == CurveInterpolation.Step) return Clamp(left.Speed);
        var t = (temperature - left.Temperature) / (right.Temperature - left.Temperature);
        if (Interpolation == CurveInterpolation.Smooth) t = t * t * (3 - 2 * t);
        return Clamp(left.Speed + ((right.Speed - left.Speed) * t));
    }
    private double Clamp(double speed) => Math.Clamp(speed, MinimumSpeed, MaximumSpeed);
}
