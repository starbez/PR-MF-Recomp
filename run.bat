@echo off
setlocal
cd /d "%~dp0"

if exist "port\PR.Game\bin\Release\net8.0\PR.Game.exe" (
    "port\PR.Game\bin\Release\net8.0\PR.Game.exe" %*
) else if exist "tools\dotnet\dotnet.exe" (
    "tools\dotnet\dotnet.exe" run --project port\PR.Game\PR.Game.csproj -c Release -- %*
) else (
    dotnet run --project port\PR.Game\PR.Game.csproj -c Release -- %*
)

endlocal
