using PhantomFanControl.Core;
namespace PhantomFanControl.Tests;
public sealed class CoreTests
{
 [Fact] public void Linear_curve_interpolates() => Assert.Equal(50, new FanCurve([new(30, 20), new(70, 80)]).Evaluate(50));
 [Fact] public void Step_curve_uses_previous_point() => Assert.Equal(20, new FanCurve([new(30, 20), new(70, 80)], CurveInterpolation.Step).Evaluate(50));
 [Fact] public void Formula_uses_available_temperature_sources() => Assert.Equal(75, TemperatureFormula.Evaluate("MAX(CPU, GPU) + 5", new Dictionary<string, double?> { ["CPU"] = 70, ["GPU"] = 60 }));
 [Fact] public void Smoother_respects_fall_delay() { var smoother = new FanResponseSmoother(); var now = DateTimeOffset.UtcNow; Assert.Equal(80, smoother.Apply(80, now, new())); Assert.Equal(80, smoother.Apply(30, now.AddSeconds(1), new())); }
 [Fact] public void Safety_triggers_at_cpu_limit() { var decision = new SafetyManager(new(90, 90)).Evaluate([new("cpu", "CPU Package", SensorKind.Temperature, 91, "°C", "cpu", DateTimeOffset.Now)]); Assert.True(decision.IsEmergency); Assert.Equal(100, decision.RequiredSpeed); }
 [Fact] public async Task Configuration_round_trips() { var path = Path.GetTempFileName(); try { var store = new JsonConfigurationStore(path); await store.SaveAsync(new AppConfiguration { Settings = new() { PollingMilliseconds = 500 } }); Assert.Equal(500, (await store.LoadAsync()).Settings.PollingMilliseconds); } finally { File.Delete(path); } }
 [Fact] public void Profiles_switch_and_auto_select() { var manager = new ProfileManager([new("Balanced"), new("Gaming", "game.exe", IsAutomatic: true)]); Assert.Equal("Gaming", manager.EvaluateAutomatic([], ["game.exe"])?.Name); manager.Activate("Gaming"); Assert.Equal("Gaming", manager.Active.Name); }
 [Fact] public async Task Mock_provider_reports_realistic_contract_and_handles_missing_sensor() { var provider = new MockHardwareProvider(); var snapshot = await provider.ScanAsync(default); Assert.True(snapshot.Fans.Single().IsControllable); await provider.SetFanAsync("fan", 55, FanMode.ManualPwm, default); Assert.Equal(55, provider.LastRequestedSpeed); await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SetFanAsync("gone", 50, FanMode.ManualPwm, default)); }
}
