# Power Rangers: Mystic Force (J2ME) - Binary Resource Formats Specification

All numbers are big-endian (network byte order), matching Java's `DataInputStream` / `DataOutputStream`.

---

## 1. Text & Localization (`lang`, `text__*.txt`)

### 1.1 `lang` (18 bytes)
A simple table of supported language codes:
```
[0..1]   u16 count (4)
For each language:
  [..]   u16 string_len (2)
  [..]   utf8 string ("de", "en", "fr", "it")
```

### 1.2 `text__{de,en,fr,it}.txt`
Each language file contains 6 distinct string arrays (total 222 strings per file):
```
For array index 0..5:
  [..]   u16 string_count
  For each string:
    [..] u16 string_len
    [..] utf8 string bytes
```
- **Array 0 (46 strings):** Core UI menus and navigation labels (OK, Exit, Menu, Options, Story, etc.)
- **Array 1 (36 strings):** Character names, stage dialogue prompts, level titles
- **Array 2 (75 strings):** Cutscene narration subtitles
- **Array 3 (20 strings):** Tutorial prompts and interactive instructions
- **Array 4 (21 strings):** In-game help & control reference text
- **Array 5 (24 strings):** Credits, copyright, and version text

---

## 2. Length-Prefixed & Palette-Variant PNGs

### 2.1 Standard Length-Prefixed Images (`lp__*`, `lpok`, `i.png`)
- `i.png`: Raw PNG file without prefix (`89 50 4E 47 0D 0A 1A 0A...`).
- `lp__*`, `lpok`:
  ```
  [0..1]   u16 png_len
  [2..N]   raw PNG bytes (length = png_len)
  ```

### 2.2 Flag-Prefixed Images (`subtitle__*`, `lm`)
- `subtitle__*`:
  ```
  [0]      u8  flags (0)
  [1..2]   u16 png_len
  [3..N]   raw PNG bytes
  ```
- `lm`: Contains two consecutive flag-prefixed images (Living Mobile logo and intro banner).

### 2.3 Palette-Variant Images (`bg0..bg4`, `s0..s64`)
Employs Living Mobile's runtime PLTE patch mechanism for palette swapping:
```
[0]      u8  flags:
               bit 1 (& 2): has_palette_variants
[1..2]   u16 base_png_len
[3..N]   raw PNG bytes (Variant 0 base image)

If (flags & 2) != 0:
  [..]   u16 variant_count
  For variant v = 1 .. variant_count - 1:
    [..] u8  patch_count_byte
    If patch_count_byte == 0:
      [..] u16 patch_count
    Else:
      patch_count = patch_count_byte
    For patch p = 0 .. patch_count - 1:
      [..] u16 byte_offset_into_png
      [..] u8  replacement_byte_value
```

---

## 3. Font Asset (`font`, 2,066 bytes)

Defines proportional bitmap glyph sheets and metrics:
```
[0..1]   u16 font_count (4)
For each font image slice (0..font_count-1 step variant_count):
  [..]   Palette-Variant Image Stream (see 2.3)
[..]     utf8 primary_font_name
[..]     utf8 secondary_font_name
[..]     u16  metrics_len
[..]     metrics_len bytes
[..]     u16  dummy
[..]     u16  dummy
For each font index 0..font_count-1:
  [..]   utf8 glyph_charset
  [..]   u16  tag_count
  For each tag:
    [..] u8  tag_type (0..8)
    Tag 0:   u8 boolean
    Tag 1:   u16 len, len bytes
    Tag 2..8:u8 value
```

---

## 4. Tile Definition Tables (`b0..b12`)

Tilemaps for each level stage (all 16x16 pixel tiles):
```
[0]      u8 layer_count
For each layer:
  [0]    u8  bg_image_id
  [1]    u8  tile_size (16)
  [2]    u8  dummy
  [3..4] u16 map_width_tiles
  [5..6] u16 map_height_tiles
  [7]    u8  has_flags
  [8]    u8  parallax_scale_x (ratio / 10)
  [9]    u8  parallax_scale_y (ratio / 10)
  [10..11] i16 offset_x
  [12..13] i16 offset_y
  [..]   map_height * 2 bytes row offset table
  [..]   RLE-compressed tile indices buffer
  [..]   u8  tile_property_count
  [..]   Tile property mapping records
  [..]   u16 trigger_count
  For each trigger:
    [..] i32 trigger_rect_encoded
    [..] u8  trigger_action_type
```

