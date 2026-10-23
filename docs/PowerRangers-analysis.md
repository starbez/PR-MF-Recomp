# Power Rangers: Mystic Force (J2ME) - Codebase Analysis

## 1. Verification of Initial Claims (Section 1 Re-Verification)

All claims from Section 1 of the brief have been re-verified against the unpacked JAR, CFR 0.152 decompilation, and `javap` bytecode disassembly:

- **Manifest Attributes:** `[verified]`
  - `MIDlet-Name: Power Rangers sefan.ru`
  - `MIDlet-Vendor: Living Mobile`
  - `MIDlet-Version: 2.0.0`
  - `MicroEdition-Profile: MIDP-1.0`
  - `MicroEdition-Configuration: CLDC-1.0`
  - `MIDlet-1: Power Rangers sefan.ru, i.png, GMIDlet`
  - `MIDlet-Icon: i.png`
  - `MIDlet-Data-Size: 2000`
- **JAR Size:** `[verified]`
  - Exact file length: `246,611 bytes`
- **File Counts:** `[verified]`
  - 8 Java class files (`GMIDlet.class`, `a.class`, `b.class`, `c.class`, `d.class`, `e.class`, `f.class`, `g.class`)
  - 106 game resources in archive root + 1 `META-INF/MANIFEST.MF` = 107 total entries in JAR
- **Resource Families:** `[verified]`
  - `bg0..bg4` (5 files): length-prefixed PNG backgrounds
  - `b0..b12` (13 files): tile/block definition tables (16x16 tiles)
  - `l0..l12` (13 files): per-level binary tile/entity layouts
  - `s0..s64` (55 files): sprite definitions and length-prefixed PNG frames (indices 28, 29, 37-40, 42-45 omitted in this build)
  - `lm`, `lp__{de,en,fr,it}`, `lpok`, `subtitle__{de,en,fr,it}`: length-prefixed PNGs (2-byte length + PNG header `89 50 4E 47`)
  - `l` (55,950 bytes): master binary data pack containing level metadata, configuration tables, cutscene images, and audio payloads
  - `spr` (39,396 bytes): sprite sheet/offset pack
  - `fr` (2,809 bytes): frame and rect definitions
  - `font` (2,066 bytes): font header + PNG font glyph sheet
  - `text__{de,en,fr,it}.txt` (4 files): u16 string count followed by `writeUTF` encoded strings
  - `lang` (18 bytes): language list (`00 04` count, then `de`, `en`, `fr`, `it`)
  - `i.png` (900 bytes): standard PNG game icon

---

## 2. Class Overview Table

| Class | Lines | Role `[verified]` | Resource Files Read `[verified]` |
|---|---|---|---|
| `GMIDlet` | 643 | Application entry point extending `MIDlet`. Initializes display canvas `g`, sets full screen, initializes static bitmap font systems, provides utility decoders for length-prefixed PNGs, sprite loading, string arrays, and drawing primitives. | `i.png` (via manifest), `/font` (delegated from `g`) |
| `g` | 2123 | Core application framework extending `Canvas` and implementing `Runnable`. Manages global application state machine, top-level main thread loop, language selection, menus/forms, RecordStore (RMS) save/load, high score table, cutscene playback, and master asset loader. | `/lm`, `/lp__*`, `/lpok`, `/font`, `/text__*.txt`, `/lang`, `/l`, `/fr`, `/spr`, `/subtitle__*` |
| `f` | 2674 | In-game play engine. Executes the main action game loop targeted at 70 ms (`start + 70 - now`). Manages active Ranger state, player physics, jump/attack combo mechanics, enemy spawns and AI, boss behaviors, projectile tracking, collision resolution, camera scrolling, parallax background rendering, and HUD overlay. | `/l<N>` (l0..l12), `/b<N>` (b0..b12), `/bg<N>` (bg0..bg4) |
| `a` | 763 | Sprite and animation entity representation. Represents actors, player animations, enemy animations, particles, and interactive objects. Encapsulates frame sequences, current frame timing, hitbox bounds, transformation/flip flags, and position coordinates. | `/s<N>` (s0..s64) |
| `b` | 227 | UI Widget and Menu Item component. Represents individual menu elements including selectable text items, multi-choice selectors/spinners, image items, separators, and custom sub-menus. Handles layout measurements and item rendering. | None directly (uses resources preloaded in `g` and `GMIDlet`) |
| `c` | 559 | Tilemap engine, collision detection grid, and camera viewport manager. Parses block definition tables (`b*`) and level grids (`l*`), manages screen tile scrolling buffers, computes tile bounds, and resolves collision tests between actors (`a`) and the world. | None directly (reads binary stream from `f`) |
| `d` | 518 | Menu Screen / Form container implementing `Runnable`. Manages a list of `b` UI items, handles navigation cursor movement, scroll positioning, softkey display, and runs a dedicated 100 ms repaint thread for animated menu items and cursors. | None directly (operates on `b` items and strings preloaded in `g`) |
| `e` | 188 | Audio manager implementing `PlayerListener`. Wraps `javax.microedition.media.Manager` and `Player` interfaces. Maintains supported MIME types (`midi`, `x-wav`, `mpeg`, `basic`, `x-tone-seq`, `x-mid`, `x-smaf`, `amr`, `amr-wb`). Loads sound blobs directly from the `/l` data pack stream and controls volume and looping. | `/l` (reads audio blob segment via stream passed from `g`) |

---

## 3. CFR Decompiler Warnings & Bytecode Audit Log `[verified]`

Decompilation with CFR 0.152 generated warnings in class `g`:

| Class | Method | CFR Warning | Audit Requirement |
|---|---|---|---|
| `g` | `q()` | `Loose catch block` | Reconstruct exact exception handling and stream cleanup in save data / RMS read. |
| `g` | `h()` | `Loose catch block` | Verify high score / settings serialization logic and catch block behavior. |
| `g` | `i()` | `Loose catch block` | Verify RMS initialization and error fallback behavior. |
| `g` | `k()` | `Unable to fully structure code` | Decompile control flow graph directly from `javap` bytecode for state/menu transition logic. |
| `g` | `a(DataOutputStream, ByteArrayOutputStream, boolean)` | `Loose catch block` | Reconstruct exact binary save layout and exception recovery flow. |

All 5 flagged methods will be audited directly against `decompiled/bytecode/g.txt` in Phase 2.
