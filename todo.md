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

## Phase 4: Headless Validation Harness [IN PROGRESS]
- [ ] Build MIDP stubs in Java (`tools/harness/`).
- [ ] Deterministic clock (70ms tick) & fixed RNG seeding.
- [ ] State trace recorder & frame dumper.

## Phase 5: C# Core Logic Port [UPCOMING]
- [ ] Implement `PR.Core` replicating exact logic.
- [ ] Match traces 100% against Java harness.

## Phase 6: Software Renderer & Frame Validation [UPCOMING]
- [ ] Tilemap, sprite, background, and HUD software rasterizer.
- [ ] Pixel-identical frame comparison.

## Phase 7: Desktop Frontend & Release [UPCOMING]
- [ ] Raylib frontend (`PR.Game`).
- [ ] Audio playback, gamepad/keyboard mapping, RMS save emulation.
