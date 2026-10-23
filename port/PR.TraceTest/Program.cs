using System;
using System.Collections.Generic;
using System.IO;
using PR.Core;

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

        Console.WriteLine("\n==================================================");
        if (allPassed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ALL 4 TRACE SETS IDENTICAL TO ORIGINAL BYTECODE");
            Console.ResetColor();
            return 0;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SOME TRACE SETS FAILED VALIDATION");
            Console.ResetColor();
            return 1;
        }
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
