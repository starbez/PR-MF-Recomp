#!/bin/bash
set -e

mkdir -p traces
mkdir -p tools/harness/bin

# Compile harness
javac -d tools/harness/bin $(find tools/harness/src -name "*.java")

RUNS=(
    "1 0 100 42 tools/scripts/idle_100.txt idle_level1"
    "1 0 150 42 tools/scripts/movement_150.txt movement_level1"
    "1 0 200 42 tools/scripts/combat_200.txt combat_level1"
    "1 0 150 42 tools/scripts/survival_150.txt survival_level1"
)

echo "Recording ground truth traces from original bytecode..."

for r in "${RUNS[@]}"; do
    read -r lvl char ticks seed script name <<< "$r"
    echo "Running $name (level $lvl, seed $seed)..."
    java -cp "tools/harness/bin:extracted" tools.Harness --level "$lvl" --character "$char" --ticks "$ticks" --seed "$seed" --script "$script" --trace "traces/${name}.txt"
    java -cp "tools/harness/bin:extracted" tools.Harness --level "$lvl" --character "$char" --ticks "$ticks" --seed "$seed" --script "$script" --trace "traces/${name}_repeat.txt"
    diff "traces/${name}.txt" "traces/${name}_repeat.txt"
    rm -f "traces/${name}_repeat.txt"
    echo "  -> Verified deterministic: traces/${name}.txt"
done

echo "=================================================="
echo "ALL ORIGINAL BYTECODE TRACES VERIFIED DETERMINISTIC"
echo "=================================================="
