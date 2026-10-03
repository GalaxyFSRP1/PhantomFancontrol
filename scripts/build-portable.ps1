[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/build-release.ps1"
$root = Split-Path $PSScriptRoot -Parent; $release = Join-Path $root 'Release'; New-Item $release -ItemType Directory -Force | Out-Null
$stage = Join-Path $root 'artifacts/portable/PhantomFanControl'; Remove-Item (Split-Path $stage) -Recurse -Force -ErrorAction SilentlyContinue; New-Item $stage -ItemType Directory -Force | Out-Null
Copy-Item (Join-Path $root 'artifacts/publish/*') $stage -Recurse; Copy-Item (Join-Path $root 'README.md') $stage
Compress-Archive -Path $stage -DestinationPath (Join-Path $release 'PhantomFanControl-Portable.zip') -Force
