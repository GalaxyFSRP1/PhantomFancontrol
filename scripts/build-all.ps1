[CmdletBinding()] param()
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/build-portable.ps1"
& "$PSScriptRoot/build-msi.ps1"
& "$PSScriptRoot/build-installer.ps1"
Write-Host 'Release artifacts created in Release/. No MST is generated because no transform is necessary.'
