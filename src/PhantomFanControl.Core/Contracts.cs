namespace PhantomFanControl.Core;

public enum SensorKind { Temperature, FanRpm, Load, Power, Voltage, Control }
public enum FanMode { HardwareDefault, Automatic, ManualPwm, ManualDc }
public sealed record SensorReading(string Id, string Name, SensorKind Kind, double? Value, string Unit, string DeviceId, DateTimeOffset Timestamp);
public sealed record FanDevice(string Id, string Name, string DeviceId, bool IsControllable, FanMode[] SupportedModes, double? Rpm, double? Percent);
public sealed record HardwareNode(string Id, string Name, string Type, IReadOnlyList<HardwareNode> Children);
public sealed record HardwareSnapshot(IReadOnlyList<HardwareNode> Devices, IReadOnlyList<SensorReading> Sensors, IReadOnlyList<FanDevice> Fans, DateTimeOffset CapturedAt);
public interface IHardwareProvider : IAsyncDisposable
{
    string Name { get; }
    bool IsAvailable { get; }
    Task<HardwareSnapshot> ScanAsync(CancellationToken cancellationToken);
    Task SetFanAsync(string fanId, double percent, FanMode mode, CancellationToken cancellationToken);
    Task RestoreFanAsync(string fanId, CancellationToken cancellationToken);
}
public interface IFanCurve { double Evaluate(double temperature); }
public interface ISafetyManager { SafetyDecision Evaluate(IEnumerable<SensorReading> sensors); }
public interface IProfileManager { Profile Active { get; } event EventHandler<Profile>? ActiveChanged; void Activate(string name); Profile? EvaluateAutomatic(IEnumerable<SensorReading> readings, IEnumerable<string> processNames); }
public sealed record CurvePoint(double Temperature, double Speed);
public enum CurveInterpolation { Linear, Step, Smooth }
public sealed record SafetyDecision(bool IsEmergency, string? Reason, double? RequiredSpeed);
