using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PhantomFanControl.Core;
using PhantomFanControl.Hardware;
namespace PhantomFanControl.App;
public partial class MainWindow : Window
{
    private readonly AppHost _host; private readonly System.Windows.Forms.NotifyIcon _tray; public string PageTitle { get; set; } = "Live dashboard";
    public MainWindow() { InitializeComponent(); DataContext = this; _host = new AppHost(); _tray = CreateTrayIcon(); Loaded += async (_, _) => { await _host.StartAsync(); Profiles.ItemsSource = _host.Profiles; Profiles.SelectedItem = _host.ProfileManager.Active; ShowDashboard(); }; Closed += async (_, _) => { _tray.Dispose(); await _host.DisposeAsync(); }; StateChanged += (_, _) => { if (WindowState == WindowState.Minimized && _host.Configuration.Settings.TrayIcon) Hide(); }; }
    private System.Windows.Forms.NotifyIcon CreateTrayIcon()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip(); menu.Items.Add("Open", null, (_, _) => Dispatcher.Invoke(Show)); menu.Items.Add("Pause Fan Control"); menu.Items.Add("Exit", null, (_, _) => Dispatcher.Invoke(Close));
        return new System.Windows.Forms.NotifyIcon { Text = "Phantom Fan Control — awaiting hardware readings", Visible = true, ContextMenuStrip = menu, Icon = System.Drawing.SystemIcons.Application };
    }
    private void Dashboard_Click(object sender, RoutedEventArgs e) => ShowDashboard(); private void Hardware_Click(object sender, RoutedEventArgs e) => ShowHardware(); private void Curves_Click(object sender, RoutedEventArgs e) => ShowCurves(); private void Calibration_Click(object sender, RoutedEventArgs e) => ShowCalibration(); private void Diagnostics_Click(object sender, RoutedEventArgs e) => ShowDiagnostics(); private void Settings_Click(object sender, RoutedEventArgs e) => ShowSettings();
    private void Profile_Changed(object sender, SelectionChangedEventArgs e) { if (Profiles.SelectedItem is Profile profile) _host.ProfileManager.Activate(profile.Name); }
    private void SetPage(string title, object content) { PageTitle = title; Page.Content = content; DataContext = null; DataContext = this; }
    private void ShowDashboard() => SetPage("Live dashboard", new DashboardView(_host));
    private void ShowHardware() => SetPage("Hardware & sensors", new HardwareView(_host));
    private void ShowCurves() => SetPage("Fan curve editor", new CurveView(_host));
    private void ShowCalibration() => SetPage("Fan calibration", new CalibrationView(_host));
    private void ShowDiagnostics() => SetPage("Diagnostics", new DiagnosticsView(_host));
    private void ShowSettings() => SetPage("Settings", new SettingsView(_host));
}
