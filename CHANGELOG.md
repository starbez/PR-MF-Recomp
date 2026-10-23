# Changelog

All notable changes to this project will be documented in this file.

## [1.4.0] - Upgrades & Fixes: Task 4 (Modern Settings, Rebinding & Pause Layer)
### Added
- Modern Settings & Options Layer:
  - Sound on/off toggle and master volume slider (0% to 100% in 10% steps).
  - Dynamic in-game language selection (DE, EN, FR, IT) reloading localized text strings in real time via `App.LoadLanguage()`.
  - Frame rate mode toggle: `SMOOTH (60)` vs `ORIGINAL (14)`.
  - Screen mode toggle: `WIDESCREEN (16:9)` vs `ORIGINAL (4:3)`.
  - Display scaling mode toggle: `INTEGER` scaling vs aspect-preserved `FIT` scaling.
  - Window scale selector (1X to 6X) and Fullscreen toggle (`YES` / `NO`).
  - Key & Gamepad Rebinding sub-menu: interactive visual remap for all 8 game actions with modal key/button capture.
  - "Reset to Defaults" action restoring all audio, display, language, and input bindings in one click.
- Enhanced Pause Menu (Esc / Soft Right):
  - In-game pause overlay with `RESUME`, `SETTINGS`, `REBIND CONTROLS`, and `QUIT TO TITLE`.
  - Seamless navigation between pause and settings without losing game progress.
- Comprehensive Mouse & Gamepad Navigation:
  - Dynamic coordinate translation (`GameView.ScreenToVirtual()`) enabling point-and-click and hover selection at any window size and aspect ratio.
  - Full Gamepad navigation support with D-Pad, Left Stick, A (select), B (cancel/back), and Start (pause).
- Art & Font Preservation:
  - All modern menu screens and overlays rendered using original proportional bitmap fonts (`GMIDlet.a()`), gold border frames, and Living Mobile crimson UI style.

## [1.3.0] - Upgrades & Fixes: Task 3 (Widescreen 16:9 Extension)
### Added
- Extended internal rendering resolution to 16:9 aspect ratio (568x320) via `EngineTweaks.Widescreen`:
  - Shows more of the level on left and right without stretching original pixels.
  - Parallax backgrounds and color fills automatically extended to fill the full 568 px width; background images tile horizontally to avoid empty bars.
  - Tilemap renderer (`TileMap.void_b`) extended to render all columns across the wider viewport.
  - Camera-based activation rectangle (`Game.boolean_a(Actor)`) strictly preserved at original $[-140, +140]$ relative to camera center `ac`, ensuring zero change to enemy spawn, despawn, or AI triggers.
  - HUD elements anchored cleanly: player health and energy anchored to top-left, score display centered at screen midpoint, boss health bar centered, dialogue/subtitles centered, softkey prompts anchored to bottom corners.
  - UI and cutscene screens centered within the wider canvas with decorative framing bars spanning the full display width.
  - Added Screen Mode option in Options menu: `SCREEN: < WIDESCREEN / ORIGINAL >`, default Widescreen.
  - Tweak defaults to OFF in tests to guarantee 100% bit-for-bit trace and frame verification.

## [1.2.0] - Upgrades & Fixes: Task 2 (60 FPS Sub-Frame Interpolation)
### Added
- Implemented smooth sub-frame interpolation (`EngineTweaks.Smooth60Fps`) for camera and actors:
  - Game logic ticks remain strictly at original 70 ms step (~14.28 Hz) with 100% deterministic physics, combat, and RNG order.
  - Sub-frame alpha calculation $\alpha = \text{accumulator} / \text{SimStep}$ applied at 60 FPS presentation rate.
  - Interpolated coordinates applied to camera (`ac`, `ad`), player actor, enemy actors, projectiles, and moving platforms.
  - Animation frame indices (`var_int_a`, `x`, sub-rects) stay strictly locked to logic ticks.
  - Teleport and spawn protection: positions jump/snap instantly when displacement exceeds threshold (>64 px) or when spawned.
  - Switchable via Options menu: `FRAME RATE: < SMOOTH (60) / ORIGINAL (14) >`, default Smooth (60).
  - Tweak defaults to OFF in `PR.Core` and automated tests to guarantee 100% bit-for-bit trace and frame verification.

## [1.1.0] - Upgrades & Fixes: Task 1 (Sound Engine & MCI Playback Fix)
### Fixed
- Fixed audio playback failure on Windows: MCI `mciSendString` command failed with error 259 because `repeat` is not recognized by the Windows MIDI sequencer driver. Replaced with clean `play <alias> from 0` and polling in `MidiPlayer.Update()`.
- Implemented in-engine Living Mobile audio pack extractor in `SoundEngine.Load(dis)` inside `App.cs:m()`, loading MIDI blobs directly from memory (`/l`) at runtime from user JAR without requiring extracted files on disk.
- Connected audio trigger hooks in `PR.Core`:
  - Hooked `SoundEngine.Play(1)` at tick 2 in `Game.boolean_b()` (matching `f.java:908`).
  - Hooked `SoundEngine.Play(2)` on stage victory and `SoundEngine.Play(3)` on defeat in `Game.a()` (matching `f.java:2038-2043`).
  - Hooked `SoundEngine.Stop()` in `Game.e()`.
- Initialized Raylib audio device via `Raylib.InitAudioDevice()`, ensured volume scaling is active, and verified sound defaults to ON.
- Added `--logs` audio mode logging every sound event, byte size, MIME type, and stop event.
- Added Audio Test option in Options menu and F3 debug key to cycle and test sound playback by ID.

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
