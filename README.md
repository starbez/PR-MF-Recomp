# Power Rangers: Mystic Force (J2ME) - PC Source Port & Preservation

A clean-room reverse engineering and PC source port of the J2ME game *Power Rangers: Mystic Force* (developed by Living Mobile, 2006, 240x320 Nokia MIDP-1.0/CLDC-1.0 build).

Based on decompilation, bytecode analysis, and literal logic translation validated against execution traces from the original bytecode.

---

## Progress

| Part | Status | Notes |
|---|---|---|
| Decompilation and analysis | 100% | Phase 1 & 2 complete. All classes analyzed, bytecode audited, state machine documented. |
| Game logic | 100% | Phase 5 complete: Literal C# port (`PR.Core`) verified 100% bit-identical line-for-line across all 4 trace sets (608 ticks). |
| Rendering | 100% | Phase 6 complete: Software renderer verified 100% pixel-identical (76,800/76,800 pixels) across all 7 reference frames. |
| Window, scaling, input | 100% | Phase 7 complete: Raylib desktop window, integer scaling (1x..5x), fullscreen (F11), remappable Keyboard & Gamepad `InputRouter`. |
| Menus, hi-scores, saving | 100% | Phase 7 complete: Authentic J2ME Living Mobile remake with original frames & art, `%APPDATA%/PowerRangers/save.json`, High Scores hall of fame, character & level select, options. |
| Audio | 100% | Phase 7 & Task 1 complete: In-memory `/l` MIDI pack extraction, MCI sequencer looping fix, Raylib audio device init, audio logging & debug menu. |
| **Estimated total** | 100% | All 7 phases complete. Verified bit-identical logic, pixel-identical rendering, fully playable desktop port. |
| Task 1: Fix missing sound | 100% | DIAGNOSED & FIXED: MCI repeat parameter bug resolved, in-memory loader, verified 0 audio diffs with harness. |
| Task 2: 60 FPS smooth | 100% | Original 70ms logic preserved, sub-frame interpolation for camera/actors, snap on teleports, switchable in menu. |
| Task 3: Widescreen | 100% | DIAGNOSED & FIXED: Full 568x320 viewport buffer allocation, toroidal quadrant drawing, camera bounds clamping, 4:3 logic activation isolation, seamless boss arena edge extension. |
| Task 4: Better menus | 100% | Authentic J2ME remake: Living Mobile art assets (wallpaper tile, metallic borders, shield header, watermark, arrow sprites, world map), modern settings layer (volume, language, FPS, widescreen, scaling, rebinding), mouse/gamepad navigation, Esc pause menu. |

---

## How to Run the Game

### Quick Launch (Windows)
Double-click `run.bat` or run:
```cmd
run.bat
```

To pass options:
```cmd
run.bat --scale 4 --lang en
run.bat --jar JAR\Power_Rangers_240_nok.jar
```

### CLI Launch (.NET CLI)
```bash
# Run with default 3x scaling (720x960) and sound:
dotnet run --project port/PR.Game/PR.Game.csproj

# Run with custom options:
dotnet run --project port/PR.Game/PR.Game.csproj -- --scale 4 --lang en

# Run directly from user-supplied JAR:
dotnet run --project port/PR.Game/PR.Game.csproj -- --jar JAR/Power_Rangers_240_nok.jar
```

### Controls
- **Move:** Arrow Keys / `W`, `A`, `S`, `D` / Numpad `2`, `4`, `6`, `8` / Gamepad D-pad & Left Stick
- **Attack / Jump:** `Space` / `Enter` / Numpad `5` / Gamepad `A`
- **Magic Spell:** `Left Ctrl` / `0` / Numpad `0` / Gamepad `B`
- **Pause / Menu:** `Escape` / `P` / Gamepad `Start`
- **Fullscreen Toggle:** `F11`

---

## How to Run Validation Tests

To run the complete automated differential verification suite (traces + frames):

```bash
# On Windows (PowerShell):
powershell -File tools/run_tests.ps1

# On Linux/macOS/Git Bash:
bash tools/run_tests.sh
```

---

## Project Structure

```
docs/
  PowerRangers-analysis.md        class table, state machine, field maps, formats
  FORMATS.md                      byte-level spec of every resource format (Phase 3)
  VALIDATION.md                   trace & frame recording methodology (Phase 4)
port/
  PR.Core/                        core game logic & software renderer (no I/O dependencies)
  PR.Game/                        Raylib-cs desktop frontend (window, input, audio, JAR reader)
  PR.TraceTest/                   trace replay & differential validation tests
tools/
  cfr.jar                         CFR 0.152 decompiler (local, gitignored)
  dotnet/                         portable .NET 8 SDK (local, gitignored)
  harness/                        headless Java validation harness (Phase 4)
  inspect_resources.py            resource analyzer tool
JAR/                              user-supplied game JAR (gitignored)
extracted/                        unpacked JAR contents (gitignored)
decompiled/                       CFR output and javap bytecode disassembly (gitignored)
```

---

## Legal & Asset Policy

No copyrighted game assets or decompiled source code are committed to this repository.
The PC port reads all assets directly from the original user-supplied JAR at runtime.
