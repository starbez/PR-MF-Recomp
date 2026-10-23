$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path "frames" | Out-Null
$javaBin = "C:\Program Files\Eclipse Adoptium\jdk-21.0.1.12-hotspot\bin\java.exe"

Write-Host "Dumping reference frames from original bytecode..."
& $javaBin -cp "tools/harness/bin;extracted" tools.Harness --level 1 --character 0 --ticks 150 --seed 42 --script "tools/scripts/movement_150.txt" --frames "frames/level1_movement"
Write-Host "Frames dumped to frames/level1_movement/"
