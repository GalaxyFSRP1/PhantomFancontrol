using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PhantomFanControl.Core;
namespace PhantomFanControl.App;
public sealed class DashboardView : UserControl
{
    public DashboardView(AppHost host)
    {
        var layout = new Grid(); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); layout.RowDefinitions.Add(new RowDefinition()); layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); layout.RowDefinitions.Add(new RowDefinition());
        var notice = new TextBlock { Text = "All live temperatures and other values that LibreHardwareMonitor exposes are listed below. Values are never invented; unavailable hardware has no reading.", Foreground = System.Windows.Media.Brushes.LightGray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) }; layout.Children.Add(notice);
        var temperaturesTitle = new TextBlock { Text = "All available temperatures", FontSize = 17, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 8) }; Grid.SetRow(temperaturesTitle, 1); layout.Children.Add(temperaturesTitle);
        var temperatures = CreateReadingsGrid(host, SensorKind.Temperature); Grid.SetRow(temperatures, 2); layout.Children.Add(temperatures);
        var telemetryTitle = new TextBlock { Text = "All available telemetry (load, power, voltage, RPM and controls)", FontSize = 17, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 18, 0, 8) }; Grid.SetRow(telemetryTitle, 3); layout.Children.Add(telemetryTitle);
        var allReadings = CreateReadingsGrid(host, null); Grid.SetRow(allReadings, 4); layout.Children.Add(allReadings); Content = layout;
    }
    private static DataGrid CreateReadingsGrid(AppHost host, SensorKind? kind)
    {
        var source = new System.Windows.Data.CollectionViewSource { Source = host.Sensors }.View; source.Filter = item => item is SensorReading sensor && (kind is null ? sensor.Kind != SensorKind.Temperature : sensor.Kind == kind);
        var grid = new DataGrid { ItemsSource = source, IsReadOnly = true, AutoGenerateColumns = false, CanUserAddRows = false, HeadersVisibility = DataGridHeadersVisibility.Column, Background = System.Windows.Media.Brushes.Transparent, Foreground = System.Windows.Media.Brushes.White };
        grid.Columns.Add(new DataGridTextColumn { Header = "Sensor", Binding = new System.Windows.Data.Binding(nameof(SensorReading.Name)), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Value", Binding = new System.Windows.Data.Binding(nameof(SensorReading.Value)) { StringFormat = "0.##" }, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Unit", Binding = new System.Windows.Data.Binding(nameof(SensorReading.Unit)), Width = new DataGridLength(0.8, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Device", Binding = new System.Windows.Data.Binding(nameof(SensorReading.DeviceId)), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        return grid;
    }
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
