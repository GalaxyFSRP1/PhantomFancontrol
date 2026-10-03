using System.Management;
using PhantomFanControl.Core;
namespace PhantomFanControl.Hardware;
public sealed class WmiSystemProvider : IHardwareProvider
{
    public string Name => "Windows/WMI"; public bool IsAvailable => OperatingSystem.IsWindows();
    public Task SetFanAsync(string fanId, double percent, FanMode mode, CancellationToken cancellationToken) => throw new NotSupportedException("WMI system inventory does not provide fan control.");
    public Task RestoreFanAsync(string fanId, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<HardwareSnapshot> ScanAsync(CancellationToken cancellationToken)
    { var devices = new List<HardwareNode>(); if (!IsAvailable) return Task.FromResult(new HardwareSnapshot(devices, [], [], DateTimeOffset.Now)); try { using var search = new ManagementObjectSearcher("SELECT Name FROM Win32_ComputerSystem"); foreach (ManagementObject item in search.Get()) devices.Add(new HardwareNode("wmi-system", item["Name"]?.ToString() ?? "Windows system", "System", [])); } catch (ManagementException) { } return Task.FromResult(new HardwareSnapshot(devices, [], [], DateTimeOffset.Now)); }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
