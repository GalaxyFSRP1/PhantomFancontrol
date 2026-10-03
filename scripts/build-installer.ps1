[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/build-release.ps1"
$root = Split-Path $PSScriptRoot -Parent; $release = Join-Path $root 'Release'; New-Item $release -ItemType Directory -Force | Out-Null
$iscc = Get-Command iscc.exe -ErrorAction SilentlyContinue
if (-not $iscc) { throw 'Inno Setup 6 (iscc.exe) is required to create PhantomFanControlSetup.exe.' }
& $iscc.Source (Join-Path $root 'installer/PhantomFanControl.iss') "/DPublishDir=$(Join-Path $root 'artifacts/publish')" "/O$release"
