[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$publish = Join-Path $root 'artifacts/publish'
Remove-Item $publish -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish (Join-Path $root 'src/PhantomFanControl.App/PhantomFanControl.App.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $publish
Write-Host "Self-contained application published to $publish"
