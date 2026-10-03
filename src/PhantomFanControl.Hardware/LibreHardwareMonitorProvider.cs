using LibreHardwareMonitor.Hardware;
using PhantomFanControl.Core;
namespace PhantomFanControl.Hardware;
public sealed class LibreHardwareMonitorProvider : IHardwareProvider
{
    private readonly Computer _computer = new() { IsCpuEnabled = true, IsGpuEnabled = true, IsMotherboardEnabled = true, IsMemoryEnabled = true, IsStorageEnabled = true, IsControllerEnabled = true };
    private readonly Dictionary<string, IControl> _controls = new(); private bool _opened;
    public string Name => "LibreHardwareMonitor"; public bool IsAvailable => _opened;
    public Task<HardwareSnapshot> ScanAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested(); if (!_opened) { _computer.Open(); _opened = true; }
        _controls.Clear(); var sensors = new List<SensorReading>(); var fans = new List<FanDevice>(); var nodes = new List<HardwareNode>();
        foreach (var hardware in _computer.Hardware) { Update(hardware); nodes.Add(ReadHardware(hardware, sensors, fans)); }
        return Task.FromResult(new HardwareSnapshot(nodes, sensors, fans, DateTimeOffset.Now));
    }
    private HardwareNode ReadHardware(IHardware hardware, List<SensorReading> sensors, List<FanDevice> fans)
    {
        var children = hardware.SubHardware.Select(child => { Update(child); return ReadHardware(child, sensors, fans); }).ToArray();
        foreach (var sensor in hardware.Sensors)
        {
            var kind = sensor.SensorType switch { SensorType.Temperature => SensorKind.Temperature, SensorType.Fan => SensorKind.FanRpm, SensorType.Load => SensorKind.Load, SensorType.Power => SensorKind.Power, SensorType.Voltage => SensorKind.Voltage, SensorType.Control => SensorKind.Control, _ => (SensorKind?)null };
            if (kind is null) continue; var id = sensor.Identifier.ToString(); var value = sensor.Value is float v ? v : null; sensors.Add(new(id, sensor.Name, kind.Value, value, Unit(kind.Value), hardware.Identifier.ToString(), DateTimeOffset.Now));
            if (sensor.SensorType == SensorType.Fan) fans.Add(new(id, sensor.Name, hardware.Identifier.ToString(), false, [FanMode.HardwareDefault], value, null));
            if (sensor.SensorType == SensorType.Control && sensor.Control is not null) { _controls[id] = sensor.Control; fans.Add(new(id, sensor.Name, hardware.Identifier.ToString(), true, [FanMode.HardwareDefault, FanMode.ManualPwm, FanMode.Automatic], null, value)); }
        }
        return new HardwareNode(hardware.Identifier.ToString(), hardware.Name, hardware.HardwareType.ToString(), children);
    }
    public Task SetFanAsync(string fanId, double percent, FanMode mode, CancellationToken cancellationToken)
    { cancellationToken.ThrowIfCancellationRequested(); if (!_controls.TryGetValue(fanId, out var control)) throw new InvalidOperationException("The selected control is no longer available."); if (mode is FanMode.HardwareDefault or FanMode.Automatic) { control.ControlMode = ControlMode.Default; } else { control.SoftwareValue = (float)Math.Clamp(percent, 0, 100); control.ControlMode = ControlMode.Software; } return Task.CompletedTask; }
    public Task RestoreFanAsync(string fanId, CancellationToken cancellationToken) => SetFanAsync(fanId, 0, FanMode.HardwareDefault, cancellationToken);
    public ValueTask DisposeAsync() { if (_opened) _computer.Close(); return ValueTask.CompletedTask; }
    private static void Update(IHardware hardware) { hardware.Update(); foreach (var sub in hardware.SubHardware) Update(sub); }
    private static string Unit(SensorKind kind) => kind switch { SensorKind.Temperature => "°C", SensorKind.FanRpm => "RPM", SensorKind.Load => "%", SensorKind.Power => "W", SensorKind.Voltage => "V", SensorKind.Control => "%", _ => "" };
}
