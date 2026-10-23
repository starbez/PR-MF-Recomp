# Power Rangers: Mystic Force (J2ME) - PC Source Port & Preservation

A clean-room reverse engineering and PC source port of the J2ME game *Power Rangers: Mystic Force* (developed by Living Mobile, 2006, 240x320 Nokia MIDP-1.0/CLDC-1.0 build).

Based on decompilation, bytecode analysis, and literal logic translation validated against execution traces from the original bytecode.

---

## Progress

| Part | Status | Notes |
|---|---|---|
| Decompilation and analysis | 50% | Phase 1 complete (CFR decompilation, bytecode disassembly, Section 1 claims verified). Phase 2 analysis underway. |
| Game logic | 0% | 0/0 traces identical |
| Rendering | 0% | 0 frames pixel-identical |
| Window, scaling, input | 0% | Not started |
| Menus, hi-scores, saving | 0% | Not started |
| Audio | 0% | Audio format mapped inside `l` pack |
| **Estimated total** | 5% | Environment setup & initial decompilation |

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
