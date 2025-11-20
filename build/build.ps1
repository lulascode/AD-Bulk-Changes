# AD Bulk Changes - Build Script
# Kompiliert die Anwendung als standalone EXE

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SourceDir = Join-Path $ProjectRoot "src"
$OutputDir = Join-Path $ProjectRoot "Deploy"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  AD Bulk Changes - Build Tool" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Pruefe ob .NET 8.0 SDK installiert ist
Write-Host "[1/5] Pruefe .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "X .NET 8.0 SDK nicht gefunden!" -ForegroundColor Red
    Write-Host "  Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}
Write-Host "OK .NET SDK gefunden: $dotnetVersion" -ForegroundColor Green

# Clean Build (optional)
if ($Clean) {
    Write-Host "[2/5] Raeume Build-Verzeichnisse auf..." -ForegroundColor Yellow
    Push-Location $SourceDir
    dotnet clean --configuration $Configuration
    Pop-Location
    Write-Host "OK Clean abgeschlossen" -ForegroundColor Green
} else {
    Write-Host "[2/5] Ueberspringe Clean (verwende -Clean um aufzuraeumen)" -ForegroundColor Gray
}

# Restore NuGet Packages
Write-Host "[3/5] Lade NuGet-Pakete..." -ForegroundColor Yellow
Push-Location $SourceDir
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "X Restore fehlgeschlagen!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "OK NuGet-Pakete geladen" -ForegroundColor Green

# Build & Publish
Write-Host "[4/5] Kompiliere Anwendung ($Configuration)..." -ForegroundColor Yellow
Push-Location $SourceDir
dotnet publish `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    --output (Join-Path $OutputDir "temp")

if ($LASTEXITCODE -ne 0) {
    Write-Host "X Build fehlgeschlagen!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "OK Kompilierung erfolgreich" -ForegroundColor Green

# Kopiere EXE in Deploy-Ordner
Write-Host "[5/5] Erstelle finale Ausgabe..." -ForegroundColor Yellow
$TempExe = Join-Path $OutputDir "temp\AD-BulkChanges.exe"
$FinalExe = Join-Path $OutputDir "AD-BulkChanges.exe"

if (Test-Path $TempExe) {
    Copy-Item $TempExe $FinalExe -Force
    Remove-Item (Join-Path $OutputDir "temp") -Recurse -Force -ErrorAction SilentlyContinue
    
    $ExeInfo = Get-Item $FinalExe
    $SizeMB = [math]::Round($ExeInfo.Length / 1MB, 2)
    
    Write-Host "OK Build abgeschlossen!" -ForegroundColor Green
    Write-Host ""
    Write-Host "==================================" -ForegroundColor Cyan
    Write-Host "  Ausgabe: $FinalExe" -ForegroundColor White
    Write-Host "  Groesse: $SizeMB MB" -ForegroundColor White
    Write-Host "==================================" -ForegroundColor Cyan
} else {
    Write-Host "X EXE nicht gefunden!" -ForegroundColor Red
    exit 1
}
