$ErrorActionPreference = "Stop"

Write-Host "=================================================="
Write-Host "RUNNING FULL VALIDATION SUITE (TRACES & FRAMES)"
Write-Host "=================================================="

$dotnetBin = if (Test-Path "tools/dotnet/dotnet.exe") { "tools/dotnet/dotnet.exe" } else { "dotnet" }

Write-Host "Building solution..."
& $dotnetBin build port/PowerRangers.sln -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Executing PR.TraceTest with frame validation..."
& $dotnetBin run --project port/PR.TraceTest/PR.TraceTest.csproj -c Release -- --frames
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=================================================="
Write-Host "ALL VALIDATIONS PASSED"
Write-Host "=================================================="
