$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $PSScriptRoot

$dotnetBin = "dotnet"
if (Test-Path "$rootDir\tools\dotnet\dotnet.exe") {
    $dotnetBin = "$rootDir\tools\dotnet\dotnet.exe"
}

& $dotnetBin run --project "$rootDir\port\PR.TraceTest"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
