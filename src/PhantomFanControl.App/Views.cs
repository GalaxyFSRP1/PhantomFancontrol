using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PhantomFanControl.Core;
namespace PhantomFanControl.App;
public sealed class DashboardView : UserControl
{
    public DashboardView(AppHost host) { var panel = new StackPanel(); panel.Children.Add(new TextBlock { Text = "Live readings are provided only when a hardware provider reports them.", Foreground = System.Windows.Media.Brushes.LightGray, Margin = new Thickness(0, 0, 0, 12) }); var list = new ListView { ItemsSource = host.Sensors, DisplayMemberPath = "Name" }; panel.Children.Add(list); Content = panel; }
}
public sealed class HardwareView : UserControl { public HardwareView(AppHost host) => Content = new TreeView { ItemsSource = host.Devices, DisplayMemberPath = "Name" }; }
public sealed class CurveView : UserControl
{
    public CurveView(AppHost host)
    {
        var panel = new StackPanel(); panel.Children.Add(new TextBlock { Text = "Curve points (linear interpolation)", FontSize = 18 }); var points = host.Configuration.Fans.Values.FirstOrDefault()?.Curve ?? [new(30, 25), new(50, 45), new(70, 75), new(90, 100)]; panel.Children.Add(new ListBox { ItemsSource = points.Select(p => $"{p.Temperature:0}°C  →  {p.Speed:0}%") }); panel.Children.Add(new TextBlock { Text = "Select a temperature source formula in fan configuration, e.g. MAX(CPU, GPU) + 5. Curve outputs remain clamped to the configured minimum and maximum speeds.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 16, 0, 0) }); Content = panel;
    }
}
public sealed class CalibrationView : UserControl
{
    public CalibrationView(AppHost host)
    {
        var panel = new StackPanel(); panel.Children.Add(new TextBlock { Text = "Calibration temporarily changes fan speeds. Ensure cooling is safe before starting.", TextWrapping = TextWrapping.Wrap }); var fan = new ComboBox { ItemsSource = host.Fans, DisplayMemberPath = "Name", Margin = new Thickness(0, 12, 0, 8) }; panel.Children.Add(fan); var start = new Button { Content = "Apply 20% test speed" }; start.Click += async (_, _) => { if (fan.SelectedItem is FanDevice selected) await host.SetFanAsync(selected, 20); }; panel.Children.Add(start); panel.Children.Add(new TextBlock { Text = "Increase in 10% increments, wait for RPM stabilization, then record minimum usable speed, maximum RPM, and any stall point.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) }); Content = panel;
    }
}
public sealed class DiagnosticsView : UserControl
{
    public DiagnosticsView(AppHost host)
    {
        var panel = new StackPanel(); var report = new TextBox { Text = DiagnosticsReport.Create(host.Snapshot, host.Providers), IsReadOnly = true, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Height = 400 }; panel.Children.Add(report); var button = new Button { Content = "Export diagnostics" }; button.Click += async (_, _) => { var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PhantomFanControl-diagnostics.txt"); await File.WriteAllTextAsync(path, report.Text); MessageBox.Show($"Saved diagnostics to {path}"); }; panel.Children.Add(button); Content = panel;
    }
}
public sealed class SettingsView : UserControl
{
    public SettingsView(AppHost host)
    {
        var panel = new StackPanel(); panel.Children.Add(new TextBlock { Text = "General", FontSize = 18 }); var startup = new CheckBox { Content = "Start with Windows", IsChecked = host.Configuration.Settings.StartWithWindows }; panel.Children.Add(startup); panel.Children.Add(new TextBlock { Text = "Monitoring", FontSize = 18, Margin = new Thickness(0, 16, 0, 0) }); panel.Children.Add(new TextBlock { Text = $"Polling rate: {host.Configuration.Settings.PollingMilliseconds} ms" }); panel.Children.Add(new TextBlock { Text = "Fan Control", FontSize = 18, Margin = new Thickness(0, 16, 0, 0) }); panel.Children.Add(new TextBlock { Text = $"Emergency thresholds — CPU: {host.Configuration.Safety.CpuEmergencyC:0}°C, GPU: {host.Configuration.Safety.GpuEmergencyC:0}°C" }); var logs = new Button { Content = "Open Logs Folder" }; logs.Click += (_, _) => { var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhantomFanControl", "logs"); Directory.CreateDirectory(path); Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }; panel.Children.Add(logs); Content = panel;
    }
}
