using PhantomFanControl.Core;
namespace PhantomFanControl.Tests;
internal sealed class MockHardwareProvider : IHardwareProvider
{
    public string Name => "Mock"; public bool IsAvailable => true; public double? LastRequestedSpeed { get; private set; }
    public Task<HardwareSnapshot> ScanAsync(CancellationToken cancellationToken) => Task.FromResult(new HardwareSnapshot([], [new("cpu", "CPU Package", SensorKind.Temperature, 42, "°C", "cpu", DateTimeOffset.Now)], [new("fan", "CPU_FAN", "board", true, [FanMode.HardwareDefault, FanMode.ManualPwm], 1200, 40)], DateTimeOffset.Now));
    public Task SetFanAsync(string fanId, double percent, FanMode mode, CancellationToken cancellationToken) { if (fanId != "fan") throw new InvalidOperationException("Sensor disconnected."); LastRequestedSpeed = percent; return Task.CompletedTask; }
    public Task RestoreFanAsync(string fanId, CancellationToken cancellationToken) { LastRequestedSpeed = null; return Task.CompletedTask; }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
