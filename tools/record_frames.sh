#!/bin/bash
set -e

mkdir -p frames/level1_movement
echo "Dumping reference frames from original bytecode..."
java -cp "tools/harness/bin:extracted" tools.Harness --level 1 --character 0 --ticks 150 --seed 42 --script "tools/scripts/movement_150.txt" --frames "frames/level1_movement"
echo "Frames dumped to frames/level1_movement/"
