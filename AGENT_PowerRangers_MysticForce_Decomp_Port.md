# AGENT BRIEF: Decompile and PC-port "Power Rangers" (J2ME, Living Mobile)

You are an autonomous engineering agent. Your job is to **reverse-engineer the J2ME game in the supplied JAR and build a validated PC port**, following the methodology of the reference project `https://github.com/yanm1103/mikisworld` (decomp + literal translation + trace/frame validation against the original).

Inputs you will be given:
- This file.
- `Power_Rangers_240_nok.jar` (the user's own copy of the game).

Work autonomously, phase by phase. Do not stop to ask questions unless a **STOP CONDITION** (section 12) is hit. Record every decision in the docs.

---

## 0. Ground rules (read first)

1. **Do not distribute the game.** The JAR and anything extracted from it (images, level data, text, audio) must be listed in `.gitignore`. The repo contains only new code, tools and documentation. The port reads resources **straight from the user-supplied JAR at runtime**.
2. **Do not commit decompiled output** (`decompiled/`, `extracted/`). It is derived from copyrighted code. Keep it local and gitignored. Your hand-written C# port and docs are the deliverable.
3. **Literal first, clean later.** Translate logic exactly (integer math, order of operations, quirks, even bugs). Only after validation passes may optional "engine tweaks" be added, and they must be switchable off so tests stay identical.
4. **Evidence over assumption.** Every claim in the analysis doc is tagged `[verified]` (checked by bytecode/trace/pixels) or `[inferred]`.
5. **Commit small and often**, with a progress table updated on each commit (see section 10).
6. Never fabricate resource formats. If a format is not yet understood, say so and write an independent parser to prove it.

---

## 1. What is already known (from a first-pass analysis)

Treat as `[inferred]` until you re-verify.

**Manifest:** `MIDlet-Name: Power Rangers sefan.ru`, vendor Living Mobile, version 2.0.0, MIDP-1.0, CLDC-1.0, MIDlet class `GMIDlet`, icon `i.png`. The JAR is 246,611 bytes and has 8 classes plus about 107 resource files.

**Classes** (obfuscated, default package, decompiled with CFR 0.152):

| Class | Lines | Role (first pass) |
|---|---|---|
| `GMIDlet` | ~650 | MIDlet entry; static image/sprite-pack helpers; creates `g`, sets full screen |
| `g` | ~2100 | `extends Canvas implements Runnable`: main thread, menus, text, fonts, language, RecordStore saves, pack loading (`lm`, `lp__*`, `lpok`, `font`, `text__*`, `lang`, `l`, `fr`, `spr`, `subtitle__*`) |
| `f` | ~2700 | In-level gameplay; loads `l<N>`, `b<N>`, `bg<N>`; `static Random`; **70 ms frame target** (`start + 70 - now`) |
| `a` | ~770 | Sprite/animation sets; loads `s<N>` |
| `b`, `c` | ~230, ~560 | Not yet analysed |
| `d` | ~520 | `implements Runnable`, own thread, 100 ms sleep (loading screen / intro?) |
| `e` | ~190 | Audio: `javax.microedition.media`, MIME table `midi, x-wav, mpeg, basic, x-tone-seq, x-mid, x-smaf, amr, amr-wb`; plays raw byte arrays via `Manager.createPlayer` |

**CFR trouble spots in `g`:** `q()`, `h()`, `i()`, `k()` and one `a(DataOutputStream, ByteArrayOutputStream, boolean)` had "loose catch block" or "unable to fully structure" warnings. Verify these from **bytecode** (`javap -c -p`).

**Resource families:**

| Files | Format (first pass) |
|---|---|
| `bg0..bg4`, `lm`, `lp__{de,en,fr,it}`, `lpok`, `subtitle__{de,en,fr,it}`, `s0..s64` | Length-prefixed PNG(s). 2-byte length then PNG (`89 50 4E 47`). Some `s*` files have an extra leading header byte, matching `GMIDlet.a(DataInputStream, Image[], short[], …)` (reads `byte flags`, `short len`, bytes, optionally `short count`) |
| `l0..l12` | Per-level binary data (small header + tile/entity data) |
| `b0..b12` | Tile/block definition tables; headers start `04 00 10 10 ...` (probably 16x16 tiles) |
| `l` (56 KB) | Big pack with offset tables (header `0a 00 05 00 59 ...`); may contain audio blobs |
| `spr` (39 KB) | Sprite pack (header `00 28 00 01 ...`) |
| `fr` (2.8 KB) | Frame/rect tables |
| `font` | Short header + PNG glyph sheet |
| `text__{de,en,fr,it}.txt` | u16 count, then Java `writeUTF` strings (UI text already readable) |
| `lang` | `00 04` then `00 02 de`, `en`, `fr`, `it` |

**Open question:** where the audio data lives (probably inside `l`) and what each blob's MIME type is. Resolve in Phase 3.

---

## 2. Target stack and repo layout

Default (mirror the reference project; change only with a written reason):
- **C# / .NET 8**, rendering and audio frontend with **Raylib-cs**.
- **Core library has no window/audio dependency** (so tests can run headless).
- Original-side harness in **Java (JDK 11+)**.
- Tools in **Python 3** (independent parsers and test runners).

```
docs/
  PowerRangers-analysis.md        class table, state machine, field map, formats
  FORMATS.md                      byte-level spec of every resource format
  VALIDATION.md                   how traces/frames are recorded and compared
  MODDING.md                      (optional, late)
port/
  PR.Core/                        logic + software renderer + format parsers (no I/O deps)
  PR.Game/                        Raylib window, input, audio, JAR resource reader
  PR.TraceTest/                   trace/frame replay + unit tests
tools/
  harness/                        Java: MIDP stubs + headless runner + recorders
  parse_level.py  parse_pack.py  parse_sprites.py  decode_text.py
  classmatch.py                   (only if multiple JAR variants are supplied)
  gen_inputs.py                   deterministic input scripts
  record_traces.sh  record_frames.sh  run_tests.sh
JAR/                              user's JAR(s); gitignored
extracted/  decompiled/  traces/  frames/   all gitignored
README.md  CHANGELOG.md  todo.md  .gitignore
```

`.gitignore` must include: `JAR/`, `extracted/`, `decompiled/`, `traces/`, `frames/`, `sons/`, `tools/cfr.jar`, `tools/dotnet/`, `dist/`, `*.jar`.

---

## 3. PHASE 1: Environment and extraction

**Goal:** reproducible local setup.

Steps:
1. Check tools: `java -version` (need 11+), `python3`, `dotnet --version` (8.x). Install what is missing (portable .NET: `dotnet-install` with `-Channel 8.0 -InstallDir tools/dotnet`).
2. Place the JAR at `JAR/Power_Rangers_240_nok.jar`.
3. Extract: `mkdir -p extracted && cd extracted && unzip -o ../JAR/Power_Rangers_240_nok.jar`.
4. Get the decompiler (CFR 0.152):
   `https://github.com/leibnitz27/cfr/releases/download/0.152/cfr-0.152.jar` → `tools/cfr.jar`
   (fallback: `repo1.maven.org/maven2/org/benf/cfr/0.152/`).
5. Decompile:
   `java -jar tools/cfr.jar JAR/Power_Rangers_240_nok.jar --outputdir decompiled --renamedupmembers true --renameillegalidents true`
6. Disassemble all classes for ground truth:
   `javap -c -p -constants -cp extracted <class>` → `decompiled/bytecode/<class>.txt`
7. Initialise git, write `.gitignore`, create `README.md` skeleton.

**Deliverable:** `docs/PowerRangers-analysis.md` with the class table (section 1) re-verified, and a log of CFR warnings.
**Done when:** every class has a one-paragraph role description and a list of resource files it reads.

---

## 4. PHASE 2: Read and annotate the code

**Goal:** understand the whole game before porting.

Procedure, in this order: `GMIDlet` → `g` → `a` → `e` → `d` → `b`/`c` → `f`.

For each class:
1. Build a **field map** (obfuscated name → meaning, type, units). Maintain it in `docs/PowerRangers-analysis.md` as a table.
2. Build a **method map** (obfuscated signature → meaning). Note duplicates (CFR flags "duplicate member names": distinguish by descriptor in bytecode).
3. Resolve every CFR-flagged method from bytecode, not from the decompiler.
4. Record: loop timing (`f`: 70 ms; `g`/`d`: own timing), key handling (`keyPressed`/`keyReleased` and the key-code mapping), pause/hide handling, `Random` use (seed source, call order), `RecordStore` layout (record ids, byte layout for saves/high scores).
5. Draw the **state machine** of `g` (loading → language select → title → menu → story/survival/tutorial → level → cutscene → game over, etc.) as a Mermaid diagram.

Game systems to document inside `f` (names from the UI text: Story mode, Survival, Tutorial, Locked, High score): player (Ranger) movement and physics, attacks and power-ups, enemies and AI, bosses, projectiles, collision (tile grid + hitboxes), camera/scrolling, parallax backgrounds (`bg*`), HUD, scoring, lives/health, level progression and unlock flags.

**Deliverable:** analysis doc covering 100% of classes, with `[verified]`/`[inferred]` tags.
**Done when:** you can describe a full game tick (input → update → collide → draw) without gaps.

---

## 5. PHASE 3: Resource formats (independent parsers)

**Goal:** prove each file format with a parser written **independently of the decompiled loader**, then cross-check the two.

For each family, write a Python parser under `tools/` that dumps human-readable output (and PNGs where applicable), and a byte-level spec in `docs/FORMATS.md`.

Order of attack:
1. `text__*.txt`, `lang` (trivial; do first as a warm-up).
2. Length-prefixed PNG files: `lm`, `lp__*`, `lpok`, `subtitle__*`, `bg*`, `font`, `i.png`. Export every image to `extracted_png/` (gitignored).
3. `s0..s64` sprite sets (flag byte semantics; palette-swap/variant copies are suggested by the `(by & 2)`, `n2`, bitmask logic in `GMIDlet`: decode this precisely).
4. `b0..b12` (tile/block tables; confirm 16x16).
5. `l0..l12` (level layouts: dimensions, tile layers, entity/spawn lists, triggers).
6. `l`, `spr`, `fr` (offset tables; frame/rect definitions; animation sequences).
7. **Audio:** find where `e` gets its byte arrays and the one-byte MIME index trailer (`array[length-1]` indexes the MIME table). Extract every blob, identify its real format (MIDI `MThd`, AMR `#!AMR`, WAV `RIFF`, tone sequence, etc.), and export to `sons/` (gitignored).

Validation of parsers: for each file, the parser must consume the file **exactly** (no leftover bytes, no overrun) and match what the Java loader reads. Add a Python test that runs over every file in the JAR.

**Done when:** `tools/parse_*.py` run clean over all 107 resources and `docs/FORMATS.md` documents each format.

---

## 6. PHASE 4: Headless harness for the original

**Goal:** run the original bytecode deterministically and record ground truth.

Build `tools/harness/` (Java, no real MIDP runtime needed):
1. **MIDP stubs** for what the game uses: `MIDlet`, `Display`, `Displayable`, `Canvas` (with `paint`, key events, `getWidth/getHeight`, `setFullScreenMode`), `Graphics` (real implementation on `BufferedImage`: clip, translate, `drawImage`, `drawRegion` with transforms, `fillRect`, `drawLine`, `setColor`, string/char drawing if used), `Image` (`createImage` from bytes/size, `getGraphics`, `getWidth/Height`, mutable/immutable), `RecordStore` (in-memory), `javax.microedition.media` (`Manager`, `Player`, `PlayerListener`: no-op but **record calls**), vibration if used.
2. **Determinism:**
   - Replace `System.currentTimeMillis()` with a controlled clock. Prefer bytecode rewriting or running the game's update/paint methods directly with a **fixed 70 ms step** (the `f` loop target).
   - Control `java.util.Random`: supply a fixed seed by patching `f.a = new Random()` through reflection after class init, or subclass-replace via bytecode instrumentation. Record the seed in each trace.
   - Drive the game **tick by tick** from the harness instead of letting threads run free.
3. **Recorders:**
   - Trace: after every tick, dump the complete relevant state (positions, velocities, state ids, health, score, timers, entity lists, RNG-dependent values) as one text line.
   - Frames: dump the 176x208-class screen (use the real canvas size the game reports) as PNG or raw RGB at chosen ticks.
   - Audio events: log `(tick, sound id, loop count)`.
4. **Input scripts** from `tools/gen_inputs.py`: deterministic sequences of key presses (map to phone key codes the game's `keyPressed` expects), plus optional **teleports** (direct state writes via reflection) to reach late-game areas.

**Done when:** `tools/record_traces.sh` produces identical trace files on repeated runs (run twice and `diff`).

---

## 7. PHASE 5: Literal C# port of the logic

**Goal:** `PR.Core` reproduces the original traces exactly.

1. Create `GameBase`-style shared code only where the original really shares it. There is just one game here, so keep one `Game` class mirroring `f` plus an `App` class mirroring `g` (menus/state/text/saves).
2. Translate **method by method**, keeping the original control flow. Preserve:
   - integer overflow and truncation behaviour (`short`/`byte` casts, `>>` vs `>>>`, integer division of negatives),
   - the exact call order of `Random.nextInt` (port `java.util.Random`'s LCG exactly; do not use `System.Random`),
   - array bounds quirks (reproduce original crashes on purpose where the original crashes, and document them).
3. Resource access goes through an `IResourceProvider` interface. The Raylib frontend implements it by reading the JAR (zip) at runtime; tests implement it from `extracted/`.
4. Add `EngineTweaks` (all **off** in tests): none are required at this stage.

Validation loop (repeat until identical):
- `tools/run_traces.sh` replays inputs in C#, diffs against the original traces, and reports the **first diverging tick** and field.
- Fix divergence by re-reading the bytecode for the responsible method.
- Add **negative controls** (wrong seed, trace from another input script) and confirm the comparison fails.

**Done when:** all trace sets match line for line, including at least one full story playthrough segment per level (via teleports) and Survival mode.

---

## 8. PHASE 6: Software renderer and frame validation

**Goal:** pixel-identical frames.

1. Implement a `Canvas`-like software renderer in Core (indexed or ARGB buffer; clip; `drawRegion` with all transforms the game uses; text via the game's own bitmap `font`, not the phone font).
2. Parallax backgrounds (`bg*`), tilemaps (`b*`/`l*`), sprites (`spr`, `fr`, `s*`), HUD, wipes/transitions, cutscene screens (`lp__*`, `subtitle__*`).
3. Compare against harness frames pixel by pixel. If the original uses the device system font anywhere, exclude those regions and document it.
4. Frame sets per area: title, menu, each level at several ticks, boss fights, game over, high scores.

**Done when:** all recorded frame sets are pixel-identical (excluding documented system-font text).

---

## 9. PHASE 7: PC frontend, audio, saves

1. **Window/scaling:** Raylib window; integer scaling with optional filters; full screen toggle (F11).
2. **Input:** keyboard and gamepad → phone key events via an `InputRouter`; all keys remappable; persist to `%APPDATA%/<GameName>/bindings.json`. Keep numpad digits behaving like phone keys.
3. **Audio:** decode the formats found in Phase 3. For MIDI/AMR/WAV use well-known libraries or convert at load time; only one sound at a time if the original works that way. Respect loop counts. Validate against logged audio events.
4. **Saves:** emulate the original `RecordStore` layout in a JSON file; high scores identical to the original logic (validate by differential test against the original classes).
5. **Menus:** language selection (de/en/fr/it), sound on/off (+ volume slider), vibration → gamepad rumble, tutorial, high-score clearing.
6. **Crash handling:** show error on screen and write `crash.log`.
7. CLI flags: `--scale N`, `--no-sound`, `--logs`, `--lang xx`, `--jar path`.

**Done when:** a human can play the whole game from title to ending on keyboard and gamepad.

---

## 10. Progress tracking (every commit)

Keep this table in `README.md` and update it on **every commit**. Weights: decompilation 10, logic 30, rendering 20, window/input 10, menus/saves 15, audio 15.

| Part | Status | Notes |
|---|---|---|
| Decompilation and analysis | % | |
| Game logic | % | N/N traces identical |
| Rendering | % | N frames pixel-identical |
| Window, scaling, input | % | |
| Menus, hi-scores, saving | % | |
| Audio | % | |
| **Estimated total** | % | |

Also maintain `CHANGELOG.md` and a `todo.md` of known gaps. `tools/run_tests.sh` must run everything and end with `ALL VALIDATIONS PASSED`.

---

## 11. Optional later phases

- **Extended view** (draw more of the level than the original screen; logic untouched).
- **Debug menu** (F3): invincibility, infinite lives, hitbox wireframe, skip level, unlock modes.
- **Mods:** folder overrides for images/sounds with hot reload (F5), documented in `docs/MODDING.md`.
- **Multiple JAR variants** (other resolutions/handsets): use `tools/classmatch.py` (match obfuscated classes by bytecode) to port differences instead of redoing the work.

Do these only after Phases 1-7 pass.

---

## 12. STOP CONDITIONS (ask the user)

Stop and report if:
1. The JAR contains classes or resources not described here and they change the architecture (e.g. native/other-platform code).
2. A required tool cannot be installed or network access is blocked and no fallback exists.
3. You would need to commit game assets or decompiled code to make progress.
4. Traces cannot be made deterministic after reasonable effort (explain what blocks it: timers, threads, uncontrolled randomness).
5. Two reads of the bytecode disagree and you cannot decide which is right.

Otherwise keep going. Prefer partial, validated progress over broad, unvalidated code.

---

## 13. Final deliverables checklist

- [ ] `docs/PowerRangers-analysis.md` (all classes, field/method maps, state machine)
- [ ] `docs/FORMATS.md` (every resource format) plus parsers in `tools/`
- [ ] `tools/harness/` and recorded traces/frames (local, gitignored)
- [ ] `PR.Core` with traces identical to the original
- [ ] Pixel-identical rendering
- [ ] `PR.Game` playable end to end (input, audio, saves, languages)
- [ ] `tools/run_tests.sh` ending in `ALL VALIDATIONS PASSED`
- [ ] `README.md` (player part and technical part, reference-project style), `CHANGELOG.md`, `todo.md`
- [ ] Repo contains **no** game files and **no** decompiled code
