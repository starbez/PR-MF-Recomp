#!/bin/bash
set -e

echo "=================================================="
echo "RUNNING FULL VALIDATION SUITE (TRACES & FRAMES)"
echo "=================================================="

DOTNET_BIN="dotnet"
if [ -f "tools/dotnet/dotnet" ]; then
    DOTNET_BIN="tools/dotnet/dotnet"
fi

echo "Building solution..."
$DOTNET_BIN build port/PowerRangers.sln -c Release

echo "Executing PR.TraceTest with frame validation..."
$DOTNET_BIN run --project port/PR.TraceTest/PR.TraceTest.csproj -c Release -- --frames

echo "=================================================="
echo "ALL VALIDATIONS PASSED"
echo "=================================================="
