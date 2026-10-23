# Project TODO

## Phase 1: Environment & Extraction [DONE]
- [x] Configure `.gitignore` to prevent any game assets or decompiled code leaks.
- [x] Verify Java 11+, Python 3, and install portable .NET 8 SDK.
- [x] Download CFR 0.152 decompiler.
- [x] Extract `JAR/Power_Rangers_240_nok.jar`.
- [x] Decompile all classes using CFR.
- [x] Disassemble all classes to bytecode (`javap`).
- [x] Verify claims in Section 1 and initialize `docs/PowerRangers-analysis.md`.

## Phase 2: Code Reading & Annotation [DONE]
- [x] Map obfuscated fields and methods for:
  - [x] `GMIDlet`
  - [x] `g` (Canvas, main loop, state machine, RMS, text)
  - [x] `a` (Sprite & animation manager)
  - [x] `e` (Audio engine & MIME player)
  - [x] `d` (Menu manager & forms)
  - [x] `b` (UI widget items)
  - [x] `c` (Tilemap, camera, tile collisions)
  - [x] `f` (Gameplay, player physics, enemies, bosses, weapons)
- [x] Resolve CFR trouble spots (`q`, `h`, `i`, `k`, `a`) from bytecode in `g`.
- [x] Document loop timing, key mappings, RNG order, RecordStore format.
- [x] Generate Mermaid state diagram for `g`.
- [x] Document gameplay tick pipeline in `f`.

## Phase 3: Resource Formats & Independent Parsers [DONE]
- [x] Parse `lang` and `text__*.txt`.
- [x] Parse length-prefixed PNGs (`lm`, `lp__*`, `lpok`, `subtitle__*`, `bg*`, `font`, `i.png`).
- [x] Parse `s0..s64` sprite formats and palette swap flags.
- [x] Parse `b0..b12` block tables (16x16 tiles).
- [x] Parse `l0..l12` level layouts and entities.
- [x] Parse `l` master pack, `spr`, and `fr`.
- [x] Extract and verify audio blobs from `l`.
- [x] Document all formats in `docs/FORMATS.md`.
- [x] Automated verification suite `tools/test_parsers.py` passes 107/107 files.

## Phase 4: Headless Validation Harness [DONE]
- [x] Build MIDP stubs in Java (`tools/harness/`).
- [x] Deterministic clock (70ms tick) & fixed RNG seeding.
- [x] State trace recorder & frame dumper.
- [x] Input scripts generated (`tools/scripts/`).
- [x] Record ground truth traces and verify identical repeated runs.
- [x] Record reference frames.

## Phase 5: C# Core Logic Port [DONE]
- [x] Implement `PR.Core` replicating exact logic.
- [x] Match traces 100% line-for-line across all 4 scenarios (608 ticks) against original Java bytecode harness.
- [x] Verified negative controls detect divergence on seed/input deviation.

## Phase 6: Software Renderer & Frame Validation [DONE]
- [x] Clean-room 240x320 software rasterizer in `PR.Core/Graphics/`: `Image`, `Graphics`, `GMIDlet`.
- [x] Parallax backgrounds, tilemaps, sprite sequences, animation offsets, HUD, word wrapping, accents/umlauts.
- [x] 100% bit-for-bit pixel-identical verification against all 7 reference frames (76,800/76,800 pixels matching).

## Phase 7: Desktop Frontend & Release [DONE]
- [x] Raylib frontend desktop app (`PR.Game`).
- [x] Integer scaling (1x..5x), fullscreen toggle (F11), letterboxing/pillarboxing.
- [x] Native Windows MCI MIDI audio player (`MidiPlayer`) playing `sound_1.mid` and `sound_2.mid`.
- [x] Remappable Keyboard & Gamepad `InputRouter` with `%APPDATA%/PowerRangers/bindings.json`.
- [x] `SaveManager` with `%APPDATA%/PowerRangers/save.json` persisting settings & High Scores.
- [x] Full menu system `MenuSystem` (Title, Main Menu, Ranger Select with stats, Level Select 1-12, Survival, High Scores, Options, Controls & Help, Pause, Game Over & Name Entry).
- [x] Unified automated test suite `tools/run_tests.sh` and `tools/run_tests.ps1` passing with `ALL VALIDATIONS PASSED`.
