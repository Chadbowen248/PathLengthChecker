<#
.SYNOPSIS
  Build portable Windows release binaries (GUI + CLI), same idea as the upstream release .exe.

.DESCRIPTION
  Produces self-contained win-x64 single-file apps under .\artifacts\ so you can
  double-click PathLengthCheckerGUI.exe without installing the .NET SDK on the target PC.

  Must be run on Windows (WPF GUI cannot build on macOS/Linux).

.EXAMPLE
  .\build\build-release.ps1

.EXAMPLE
  .\build\build-release.ps1 -Configuration Release -Runtime win-x64
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$Runtime = 'win-x64',

    [string]$OutputRoot = '',

    [switch]$FrameworkDependent
)

$ErrorActionPreference = 'Stop'

# Guard: WPF publish only works on Windows (Windows PowerShell 5.1 has no $IsWindows).
$onWindows = ($env:OS -like '*Windows*') -or ($PSVersionTable.PSEdition -eq 'Desktop') -or ($IsWindows -eq $true)
if (-not $onWindows) {
    Write-Error 'This script must be run on Windows to build the WPF GUI.'
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
if (-not $OutputRoot) {
    $OutputRoot = Join-Path $repoRoot 'artifacts'
}

$guiProject = Join-Path $repoRoot 'src\PathLengthCheckerGUI\PathLengthCheckerGUI.csproj'
$cliProject = Join-Path $repoRoot 'src\PathLengthChecker\PathLengthChecker.csproj'
$guiOut = Join-Path $OutputRoot "gui-$Runtime"
$cliOut = Join-Path $OutputRoot "cli-$Runtime"

Write-Host "Repo root : $repoRoot" -ForegroundColor Cyan
Write-Host "Output    : $OutputRoot" -ForegroundColor Cyan
Write-Host "Runtime   : $Runtime" -ForegroundColor Cyan
Write-Host "Self-cont : $(-not $FrameworkDependent)" -ForegroundColor Cyan

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "dotnet SDK not found. Install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0"
}

Write-Host "`n==> dotnet --info (sdk)" -ForegroundColor DarkGray
& dotnet --list-sdks

$selfContained = if ($FrameworkDependent) { 'false' } else { 'true' }

$commonArgs = @(
    '-c', $Configuration,
    '-r', $Runtime,
    '--self-contained', $selfContained,
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableCompressionInSingleFile=true',
    '-p:DebugType=None',
    '-p:DebugSymbols=false'
)

function Publish-Project {
    param(
        [string]$Project,
        [string]$OutDir,
        [string]$Label
    )
    if (Test-Path $OutDir) {
        Remove-Item -Recurse -Force $OutDir
    }
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

    Write-Host "`n==> Publishing $Label" -ForegroundColor Cyan
    Write-Host "    $Project" -ForegroundColor DarkGray
    Write-Host "    -> $OutDir" -ForegroundColor DarkGray

    & dotnet publish $Project @commonArgs -o $OutDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Label (exit $LASTEXITCODE)"
    }
}

Publish-Project -Project $cliProject -OutDir $cliOut -Label 'CLI (PathLengthChecker.exe)'
Publish-Project -Project $guiProject -OutDir $guiOut -Label 'GUI (PathLengthCheckerGUI.exe)'

# Zip packages for easy sharing / GitHub releases
$guiZip = Join-Path $OutputRoot "PathLengthCheckerGUI-$Runtime.zip"
$cliZip = Join-Path $OutputRoot "PathLengthChecker-CLI-$Runtime.zip"
$bundleZip = Join-Path $OutputRoot "PathLengthChecker-$Runtime.zip"

if (Test-Path $guiZip) { Remove-Item -Force $guiZip }
if (Test-Path $cliZip) { Remove-Item -Force $cliZip }
if (Test-Path $bundleZip) { Remove-Item -Force $bundleZip }

Write-Host "`n==> Creating zip packages" -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $guiOut '*') -DestinationPath $guiZip
Compress-Archive -Path (Join-Path $cliOut '*') -DestinationPath $cliZip

$bundleDir = Join-Path $OutputRoot "_bundle"
if (Test-Path $bundleDir) { Remove-Item -Recurse -Force $bundleDir }
New-Item -ItemType Directory -Path (Join-Path $bundleDir 'GUI') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $bundleDir 'CLI') -Force | Out-Null
Copy-Item (Join-Path $guiOut '*') (Join-Path $bundleDir 'GUI') -Recurse
Copy-Item (Join-Path $cliOut '*') (Join-Path $bundleDir 'CLI') -Recurse
Compress-Archive -Path (Join-Path $bundleDir '*') -DestinationPath $bundleZip
Remove-Item -Recurse -Force $bundleDir

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Build complete." -ForegroundColor Green
Write-Host "GUI exe : $(Join-Path $guiOut 'PathLengthCheckerGUI.exe')" -ForegroundColor Green
Write-Host "CLI exe : $(Join-Path $cliOut 'PathLengthChecker.exe')" -ForegroundColor Green
Write-Host "GUI zip : $guiZip" -ForegroundColor Green
Write-Host "CLI zip : $cliZip" -ForegroundColor Green
Write-Host "All zip : $bundleZip" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green
Write-Host "Double-click PathLengthCheckerGUI.exe to run (like the original release)."