---

## 5. Level Layouts (`l0..l12`)

Level layouts, entity placement, player spawn, and spline paths:
```
[0]      u8  b_file_index (which b<N> to load)
[1]      u8  bg_variant_index
[2]      u8  primary_collision_layer_index
[3..4]   u16 level_tags_count
For each tag:
  [..]   u8 tag_type, u8 tag_val

[..]     u16 decoration_actors_count
For each decoration actor:
  [..]   Actor definition record

[..]     u16 dummy
[..]     Player spawn definition record

[..]     u16 enemy_actors_count
For each enemy actor:
  [..]   Actor definition record (includes spawn coords, archetype, AI parameters)

[..]     u16 interactive_triggers_count
For each trigger:
  [..]   Trigger actor record

[..]     u16 moving_hazards_count
For each hazard:
  [..]   i16 path_coord, u8 active_flag, u8 speed, u16 dummy, actor record

[..]     u16 pickups_count
For each pickup:
  [..]   u8 param, u8 type, u16 dummy, actor record

[..]     u16 checkpoints_count
[..]     u16 foreground_items_count

[..]     u16 spline_count
For each spline path:
  [..]   u16 point_count
  [..]   point_count * 2 bytes (i16 coordinates)
```

---

## 6. Global Packs: Rects (`fr`), Sprites (`spr`), Master Pack (`l`)

### 6.1 Bounding Rects (`fr`, 2,809 bytes)
Hitboxes and physics collision boxes:
```
[0..1]   u16 total_rect_count (660)
[2..3]   u16 group_count (55)
For each group:
  [..]   u16 rect_count
  [..]   u8  group_flag
  For each rect:
    [..] i8 x, i8 y, i8 w, i8 h
```

### 6.2 Sprite Animation Definitions (`spr`, 39,396 bytes)
Contains animation frame sequences and sub-rect slices:
```
[0..1]   u16 sprite_count (40)
For each sprite 0..sprite_count-1:
  [..]   12 bytes header (2x u16, 1x u64)
  [..]   u16 frame_count
  For each frame:
    [..] u16 subrect_count
    [..] (5 + subrect_count * 4) bytes sub-frame data
  [..]   u16 sequence_count
  For each animation sequence:
    [..] u16 seq_len
    [..] u8  seq_type (if 4: 2 words/frame; else: 4 words/frame)
    [..] u8  param1, u8 param2
    [..] (words_per_frame * seq_len) i16 frame indices & timings
```

### 6.3 Master Pack (`l`, 55,950 bytes) & Audio Payloads
Contains level progression parameters, character constants, cutscene images, and all audio:
```
[0]      u8  level_count
[1..2]   u16 config_int_count
[..]     config_int_count * 4 bytes
[..]     Config byte tables (enemy stats, physics gravity curves, level unlock masks)
[..]     29 Embedded Image definitions (PNGs + palette variants)
[..]     u8  read_byte delimiter (0)

Audio Section (loaded by e.a):
[..]     u16 sound_count (2)
For each sound 1..sound_count:
  [..]   u8  delimiter
  [..]   u16 sound_blob_length
  [..]   sound_blob_length bytes raw audio payload
  [..]   u8  mime_table_index (0 = "midi")
  [..]   u16 mapping_records_count
  For each mapping:
    [..] u8 sound_event_trigger_id
  [..]   u16 loop_flag / control
```

#### Audio Extract Log:
- **`sons/sound_1.mid`**: 4,626 bytes, Standard MIDI format (`MThd` header), Title & Menu Theme.
- **`sons/sound_2.mid`**: 7,076 bytes, Standard MIDI format (`MThd` header), In-Game Combat Theme.
