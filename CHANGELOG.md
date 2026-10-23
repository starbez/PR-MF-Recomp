# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - Phase 3 Complete
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
