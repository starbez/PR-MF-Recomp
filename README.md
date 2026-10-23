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
| Window, scaling, input | 0% | Phase 7 |
| Menus, hi-scores, saving | 25% | Formats and RMS byte layout fully mapped, serialization ported |
| Audio | 50% | Both MIDI tracks extracted from `l` pack to `sons/` and verified |
| **Estimated total** | 75% | Phases 1-6 complete. Logic and rendering verified bit-identical to original J2ME bytecode. |

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
