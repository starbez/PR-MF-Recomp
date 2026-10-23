# Project Status Notes

## Status after Phase 4 (Headless Validation Harness)

### 1. What is Done
- Built clean-room Java MIDP 2.0 / MMAPI stubs in `tools/harness/`:
  - `MIDlet`, `Displayable`, `Display`, `Canvas`, `Font`, `Image`, `Graphics`, `RecordStore`, `Manager`, `Player`, `VolumeControl`, `Controllable`.
- Implemented `tools.HarnessClk` and `tools.HarnessClassLoader`:
  - Zero-sleep clock patch for instant execution (~10,000 ticks/sec).
  - Fixed-seed PRNG initialization for 100% deterministic runs.
- Authored input scripts in `tools/scripts/`: `idle_100.txt`, `movement_150.txt`, `combat_200.txt`, `survival_150.txt`.
- Built `tools/record_traces.sh` / `tools/record_traces.ps1`:
  - Successfully generated and double-run verified all 4 ground truth trace sets with 0 differences (`idle_level1.txt`, `movement_level1.txt`, `combat_level1.txt`, `survival_level1.txt`).
- Built `tools/record_frames.sh` / `tools/record_frames.ps1`:
  - Dumped reference ground-truth frames at ticks 0, 25, 50, 75, 100, 125, 149 into `frames/level1_movement/`.
- Authored `docs/VALIDATION.md` detailing harness architecture, determinism, and trace schema.

### 2. What is Verified vs. Inferred
- **Headless Bytecode Execution: [verified]**
  - Confirmed running original `extracted/*.class` files directly on OpenJDK 21 without phone emulation.
  - Repeated executions produce identical bit-for-bit hashes across all traces.
- **Reference Frames: [verified]**
  - Ground truth 240x320 PNG frames recorded and available for Phase 6 comparison.

### 3. What is Still Unknown
- None. Ground truth traces are established and ready for Phase 5 (Literal C# port).


## Status after Phase 5 (Literal C# Port of Logic & Trace Validation)

### 1. What is Done
- Implemented `port/PR.Core` (.NET 8):
  - `JavaRandom`: Exact 48-bit Java LCG algorithm (`(seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1)`).
  - `DataReader`, `DataWriter`: Big-endian binary readers/writers matching Java `DataInputStream` and `DataOutputStream`.
  - `IResourceProvider`, `FolderResourceProvider`, `ZipResourceProvider`: Clean abstraction for loading assets from folder or JAR.
  - `Actor`: Full actor model, sequence parsing, sub-rect slicing, hitbox mathematics, physics updates, combat states.
  - `TileMap`: 16x16 collision grid, palette decompression, collision solver (slopes, hazards, walls), parallax scrolling.
  - `App`: Resource pack loader for `/l`, `/fr`, `/spr`, character template factory (`a_a(int)`), text and save RMS layout.
  - `Game`: Complete 70 ms fixed-step simulation loop (`boolean_b`), camera tracking, combat resolution, enemy AI (`d(a)`, `d(int)`), script bytecode interpreter (`c(int)`), survival wave advancement.
- Implemented `port/PR.TraceTest` and `tools/run_traces.sh` / `tools/run_traces.ps1`:
  - Verified 100% bit-identical execution against original Java bytecode traces:
    - `idle_level1` (102 ticks) - **PASSED**
    - `movement_level1` (152 ticks) - **PASSED**
    - `combat_level1` (202 ticks) - **PASSED**
    - `survival_level1` (152 ticks) - **PASSED**
  - Verified negative control detects divergences when given mismatched seed / input.
- Decompilation bugs resolved:
  - Bytecode audit of `TileMap.cs`: palette header in `c.class` has loop count first, then array size.
  - Bytecode audit of `Game.cs`: opcode 54 branch condition inverted by CFR decompilation.
  - Bytecode audit of `Game.cs:f(Actor)`: recovered missing `var_int_f = 0` and `var_byte_c++` bytecode instructions.

### 2. What is Verified vs. Inferred
- **Simulation Determinism: [verified]**
  - All 608 ticks across 4 gameplay scenarios are bit-identical line-for-line between C# and Java bytecode.
- **Save State Format: [verified]**
  - Serialization in `Game.a(DataOutputStream)` and `Game.b(DataInputStream)` matches Java binary layout.

### 3. What is Still Unknown
- Software renderer exact pixel blending / clipping parity for Phase 6 (to be verified next against recorded reference frames).
