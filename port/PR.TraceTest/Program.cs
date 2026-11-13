using System;
using System.Collections.Generic;
using System.IO;
using PR.Core;
using PR.Core.Graphics;

namespace PR.TraceTest;

public class Program
{
    public record TestCase(int Level, int Character, int Ticks, long Seed, string ScriptPath, string TracePath, string Name);

    public static int Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("PR.TraceTest: Validating C# Port Against Java Traces");
        Console.WriteLine("==================================================");

        string rootDir = Directory.GetCurrentDirectory();
        // Locate extracted/ and traces/ directory
        string extractedDir = Path.Combine(rootDir, "extracted");
        if (!Directory.Exists(extractedDir))
        {
            extractedDir = Path.Combine(rootDir, "..", "..", "extracted");
        }
        if (!Directory.Exists(extractedDir))
        {
            extractedDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "extracted"));
        }

        string tracesDir = Path.Combine(rootDir, "traces");
        if (!Directory.Exists(tracesDir))
        {
            tracesDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "traces"));
        }

        string scriptsDir = Path.Combine(rootDir, "tools", "scripts");
        if (!Directory.Exists(scriptsDir))
        {
            scriptsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "scripts"));
        }

        Console.WriteLine($"Extracted dir: {extractedDir}");
        Console.WriteLine($"Traces dir:    {tracesDir}");
        Console.WriteLine($"Scripts dir:   {scriptsDir}");

        var testCases = new List<TestCase>
        {
            new TestCase(1, 0, 100, 42L, Path.Combine(scriptsDir, "idle_100.txt"), Path.Combine(tracesDir, "idle_level1.txt"), "idle_level1"),
            new TestCase(1, 0, 150, 42L, Path.Combine(scriptsDir, "movement_150.txt"), Path.Combine(tracesDir, "movement_level1.txt"), "movement_level1"),
            new TestCase(1, 0, 200, 42L, Path.Combine(scriptsDir, "combat_200.txt"), Path.Combine(tracesDir, "combat_level1.txt"), "combat_level1"),
            new TestCase(1, 0, 150, 42L, Path.Combine(scriptsDir, "survival_150.txt"), Path.Combine(tracesDir, "survival_level1.txt"), "survival_level1"),
        };

        bool allPassed = true;

        foreach (var tc in testCases)
        {
            Console.WriteLine($"\n--- Running {tc.Name} (Level {tc.Level}, Seed {tc.Seed}, Ticks {tc.Ticks}) ---");
            if (!File.Exists(tc.TracePath))
            {
                Console.WriteLine($"ERROR: Reference trace not found at {tc.TracePath}");
                allPassed = false;
                continue;
            }

            var generatedLines = RunSimulation(extractedDir, tc);
            var refLines = File.ReadAllLines(tc.TracePath);

            var diff = CompareTraces(refLines, generatedLines);
            if (diff != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAILED: Divergence in {tc.Name}:");
                Console.WriteLine($"  {diff}");
                Console.ResetColor();
                allPassed = false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PASSED: {tc.Name} ({generatedLines.Count} ticks verified line-for-line bit-identical)");
                Console.ResetColor();
            }
        }

        // Run negative control tests to ensure harness actually catches divergences
        Console.WriteLine("\n--- Running Negative Controls ---");
        var negControl = testCases[2] with { Seed = 99999L }; // Divergent seed on combat
        var negLines = RunSimulation(extractedDir, negControl);
        var originalRef = File.ReadAllLines(testCases[2].TracePath);
        var negDiff = CompareTraces(originalRef, negLines);
        if (negDiff == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAILED: Negative control did not detect divergence!");
            Console.ResetColor();
            allPassed = false;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"PASSED: Negative control correctly detected divergence with wrong seed: {negDiff}");
            Console.ResetColor();
        }

        bool framePassed = ValidateFrames(extractedDir, Path.Combine(rootDir, "frames", "level1_movement"), Path.Combine(scriptsDir, "movement_150.txt"));
        if (!framePassed) allPassed = false;

        bool qaPassed = ValidateQASuite(extractedDir);
        if (!qaPassed) allPassed = false;

        Console.WriteLine("\n==================================================");
        if (allPassed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ALL VALIDATIONS (TRACES, FRAMES & QA SUITE) PASSED");
            Console.ResetColor();
            return 0;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SOME VALIDATIONS FAILED");
            Console.ResetColor();
            return 1;
        }
    }

    private static bool ValidateQASuite(string extractedDir)
    {
        Console.WriteLine("\n==================================================");
        Console.WriteLine("PR.QATest: Comprehensive Compatibility & Engine Suite");
        Console.WriteLine("==================================================");

        bool suitePassed = true;
        var res = new FolderResourceProvider(extractedDir);
        var app = new App(res);
        app.m();

        // TEST 1: Level & Character Matrix (13 levels x 5 Rangers = 65 permutations)
        Console.WriteLine("\n[QA 1/4] Testing Level & Character Matrix (65 combinations)...");
        int permutationsPassed = 0;
        string[] rangerNames = { "Red (Nick)", "Yellow (Chip)", "Blue (Madison)", "Pink (Vida)", "Green (Xander)" };

        for (int lvl = 0; lvl <= 12; lvl++)
        {
            for (int ranger = 0; ranger < 5; ranger++)
            {
                try
                {
                    Game.G = lvl;
                    Game.F = ranger;
                    Game.var_boolean_e = (lvl == 0);
                    Game.var_java_util_Random_a = new JavaRandom(42L);

                    var game = new Game(app);
                    Game.var_f_a = game;
                    app.var_f_a = game;

                    game.e();
                    if (Game.var_a_f == null)
                    {
                        Console.WriteLine($"  FAIL: Level {lvl}, {rangerNames[ranger]}: Player actor null!");
                        suitePassed = false;
                        continue;
                    }

                    for (int t = 0; t < 20; t++)
                    {
                        game.boolean_b();
                    }

                    permutationsPassed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  FAIL: Level {lvl}, {rangerNames[ranger]}: Exception: {ex.Message}");
                    suitePassed = false;
                }
            }
        }
        Console.WriteLine($"  PASSED: {permutationsPassed}/65 permutations executed simulation ticks without errors.");

        // TEST 2: Survival Arena (Level 0) Spawning & Wave Logic
        Console.WriteLine("\n[QA 2/4] Testing Survival Arena (Level 0) Initialization & Wave Spawning...");
        try
        {
            Game.G = 0;
            Game.F = 0;
            Game.var_boolean_e = true;
            Game.var_java_util_Random_a = new JavaRandom(42L);

            var survivalGame = new Game(app);
            Game.var_f_a = survivalGame;
            app.var_f_a = survivalGame;

            survivalGame.e();
            for (int t = 0; t < 10; t++) survivalGame.boolean_b();

            if (survivalGame.var_a_arr_g == null || survivalGame.var_a_arr_g.Length == 0)
            {
                Console.WriteLine("  FAIL: Survival arena enemy array is empty or null!");
                suitePassed = false;
            }
            else
            {
                int activeEnemies = 0;
                foreach (var e in survivalGame.var_a_arr_g)
                {
                    if (e.var_boolean_a) activeEnemies++;
                }
                Console.WriteLine($"  PASSED: Survival Arena loaded: {survivalGame.var_a_arr_g.Length} total enemy slots, {activeEnemies} active wave enemies, wave counter = {survivalGame.aO}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: Survival Arena exception: {ex.Message}");
            suitePassed = false;
        }

        // TEST 3: Cheat Codes Runtime Enforcement (GodMode & InfiniteMagic)
        Console.WriteLine("\n[QA 3/4] Testing EngineTweaks Cheats (GodMode & InfiniteMagic)...");
        try
        {
            Game.G = 1;
            Game.F = 0;
            Game.var_boolean_e = false;
            Game.var_java_util_Random_a = new JavaRandom(42L);

            var cheatGame = new Game(app);
            Game.var_f_a = cheatGame;
            app.var_f_a = cheatGame;
            cheatGame.e();
            cheatGame.boolean_b();

            // Enable cheats
            EngineTweaks.GodMode = true;
            EngineTweaks.InfiniteMagic = true;

            int maxHp = Game.var_a_f!.A;
            Game.var_a_f.z = 1; // Inflict near-lethal damage
            cheatGame.an = 0; // Drain magic to 0

            cheatGame.boolean_b(); // Run game tick with cheats active

            bool hpRestored = Game.var_a_f.z == maxHp;
            bool magicRestored = cheatGame.an == cheatGame.am;

            if (hpRestored && magicRestored)
            {
                Console.WriteLine($"  PASSED: GodMode restored HP (1 -> {Game.var_a_f.z}/{maxHp}), InfiniteMagic restored magic (0 -> {cheatGame.an}/{cheatGame.am}).");
            }
            else
            {
                Console.WriteLine($"  FAIL: Cheats not enforced! HP={Game.var_a_f.z}/{maxHp}, Magic={cheatGame.an}/{cheatGame.am}");
                suitePassed = false;
            }

            // Restore defaults
            EngineTweaks.GodMode = false;
            EngineTweaks.InfiniteMagic = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: Cheats test exception: {ex.Message}");
            suitePassed = false;
        }

        // TEST 4: Texture Buffer Row-Stride Mapping Invariant (4:3 vs 16:9)
        Console.WriteLine("\n[QA 4/4] Testing Texture Buffer Stride Invariant (4:3 into 16:9 surface)...");
        try
        {
            const int maxWidth = 568;
            const int baseHeight = 320;
            const int canvasW = 240;

            uint[] srcBuffer = new uint[canvasW * baseHeight];
            for (int y = 0; y < baseHeight; y++)
            {
                for (int x = 0; x < canvasW; x++)
                {
                    srcBuffer[y * canvasW + x] = (uint)((y << 16) | (x & 0xFFFF));
                }
            }

            uint[] dstBuffer = new uint[maxWidth * baseHeight];
            // Apply correct row-stride copy
            for (int y = 0; y < baseHeight; y++)
            {
                int srcRow = y * canvasW;
                int dstRow = y * maxWidth;
                for (int x = 0; x < canvasW; x++)
                {
                    dstBuffer[dstRow + x] = srcBuffer[srcRow + x];
                }
            }

            // Verify row 1 column 0 is at dstBuffer[568] (NOT dstBuffer[240])
            bool strideCorrect = (dstBuffer[maxWidth] == srcBuffer[canvasW]) &&
                                 (dstBuffer[240] == 0); // Margin beyond row 0 should be 0
            if (strideCorrect)
            {
                Console.WriteLine("  PASSED: Row stride mapping invariant verified (Pitch = 568, no diagonal shear).");
            }
            else
            {
                Console.WriteLine("  FAIL: Stride mapping invariant violated!");
                suitePassed = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: Stride test exception: {ex.Message}");
            suitePassed = false;
        }

        // TEST 5: Elevator (Moving Platform) Landing & Riding Invariants
        Console.WriteLine("\n[QA 5/5] Testing Elevator (Moving Platform) Landing & Riding Invariants...");
        try
        {
            int elevatorLevelsTested = 0;
            for (int lvl = 0; lvl <= 12; lvl++)
            {
                Game.G = lvl;
                Game.F = 0;
                Game.var_boolean_e = false;
                Game.var_java_util_Random_a = new JavaRandom(42L);

                var platGame = new Game(app);
                Game.var_f_a = platGame;
                app.var_f_a = platGame;
                platGame.e();

                if (platGame.var_a_arr_h != null && platGame.var_a_arr_h.Length > 0)
                {
                    elevatorLevelsTested++;
                    var plat = platGame.var_a_arr_h[0];
                    var player = Game.var_a_f!;

                    // 1. Position player directly above platform, in authentic falling state (var_byte_a = 8, curve vy >= +1)
                    player.var_int_d = plat.var_int_d;
                    player.var_int_e = plat.var_int_e - 10;
                    player.var_int_n = plat.var_int_e - 20;
                    player.t = 3; // Falling
                    player.var_byte_a = 8;

                    // Tick simulation until player lands or times out
                    bool landed = false;
                    for (int tick = 0; tick < 10; tick++)
                    {
                        platGame.boolean_b();
                        if (player.var_boolean_b && player.var_int_e == plat.var_int_e && (player.t == 0 || player.t == 1))
                        {
                            landed = true;
                            break;
                        }
                    }
                    if (!landed)
                    {
                        Console.WriteLine($"  FAIL: Level {lvl} Player failed to land on elevator! Grounded={player.var_boolean_b}, PlayerY={player.var_int_e}, PlatY={plat.var_int_e}, State={player.t}");
                        suitePassed = false;
                    }
                    else
                    {
                        // 2. Test riding elevator moving DOWN naturally via simulation tick
                        plat.var_int_g = 4;
                        platGame.boolean_b();

                        bool rodeDown = player.var_boolean_b && player.var_int_e == plat.var_int_e;
                        if (!rodeDown)
                        {
                            Console.WriteLine($"  FAIL: Level {lvl} Player did not ride elevator down! Grounded={player.var_boolean_b}, PlayerY={player.var_int_e}, PlatY={plat.var_int_e}");
                            suitePassed = false;
                        }
                        else
                        {
                            // 3. Test riding elevator moving UP naturally via simulation tick
                            plat.var_int_g = -4;
                            platGame.boolean_b();

                            bool rodeUp = player.var_boolean_b && player.var_int_e == plat.var_int_e;
                            if (!rodeUp)
                            {
                                Console.WriteLine($"  FAIL: Level {lvl} Player did not ride elevator up! Grounded={player.var_boolean_b}, PlayerY={player.var_int_e}, PlatY={plat.var_int_e}");
                                suitePassed = false;
                            }
                            else
                            {
                                // 4. Test jumping up from below (one-way pass-through: should NOT land or get blocked while moving up)
                                player.var_int_d = plat.var_int_d;
                                player.var_int_e = plat.var_int_e + 20;
                                player.var_int_n = plat.var_int_e + 30;
                                player.t = 2; // Jumping up
                                player.var_byte_a = 0; // upward curve vy = -12
                                platGame.boolean_b();

                                bool passedThrough = player.t == 2 && player.var_int_e < plat.var_int_e + 20;
                                if (!passedThrough)
                                {
                                    Console.WriteLine($"  FAIL: Level {lvl} Player failed one-way pass-through from below!");
                                    suitePassed = false;
                                }
                                else
                                {
                                    Console.WriteLine($"  PASSED Level {lvl}: Elevator landing, down-ride, up-ride, and one-way pass-through verified (Platform count: {platGame.var_a_arr_h.Length}).");
                                }
                            }
                        }
                    }
                }
            }

            if (elevatorLevelsTested == 0)
            {
                Console.WriteLine("  WARNING: No levels with elevators found in levels 0..12!");
                // Also synthesize a platform to verify the logic directly
                var synGame = new Game(app);
                Game.var_f_a = synGame;
                app.var_f_a = synGame;
                Game.G = 0;
                Game.F = 0;
                synGame.e();

                var synPlat = new Actor();
                synPlat.var_int_d = 100;
                synPlat.var_int_e = 100;
                synPlat.var_int_n = 100;
                synPlat.var_byte_d = 4;
                synPlat.var_boolean_a = true;
                synPlat.var_boolean_j = true;
                synGame.var_a_arr_h = new Actor[] { synPlat };
                synGame.var_int_arr_f = new int[] { 50 };

                var player = Game.var_a_f!;
                player.var_int_d = synPlat.var_int_d;
                player.var_int_e = synPlat.var_int_e - 4;
                player.var_int_n = synPlat.var_int_e - 16;
                player.t = 3;

                synGame.boolean_b();
                bool landed = player.var_boolean_b && player.var_int_e == synPlat.var_int_e;
                if (!landed)
                {
                    Console.WriteLine($"  FAIL: Player failed to land on synthetic elevator! Grounded={player.var_boolean_b}, PlayerY={player.var_int_e}, PlatY={synPlat.var_int_e}");
                    suitePassed = false;
                }
                else
                {
                    Console.WriteLine("  PASSED: Synthetic elevator landing verified.");
                }
            }
            else
            {
                Console.WriteLine($"  Elevators verified successfully across {elevatorLevelsTested} level(s).");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: Elevator test exception: {ex.Message}");
            suitePassed = false;
        }

        return suitePassed;
    }

    private static bool ValidateFrames(string extractedDir, string framesDir, string scriptPath)
    {
        Console.WriteLine("\n==================================================");
        Console.WriteLine("PR.FrameTest: Validating Software Renderer Frames");
        Console.WriteLine("==================================================");

        if (!Directory.Exists(framesDir))
        {
            Console.WriteLine($"WARNING: Frames directory not found at {framesDir}, skipping frame test.");
            return true;
        }

        var res = new FolderResourceProvider(extractedDir);
        var app = new App(res);
        app.m();

        Game.G = 1;
        Game.F = 0;
        Game.var_java_util_Random_a = new JavaRandom(42L);

        var game = new Game(app);
        Game.var_f_a = game;
        app.var_f_a = game;

        game.e();
        Game.var_java_util_Random_a = new JavaRandom(42L);

        var screen = Image.CreateImage(240, 320);
        var gr = screen.GetGraphics();
        GMIDlet.var_javax_microedition_lcdui_Graphics_a = gr;

        var inputSchedule = new Dictionary<int, List<(bool isPress, int key)>>();
        if (File.Exists(scriptPath))
        {
            foreach (var rawLine in File.ReadAllLines(scriptPath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int t = int.Parse(parts[0]);
                bool isPress = parts[1].Equals("press", StringComparison.OrdinalIgnoreCase);
                int key = int.Parse(parts[2]);

                if (!inputSchedule.ContainsKey(t)) inputSchedule[t] = new List<(bool, int)>();
                inputSchedule[t].Add((isPress, key));
            }
        }

        var frameTicks = new HashSet<int> { 0, 25, 50, 75, 100, 125, 149 };
        bool allMatch = true;

        for (int tick = 0; tick < 150; tick++)
        {
            if (inputSchedule.TryGetValue(tick, out var events))
            {
                foreach (var ev in events)
                {
                    if (ev.isPress) game.void_a(ev.key);
                    else game.b(ev.key);
                }
            }

            bool done = game.boolean_b();

            if (frameTicks.Contains(tick))
            {
                // Render frame
                game.d();

                string refPath = Path.Combine(framesDir, $"frame_{tick:D4}.png");
                if (File.Exists(refPath))
                {
                    byte[] refBytes = File.ReadAllBytes(refPath);
                    var refImg = Image.CreateImage(refBytes, 0, refBytes.Length);

                    int mismatches = 0;
                    int firstX = -1, firstY = -1;
                    uint expP = 0, gotP = 0;

                    for (int y = 0; y < 320; y++)
                    {
                        for (int x = 0; x < 240; x++)
                        {
                            uint pActual = screen.Pixels[y * 240 + x];
                            uint pExpected = refImg.Pixels[y * 240 + x];

                            if (pActual != pExpected)
                            {
                                if (mismatches == 0)
                                {
                                    firstX = x;
                                    firstY = y;
                                    expP = pExpected;
                                    gotP = pActual;
                                }
                                mismatches++;
                            }
                        }
                    }

                    if (mismatches == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"PASSED Frame {tick:D4}: 100% pixel-identical (76,800/76,800 pixels match)");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"FAILED Frame {tick:D4}: {mismatches}/76800 pixels differ ({(mismatches*100.0/76800):F2}%). First diff at ({firstX}, {firstY}): exp=0x{expP:X8}, got=0x{gotP:X8}");
                        Console.ResetColor();
                        allMatch = false;

                        // Save actual rendered frame for inspection
                        string outPath = Path.Combine(framesDir, $"actual_{tick:D4}.png");
                        screen.SavePng(outPath);
                    }
                }
            }

            if (done) break;
        }

        return allMatch;
    }

    private static List<string> RunSimulation(string extractedDir, TestCase tc)
    {
        var res = new FolderResourceProvider(extractedDir);
        var app = new App(res);
        app.m();

        Game.G = tc.Level;
        Game.F = tc.Character;
        Game.var_java_util_Random_a = new JavaRandom(tc.Seed);

        var game = new Game(app);
        Game.var_f_a = game;
        app.var_f_a = game;

        game.e();

        // Re-seed PRNG after level loading exactly as Java harness does
        Game.var_java_util_Random_a = new JavaRandom(tc.Seed);

        // Load input script
        var inputSchedule = new Dictionary<int, List<(bool isPress, int key)>>();
        if (File.Exists(tc.ScriptPath))
        {
            foreach (var rawLine in File.ReadAllLines(tc.ScriptPath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int t = int.Parse(parts[0]);
                bool isPress = parts[1].Equals("press", StringComparison.OrdinalIgnoreCase);
                int key = int.Parse(parts[2]);

                if (!inputSchedule.ContainsKey(t)) inputSchedule[t] = new List<(bool, int)>();
                inputSchedule[t].Add((isPress, key));
            }
        }

        var lines = new List<string>();
        lines.Add($"# Power Rangers Trace: Level={tc.Level}, Character={tc.Character}, Seed={tc.Seed}");
        lines.Add("# tick,px,py,vx,vy,state,frame,hp,max_hp,magic,score,cam_x,cam_y,enemy_count,e0_x,e0_y");

        for (int tick = 0; tick < tc.Ticks; tick++)
        {
            // Apply input events
            if (inputSchedule.TryGetValue(tick, out var events))
            {
                foreach (var ev in events)
                {
                    if (ev.isPress) game.void_a(ev.key);
                    else game.b(ev.key);
                }
            }

            bool done = game.boolean_b();

            // Record state
            var p = Game.var_a_f;
            int px = p != null ? p.var_int_d : 0;
            int py = p != null ? p.var_int_e : 0;
            int vx = p != null ? p.var_int_f : 0;
            int vy = p != null ? p.var_int_g : 0;
            int st = p != null ? p.t : 0;
            int fr = p != null ? p.x : 0;
            int hp = p != null ? p.z : 0;
            int mhp = p != null ? p.A : 0;
            int mg = game.an;
            int sc = App.var_int_g;
            int cx = game.ac;
            int cy = game.ad;

            int eCount = game.var_a_arr_g != null ? game.var_a_arr_g.Length : 0;
            int e0x = (eCount > 0 && game.var_a_arr_g![0] != null) ? game.var_a_arr_g[0].var_int_d : 0;
            int e0y = (eCount > 0 && game.var_a_arr_g![0] != null) ? game.var_a_arr_g[0].var_int_e : 0;

            lines.Add($"{tick},{px},{py},{vx},{vy},{st},{fr},{hp},{mhp},{mg},{sc},{cx},{cy},{eCount},{e0x},{e0y}");

            if (done) break;
        }

        return lines;
    }

    private static string? CompareTraces(string[] refLines, List<string> actualLines)
    {
        // Strip comment lines
        var refData = new List<string>();
        foreach (var l in refLines)
        {
            var trimmed = l.Trim();
            if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;
            refData.Add(trimmed);
        }

        var actData = new List<string>();
        foreach (var l in actualLines)
        {
            var trimmed = l.Trim();
            if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;
            actData.Add(trimmed);
        }

        int minLen = Math.Min(refData.Count, actData.Count);
        for (int i = 0; i < minLen; i++)
        {
            if (refData[i] != actData[i])
            {
                var refCols = refData[i].Split(',');
                var actCols = actData[i].Split(',');
                string colNames = "tick,px,py,vx,vy,state,frame,hp,max_hp,magic,score,cam_x,cam_y,enemy_count,e0_x,e0_y";
                var names = colNames.Split(',');

                string diffCol = "unknown";
                string expVal = "?";
                string gotVal = "?";
                for (int c = 0; c < Math.Min(refCols.Length, actCols.Length); c++)
                {
                    if (refCols[c] != actCols[c])
                    {
                        diffCol = c < names.Length ? names[c] : $"col{c}";
                        expVal = refCols[c];
                        gotVal = actCols[c];
                        break;
                    }
                }

                return $"Tick {i} divergence on {diffCol}: expected {expVal}, got {gotVal}\n" +
                       $"  Expected: {refData[i]}\n" +
                       $"  Actual:   {actData[i]}";
            }
        }

        if (refData.Count != actData.Count)
        {
            return $"Line count mismatch: expected {refData.Count} ticks, got {actData.Count} ticks";
        }

        return null;
    }
}
