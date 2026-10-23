# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - Phase 7 Complete (PC Port Finished)
### Added
- Native PC frontend desktop application `port/PR.Game` (.NET 8 + Raylib-cs 8.1.0):
  - Integer scaling (1x to 5x, default 3x -> 720x960), fullscreen toggle (F11), letterboxing/pillarboxing.
  - Native Windows MCI MIDI audio player (`MidiPlayer`) for Title music (`sound_1.mid`) and Level music (`sound_2.mid`) with volume control, looping, and mute toggle.
  - Configurable `InputRouter` mapping Keyboard & Gamepad to J2ME key codes, saved to `%APPDATA%/PowerRangers/bindings.json`.
  - Save manager `SaveManager` with `%APPDATA%/PowerRangers/save.json` persisting options, unlock state, and Top 5 High Scores.
  - Complete menu system `MenuSystem`: Title screen, Main menu, Ranger character select (Nick, Chip, Madison, Vida, Xander) with stat meters, Level select (1-12), Survival mode, High Scores hall of fame, Options menu, Controls & help overlay, Pause menu, Game Over and High Score initials entry.
  - Crash handler writing full call stack to `crash.log`.
  - Comprehensive CLI options: `--scale N`, `--no-sound`, `--fullscreen`, `--lang <xx>`, `--jar <path>`, `--logs`, `--help`.
- Unified automated test runner `tools/run_tests.sh` and `tools/run_tests.ps1` verifying both traces and frames.

## [0.6.0] - Phase 6 Complete
### Added
- Clean-room 240x320 software renderer in `port/PR.Core/Graphics/`:
  - `Image`: 32-bit ARGB software surface with StbImageSharp / StbImageWriteSharp decoding and encoding.
  - `Graphics`: Complete MIDP 2.0 graphics engine supporting `Translate`, `SetClip`, `ClipRect`, `FillRect`, `DrawRect`, `DrawLine`, `DrawImage`, `DrawRegion` (all MIDP 0..7 transforms and 90/180/270 angle conventions), and alpha compositing.
  - `GMIDlet`: Living Mobile resource stream unpacker, font metric atlas loading, proportional bitmap font glyph renderer, word wrapping (`WordWrap`), and multi-layer accent/umlaut drawing.
- 100% bit-for-bit pixel-identical verification against ground truth reference frames dumped from the original Java engine (76,800/76,800 pixels matching on all 7 reference frames).

## [0.5.0] - Phase 5 Complete
### Added
- Literal C# port of simulation logic in `port/PR.Core`:
  - `JavaRandom`, `DataReader`, `DataWriter`, `Actor`, `TileMap`, `App`, `Game`.
- Verified 100% bit-identical line-for-line trace validation against original Java bytecode across 4 test scenarios (608 total ticks) with negative controls.
### Added
- Clean-room Java MIDP 2.0 / MMAPI harness in `tools/harness/`:
  - `MIDlet`, `Displayable`, `Display`, `Canvas`, `Font`, `Image`, `Graphics`, `RecordStore`, `Manager`, `Player`, `VolumeControl`, `Controllable`.
- Zero-sleep bytecode clock patching via `tools.HarnessClk` and `tools.HarnessClassLoader`.
- Input scripts in `tools/scripts/`: `idle_100.txt`, `movement_150.txt`, `combat_200.txt`, `survival_150.txt`.
- Verification scripts `tools/record_traces.sh` / `tools/record_traces.ps1` proving identical trace output across repeated executions.
- Reference frame capture script `tools/record_frames.sh` / `tools/record_frames.ps1` dumping frames to `frames/`.
- Authored validation documentation in `docs/VALIDATION.md`.
- Updated status notes in `docs/STATUS.md`.

## [0.3.0] - Phase 3 Complete
### Added
- Independent Python format parsers in `tools/`:
  - `decode_text.py`: `lang` and `text__*.txt` (6 string arrays per language, 222 strings each).
  - `parse_pngs.py`: Length-prefixed PNG extraction (`lm`, `lp__*`, `lpok`, `subtitle__*`, `bg*`, `font`, `i.png`).
  - `parse_sprites.py`: All 55 sprite files `s0..s64` and their PLTE palette variants.
  - `parse_blocks.py`: 13 block definition files `b0..b12` (16x16 tile properties and parallax layers).
  - `parse_level.py`: 13 level layout files `l0..l12` (actors, triggers, hazards, splines).
  - `parse_pack.py`: Global packs `fr` (660 rects), `spr` (40 animation definitions), and master pack `l`.
- `tools/test_parsers.py`: Automated test suite verifying 107/107 files parsed with 0 unconsumed bytes.
- Extracted and verified both MIDI tracks from `l` into `sons/` (`sound_1.mid`, `sound_2.mid`).
- Complete binary format specification authored in `docs/FORMATS.md`.
- Status report recorded in `docs/STATUS.md`.

## [0.2.0] - Phase 2 Complete
### Added
- Phase 2 Code Reading & Annotation completed in full:
  - Audited all 8 classes: `GMIDlet`, `g`, `f`, `a`, `b`, `c`, `d`, `e`.
  - Built comprehensive field maps and method maps for every class in `docs/PowerRangers-analysis.md`.
  - Resolved all 5 CFR trouble spots in `g` (`q()`, `h()`, `i()`, `k()`, `a(DataOutputStream, ByteArrayOutputStream, boolean)`) directly against bytecode.
  - Documented timing loops (70 ms fixed step in `f` and `g`, 100 ms repaint thread in `d`).
  - Documented input mapping, cheat sequences (`#56*`, `#44*`, `#22*`, etc.), and softkey pause mechanism.
  - Documented pseudo-random number generator usage (`java.util.Random` LCG).
  - Documented RecordStore RMS `"op"` persistence and reverse-byte packing quirk.
  - Generated application state machine diagram in Mermaid.
  - Documented entire gameplay pipeline (`input -> update -> collide -> draw`), combat systems, magic gauges, enemy AI, and camera.

## [0.1.0] - Phase 1 Complete
### Added
- Git repository initialized with strict `.gitignore` rules (no JARs, extracted assets, decompiled code, or local tool binaries committed).
- Environment verification: Java OpenJDK 21, Python 3.12, .NET 8.0 SDK portable in `tools/dotnet/`, CFR 0.152 in `tools/cfr.jar`.
- Extracted `JAR/Power_Rangers_240_nok.jar` into `extracted/` (106 game resource files + `META-INF/MANIFEST.MF`).
- Decompiled all 8 classes + `GMIDlet` with CFR 0.152 into `decompiled/`.
- Disassembled all classes into bytecode text with `javap -c -p -constants` into `decompiled/bytecode/`.
- Re-verified all claims in Section 1 of brief with `[verified]` tags.
- Authored initial analysis document `docs/PowerRangers-analysis.md`.
