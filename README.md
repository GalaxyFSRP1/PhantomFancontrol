# Phantom Fan Control

**Phantom Fan Control** is a safety-first Windows 10/11 x64 desktop application for monitoring compatible cooling hardware and applying user-defined fan curves. It is built on .NET 8 WPF and LibreHardwareMonitor, with provider, safety, profile, configuration, and diagnostics abstractions designed for future hardware integrations.

> **Safety notice:** Fan control is inherently hardware-specific. This application only exposes a control when a provider explicitly reports that control as writable; a detected RPM sensor is **not** assumed controllable. Motherboard firmware, GPU drivers, and hardware thermal protection remain the primary safety mechanisms. Review curves and emergency thresholds before enabling automatic control.

## Supported hardware and limitations

The included `LibreHardwareMonitorProvider` reads sensors from CPU, mainboard, GPU, memory, storage and supported controllers using LibreHardwareMonitor. It can request software control only for controls exposed as writable by that library. A Windows WMI provider supplies system identity only; it does not pretend to control fans. Hardware that is not exposed by a provider is presented as unavailable/read-only.

Actual support varies by motherboard Super I/O chip, BIOS, driver, GPU, USB controller and permissions. Run as administrator when your hardware/driver requires it. GPU vendor SDK, vendor-specific motherboard, and USB AIO providers are extension points and are intentionally not simulated.

## Build prerequisites

Build on Windows 10/11 x64 with the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Installer builds additionally require WiX Toolset v4 (`dotnet tool install --global wix`). The release publish is self-contained, so end users do not need .NET.

```powershell
./scripts/build-release.ps1
./scripts/build-portable.ps1
./scripts/build-msi.ps1
./scripts/build-installer.ps1
./scripts/build-all.ps1
```

`build-all.ps1` writes these release artifacts when WiX and Inno Setup are installed:

```
Release/
  PhantomFanControlSetup.exe
  PhantomFanControl.msi
  PhantomFanControl-Portable.zip
```

An MST is not generated because no transform is required for normal deployment. Enterprise administrators may apply their own transform to the MSI without affecting application behavior.

## Development and verification

```powershell
dotnet restore PhantomFanControl.sln
dotnet build PhantomFanControl.sln -c Release
dotnet test PhantomFanControl.sln -c Release
dotnet run --project src/PhantomFanControl.App
```

The tests use mock providers and do not access physical hardware. On a clean Windows VM, install `Release/PhantomFanControlSetup.exe`, start the app from Start Menu, and run `Release/PhantomFanControl/PhantomFanControl.exe` from the portable ZIP. Confirm a `%LocalAppData%/PhantomFanControl/configuration.json` file is persisted after changing a setting.

## Architecture

* `src/PhantomFanControl.Core` — contracts, curves, formula evaluation, safety, smoothing, profiles, JSON configuration, and diagnostics.
* `src/PhantomFanControl.Hardware` — LibreHardwareMonitor and WMI providers plus safe aggregate discovery.
* `src/PhantomFanControl.App` — WPF dashboard, device tree, settings, curve editor, calibration workflow, system tray, logging and host services.
* `tests/PhantomFanControl.Tests` — hardware-independent unit tests and mock provider.
* `installer` and `scripts` — WiX MSI, Inno Setup bootstrapper, self-contained portable archive, and release automation.

Logs are stored in `%LocalAppData%/PhantomFanControl/logs`; configuration is in `%LocalAppData%/PhantomFanControl/configuration.json`. Use the app's **Open Logs Folder**, **Export diagnostics**, and configuration import/export controls for support and recovery.
