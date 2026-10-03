namespace PhantomFanControl.Core;
public sealed record Profile(string Name, string? ProcessName = null, double? CpuLoadAbove = null, double? GpuLoadAbove = null, bool IsAutomatic = false);
public sealed class ProfileManager(IEnumerable<Profile> profiles, string initial = "Balanced") : IProfileManager
{
    private readonly List<Profile> _profiles = profiles.ToList();
    public Profile Active { get; private set; } = profiles.FirstOrDefault(p => p.Name == initial) ?? profiles.First();
    public event EventHandler<Profile>? ActiveChanged;
    public void Activate(string name) { var next = _profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException("Unknown profile.", nameof(name)); if (next != Active) { Active = next; ActiveChanged?.Invoke(this, next); } }
    public Profile? EvaluateAutomatic(IEnumerable<SensorReading> readings, IEnumerable<string> processNames) => _profiles.FirstOrDefault(p => p.IsAutomatic && ((p.ProcessName is not null && processNames.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase)) || (p.CpuLoadAbove is double cpu && readings.Any(s => s.Kind == SensorKind.Load && s.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase) && s.Value >= cpu)) || (p.GpuLoadAbove is double gpu && readings.Any(s => s.Kind == SensorKind.Load && s.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase) && s.Value >= gpu))));
}
