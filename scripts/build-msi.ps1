[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/build-release.ps1"
$root = Split-Path $PSScriptRoot -Parent; $release = Join-Path $root 'Release'; New-Item $release -ItemType Directory -Force | Out-Null
wix build (Join-Path $root 'installer/PhantomFanControl.wxs') -arch x64 -d PublishDir=(Join-Path $root 'artifacts/publish') -o (Join-Path $release 'PhantomFanControl.msi')
