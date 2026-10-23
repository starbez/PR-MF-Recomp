# Project Status Notes

## Status after Phase 3 (Resource Formats & Independent Parsers)

### 1. What is Done
- Authored independent Python parsers for every resource family under `tools/`:
  - `tools/decode_text.py`: `lang` and `text__{de,en,fr,it}.txt` (all 6 arrays, 222 strings per file).
  - `tools/parse_pngs.py`: Length-prefixed PNGs (`lm`, `lp__*`, `lpok`, `subtitle__*`, `bg*`, `font`, `i.png`).
  - `tools/parse_sprites.py`: `s0..s64` (all 55 sprite files, 65 variant PNGs).
  - `tools/parse_blocks.py`: `b0..b12` (13 block files, 16x16 tiles).
  - `tools/parse_level.py`: `l0..l12` (13 level layout files, entities, spawns, splines).
  - `tools/parse_pack.py`: `fr` (660 rects), `spr` (40 sprite animation definitions), and master pack `l` (29 images + audio).
- Extracted and verified both game audio tracks from `/l` into `sons/`:
  - `sons/sound_1.mid`: Title/Menu theme (4,626 bytes, Standard MIDI format).
  - `sons/sound_2.mid`: In-game Combat theme (7,076 bytes, Standard MIDI format).
- Ran `tools/test_parsers.py`: **ALL 107/107 files** in the JAR are consumed with 0 leftover bytes and 0 overruns.
- Authored `docs/FORMATS.md` with complete byte-level specifications.

### 2. What is Verified vs. Inferred
- **100% of Resource Formats: [verified]**
  - Confirmed via independent round-trip binary consumption matching decompiled loaders.
  - Palette swapping via runtime PLTE byte replacement is completely decoded.
  - Decompiler artifact in `f.a_a` (spurious duplicate `readByte()`) resolved via bytecode.
- **Audio Blobs: [verified]**
  - Audio files are confirmed to be Standard MIDI files (`MThd` magic).

### 3. What is Still Unknown
- None for resource formats or asset loading. All formats are 100% understood and validated.
- Ready for Phase 4: Headless Validation Harness in Java.
