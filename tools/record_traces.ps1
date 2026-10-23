$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path "traces" | Out-Null
New-Item -ItemType Directory -Force -Path "tools/harness/bin" | Out-Null

$javaBin = "C:\Program Files\Eclipse Adoptium\jdk-21.0.1.12-hotspot\bin\java.exe"
$javacBin = "C:\Program Files\Eclipse Adoptium\jdk-21.0.1.12-hotspot\bin\javac.exe"

Write-Host "Compiling Java Harness stubs..."
$javaFiles = Get-ChildItem -Path tools/harness/src -Recurse -Filter *.java | ForEach-Object { $_.FullName }
& $javacBin -d tools/harness/bin $javaFiles

$runs = @(
    @{ lvl=1; char=0; ticks=100; seed=42; script="tools/scripts/idle_100.txt"; name="idle_level1" },
    @{ lvl=1; char=0; ticks=150; seed=42; script="tools/scripts/movement_150.txt"; name="movement_level1" },
    @{ lvl=1; char=0; ticks=200; seed=42; script="tools/scripts/combat_200.txt"; name="combat_level1" },
    @{ lvl=1; char=0; ticks=150; seed=42; script="tools/scripts/survival_150.txt"; name="survival_level1" }
)

Write-Host "Recording ground truth traces from original bytecode..."

foreach ($r in $runs) {
    $lvl = $r.lvl
    $char = $r.char
    $ticks = $r.ticks
    $seed = $r.seed
    $script = $r.script
    $name = $r.name

    Write-Host "Running $name (level $lvl, seed $seed)..."
    & $javaBin -cp "tools/harness/bin;extracted" tools.Harness --level $lvl --character $char --ticks $ticks --seed $seed --script $script --trace "traces/$($name).txt"
    & $javaBin -cp "tools/harness/bin;extracted" tools.Harness --level $lvl --character $char --ticks $ticks --seed $seed --script $script --trace "traces/$($name)_repeat.txt"

    $hash1 = (Get-FileHash "traces/$($name).txt").Hash
    $hash2 = (Get-FileHash "traces/$($name)_repeat.txt").Hash

    if ($hash1 -ne $hash2) {
        Write-Error "Non-deterministic trace in $name!"
        exit 1
    }
    Remove-Item "traces/$($name)_repeat.txt"
    Write-Host "  -> Verified deterministic: traces/$($name).txt"
}

Write-Host "=================================================="
Write-Host "ALL ORIGINAL BYTECODE TRACES VERIFIED DETERMINISTIC"
Write-Host "=================================================="
