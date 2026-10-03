using System.Text.Json;
namespace PhantomFanControl.Core;
public sealed class AppConfiguration
{
    public AppSettings Settings { get; init; } = new(); public SafetySettings Safety { get; init; } = new(); public List<Profile> Profiles { get; init; } = Defaults.Profiles(); public Dictionary<string, FanConfiguration> Fans { get; init; } = new();
}
public sealed class AppSettings { public int PollingMilliseconds { get; init; } = 1000; public bool StartWithWindows { get; init; } public bool StartMinimized { get; init; } public bool TrayIcon { get; init; } = true; public string Theme { get; init; } = "Dark"; public int GraphHistorySeconds { get; init; } = 300; }
public sealed class FanConfiguration { public string TemperatureFormula { get; init; } = "MAX(CPU, GPU)"; public List<CurvePoint> Curve { get; init; } = [new(30, 25), new(50, 45), new(70, 75), new(90, 100)]; public ResponseSettings Response { get; init; } = new(); }
public static class Defaults { public static List<Profile> Profiles() => [new("Silent"), new("Balanced"), new("Performance"), new("Gaming"), new("Streaming")]; }
public sealed class JsonConfigurationStore(string path)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public async Task<AppConfiguration> LoadAsync(CancellationToken ct = default)
    {
        try { if (!File.Exists(path)) return new(); await using var stream = File.OpenRead(path); return await JsonSerializer.DeserializeAsync<AppConfiguration>(stream, Options, ct) ?? new(); }
        catch (JsonException) { var backup = path + ".invalid-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds(); if (File.Exists(path)) File.Move(path, backup, true); return new(); }
    }
    public async Task SaveAsync(AppConfiguration configuration, CancellationToken ct = default)
    { Directory.CreateDirectory(Path.GetDirectoryName(path)!); var temporary = path + ".tmp"; await using (var stream = File.Create(temporary)) await JsonSerializer.SerializeAsync(stream, configuration, Options, ct); File.Move(temporary, path, true); }
    public Task ExportAsync(string destination, AppConfiguration configuration, CancellationToken ct = default) { var copy = new JsonConfigurationStore(destination); return copy.SaveAsync(configuration, ct); }
}
public static class DiagnosticsReport
{
    public static string Create(HardwareSnapshot snapshot, IEnumerable<IHardwareProvider> providers) => $"Phantom Fan Control diagnostics — {DateTimeOffset.Now:O}\n\nProviders\n" + string.Join('\n', providers.Select(p => $"{p.Name}: {(p.IsAvailable ? "Connected" : "Unavailable")}")) + "\n\nHardware\n" + string.Join('\n', snapshot.Devices.Select(d => $"{d.Type}: {d.Name}")) + "\n\nSensors\n" + string.Join('\n', snapshot.Sensors.Select(s => $"{s.Name}: {s.Value?.ToString("0.##") ?? "Unavailable"} {s.Unit}")) + "\n\nFans\n" + string.Join('\n', snapshot.Fans.Select(f => $"{f.Name}: {(f.IsControllable ? "Controllable" : "Read-only")}"));
}
