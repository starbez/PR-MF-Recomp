#!/usr/bin/env bash
set -e
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if command -v dotnet >/dev/null 2>&1; then
    DOTNET_BIN="dotnet"
elif [ -f "$ROOT_DIR/tools/dotnet/dotnet.exe" ]; then
    DOTNET_BIN="$ROOT_DIR/tools/dotnet/dotnet.exe"
elif [ -f "$ROOT_DIR/tools/dotnet/dotnet" ]; then
    DOTNET_BIN="$ROOT_DIR/tools/dotnet/dotnet"
else
    echo "dotnet not found"
    exit 1
fi

"$DOTNET_BIN" run --project "$ROOT_DIR/port/PR.TraceTest"
