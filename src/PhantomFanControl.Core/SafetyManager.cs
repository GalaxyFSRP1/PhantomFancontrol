namespace PhantomFanControl.Core;
public sealed record SafetySettings(double CpuEmergencyC = 90, double GpuEmergencyC = 90, double WarningMarginC = 5);
public sealed class SafetyManager(SafetySettings settings) : ISafetyManager
{
    public SafetyDecision Evaluate(IEnumerable<SensorReading> sensors)
    {
        foreach (var sensor in sensors.Where(x => x.Kind == SensorKind.Temperature && x.Value.HasValue))
        {
            var threshold = sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase) ? settings.GpuEmergencyC : settings.CpuEmergencyC;
            if (sensor.Value >= threshold) return new(true, $"{sensor.Name} reached {sensor.Value:0.#}°C; emergency cooling is active.", 100);
        }
        return new(false, null, null);
    }
}
