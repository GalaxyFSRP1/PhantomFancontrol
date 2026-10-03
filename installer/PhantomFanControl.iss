#define AppName "Phantom Fan Control"
#define AppVersion "1.0.0"
#ifndef PublishDir
  #error PublishDir must be supplied by scripts/build-installer.ps1
#endif
[Setup]
AppId={{2BE357C3-C296-4AA1-9640-E0737F629A7A}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\Phantom Fan Control
DefaultGroupName={#AppName}
OutputBaseFilename=PhantomFanControlSetup
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2
SolidCompression=yes
UninstallDisplayIcon={app}\PhantomFanControl.exe
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\PhantomFanControl.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\PhantomFanControl.exe"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; Flags: unchecked
