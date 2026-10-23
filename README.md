# Power Rangers: Mystic Force (J2ME) - PC Source Port & Preservation

A clean-room reverse engineering and enhanced PC source port of the classic J2ME game *Power Rangers: Mystic Force* (developed by Living Mobile, 2006, 240x320 Nokia MIDP-2.0/CLDC-1.0 build).

Based on decompilation, bytecode analysis, and literal logic translation validated line-for-line against execution traces and frame buffers from the original Java bytecode.

---

## Features

- **Bit-Identical Simulation:** Full 70 ms fixed-step logic (`boolean_b`) verified 100% bit-identical line-for-line against original Java bytecode traces across all levels.
- **Pixel-Identical Software Renderer:** High-fidelity rasterization verified pixel-for-pixel against original Java reference frames.
- **16:9 Widescreen Support:** Wide virtual viewport buffer allocation with toroidal quadrant rendering, camera bounds clamping, and seamless edge extensions.
- **Arbitrary Refresh Rates:** Switchable between authentic 14 FPS, smooth 60 FPS, 120 FPS, 144 FPS, 240 FPS, or uncapped/VSync with sub-frame actor and camera interpolation.
- **Full Controller Support:** Native multi-gamepad support (Xbox, PlayStation, generic DirectInput/XInput) with context-aware controls (Jump with A, Attack with X, Magic with B/Y/Triggers, Move with D-Pad/Left Stick, Pause with Start/Back).
- **From-Scratch Mission Selector & Stage Dossier:** Clean, responsive 3x4 stage matrix + Survival Arena button with stage briefings, tactical objectives, and unlock tracking.
- **Continuous Story Progression:** Automatic advancement to subsequent levels upon stage completion with accumulated scores intact.
- **Debug & Cheats Submenu:** Accessible from Settings: toggle God Mode (Infinite HP), Infinite Magic, and one-click level unlock/reset.
- **Asynchronous MIDI Audio:** Zero-latency background worker thread architecture for seamless, continuous BGM looping via Windows MCI without main thread stutter.
- **Hall of Fame & Persistent Saves:** Saves high scores, unlock progression, and user preferences to `%APPDATA%/PowerRangers/save.json`.

---

## Controls

| Action | Keyboard | Numpad | Gamepad (Xbox / PS) |
|---|---|---|---|
| **Move / Climb / Crouch** | Arrow Keys / `W`, `A`, `S`, `D` | `2`, `4`, `6`, `8` | D-Pad / Left Stick |
| **Attack** | `Enter` / `Space` | `5` | `X` (Face Left / Square) |
| **Jump** | `Up` / `W` | `2` | `A` (Face Down / Cross) |
| **Magic Spell** | `Left Ctrl` / `0` | `0` | `B` / `Y` / `RB` / `RT` |
| **Menu Confirm / Select** | `Enter` / `Space` | `5` | `A` (Face Down) / `Start` |
| **Menu Cancel / Back** | `Escape` / `Backspace` | `0` | `B` (Face Right) |
| **Pause Game** | `Escape` / `P` | - | `Start` / `Back` |
| **Fullscreen Toggle** | `F11` | - | - |

---

## How to Run

### Quick Launch (Windows)
Double-click `run.bat` or run:
```cmd
run.bat
```

To specify custom startup options:
```cmd
run.bat --scale 4 --lang en
run.bat --jar JAR\Power_Rangers_240_nok.jar
```

### CLI Launch (.NET CLI)
```bash
# Run with default 3x integer scaling:
dotnet run --project port/PR.Game/PR.Game.csproj

# Run with custom options:
dotnet run --project port/PR.Game/PR.Game.csproj -- --scale 4 --lang en

# Run directly from user-supplied JAR:
dotnet run --project port/PR.Game/PR.Game.csproj -- --jar JAR/Power_Rangers_240_nok.jar
```

---

## Validation & Verification Suite

To run the automated differential verification suite (traces + frame comparison):

```bash
# On Windows (PowerShell):
powershell -ExecutionPolicy Bypass -File tools/run_tests.ps1

# On Linux / macOS / Git Bash:
bash tools/run_tests.sh
```

All 608 simulation ticks and 7 reference frames are verified 100% bit-identical and pixel-identical line-for-line against ground-truth data from the original Java MIDP bytecode.

---

## Project Structure

```
docs/
  PowerRangers-analysis.md        Class table, state machine, field maps, formats
  FORMATS.md                      Byte-level spec of resource formats
  VALIDATION.md                   Trace & frame recording methodology
  STATUS.md                       Cumulative implementation logs & milestone status
port/
  PR.Core/                        Core game logic & software renderer (.NET 8)
  PR.Game/                        Raylib desktop frontend (window, input, audio, UI)
  PR.TraceTest/                   Trace replay & differential validation tests
tools/
  cfr.jar                         CFR 0.152 decompiler (local, gitignored)
  dotnet/                         Portable .NET 8 SDK (local, gitignored)
  harness/                        Headless Java validation harness
  scripts/                        Deterministic input schedules for test scenarios
JAR/                              User-supplied game JAR (gitignored)
extracted/                        Extracted JAR resource package (gitignored)
decompiled/                       Disassembly and bytecode reference (gitignored)
```

---

## Legal & Asset Policy

No copyrighted game assets, ROMs, or decompiled source code are committed to this repository. The port reads game assets directly from the original user-supplied JAR at runtime.
