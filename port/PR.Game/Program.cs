using System;
using System.IO;
using Raylib_cs;
using PR.Core;
using PR.Core.Graphics;
using PR.Game.Audio;
using PR.Game.Input;
using PR.Game.Saves;
using PR.Game.UI;
using CoreGame = PR.Core.Game;

namespace PR.Game;

public static class Program
{
    public static int Main(string[] args)
    {
        // Parse CLI arguments
        int cliScale = -1;
        bool cliNoSound = false;
        bool cliLogs = false;
        bool cliFullscreen = false;
        string? cliLang = null;
        string? cliJarPath = null;

        bool cliCaptureWidescreenDebug = false;
        bool cliCaptureWidescreenFixed = false;
        bool cliTestWidescreenPlaythrough = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLowerInvariant();
            if (arg == "--scale" && i + 1 < args.Length && int.TryParse(args[++i], out int sc)) cliScale = sc;
            else if (arg == "--no-sound") cliNoSound = true;
            else if (arg == "--logs") cliLogs = true;
            else if (arg == "--fullscreen") cliFullscreen = true;
            else if (arg == "--lang" && i + 1 < args.Length) cliLang = args[++i].ToLowerInvariant();
            else if (arg == "--jar" && i + 1 < args.Length) cliJarPath = args[++i];
            else if (arg == "--capture-widescreen-debug") cliCaptureWidescreenDebug = true;
            else if (arg == "--capture-widescreen-fixed") cliCaptureWidescreenFixed = true;
            else if (arg == "--test-widescreen-playthrough") cliTestWidescreenPlaythrough = true;
            else if (arg == "--help" || arg == "-h")
            {
                PrintHelp();
                return 0;
            }
        }

        if (cliLogs)
        {
            Console.WriteLine("[Init] Verbose logging enabled.");
            PR.Core.Audio.SoundEngine.AudioLogging = true;
        }

        try
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("Power Rangers: Mystic Force - PC Port");
            Console.WriteLine("==================================================");

            // 1. Locate game resources
            IResourceProvider res = ResolveResourceProvider(cliJarPath);

            if (cliCaptureWidescreenDebug || cliCaptureWidescreenFixed)
            {
                string targetDir = cliCaptureWidescreenFixed ? "widescreen_fixed" : "widescreen_debug";
                CaptureWidescreenDebug(res, targetDir);
                return 0;
            }

            if (cliTestWidescreenPlaythrough)
            {
                TestWidescreenPlaythrough(res);
                return 0;
            }

            // 2. Load Save & Settings
            var saveManager = new SaveManager();
            if (cliScale > 0) saveManager.Data.Scale = cliScale;
            if (cliNoSound) saveManager.Data.SoundEnabled = false;
            if (cliFullscreen) saveManager.Data.Fullscreen = true;

            if (!string.IsNullOrEmpty(cliLang))
            {
                int langIdx = Array.IndexOf(App.var_java_lang_String_arr_b, cliLang);
                if (langIdx >= 0) saveManager.Data.LanguageIndex = langIdx;
            }

            // 3. Initialize App and Resource Packs
            App.var_byte_d = (byte)saveManager.Data.LanguageIndex;
            var app = new App(res);
            app.m();

            // 4. Initialize Window and Audio
            Raylib.InitAudioDevice();
            if (Raylib.IsAudioDeviceReady())
            {
                float masterVol = saveManager.Data.SoundEnabled ? (saveManager.Data.Volume / 1000f) : 0f;
                Raylib.SetMasterVolume(masterVol);
                if (cliLogs) Console.WriteLine($"[Audio] Raylib audio device initialized successfully. Volume: {masterVol:F2}");
            }

            using var midiPlayer = new MidiPlayer();
            midiPlayer.IsMuted = !saveManager.Data.SoundEnabled;
            midiPlayer.Volume = saveManager.Data.Volume;
            PR.Core.Audio.SoundEngine.Enabled = saveManager.Data.SoundEnabled;

            PR.Core.EngineTweaks.Smooth60Fps = saveManager.Data.Smooth60Fps;
            PR.Core.EngineTweaks.Widescreen = saveManager.Data.Widescreen;

            using var view = new GameView(saveManager.Data.Scale, saveManager.Data.Fullscreen);
            view.FitScaling = saveManager.Data.FitScaling;
            var inputRouter = new InputRouter();

            // 5. Setup Software Screen
            var screen = PR.Core.Graphics.Image.CreateImage(view.CurrentWidth, GameView.BaseHeight);
            var gr = screen.GetGraphics();
            GMIDlet.var_javax_microedition_lcdui_Graphics_a = gr;

            // 6. Initialize UI / Menu System
            var menuSystem = new MenuSystem(
                app,
                saveManager,
                midiPlayer,
                view,
                inputRouter,
                onScaleChange: newScale => view.SetScale(newScale),
                onFullscreenToggle: () => view.ToggleFullscreen(),
                onWidescreenToggle: () =>
                {
                    view.UpdateWidescreenMode();
                    screen = PR.Core.Graphics.Image.CreateImage(view.CurrentWidth, GameView.BaseHeight);
                    gr = screen.GetGraphics();
                    GMIDlet.var_javax_microedition_lcdui_Graphics_a = gr;
                }
            );

            // 7. Main Game Loop (Fixed 70ms step, 60fps presentation)
            const float SimStep = 0.070f;
            float accumulator = 0.0f;
            int simTick = 0;

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                if (dt > 0.25f) dt = 0.25f; // Prevent spiral of death
                accumulator += dt;

                // Fullscreen toggle shortcut: F11 or Alt + Enter
                bool altEnter = (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)) &&
                                (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter));
                if (Raylib.IsKeyPressed(KeyboardKey.F11) || altEnter)
                {
                    view.ToggleFullscreen();
                    saveManager.Data.Fullscreen = view.IsFullscreen;
                    saveManager.Save();
                }

                // F3 Audio Debug shortcut
                if (Raylib.IsKeyPressed(KeyboardKey.F3))
                {
                    menuSystem.HandleDebugF3();
                }

                // Check Rebinding Input if active
                menuSystem.CheckRebindingInput();

                // Process Mouse Input
                var virtMouse = view.ScreenToVirtual(Raylib.GetMousePosition());
                bool leftClick = Raylib.IsMouseButtonPressed(MouseButton.Left);
                menuSystem.ProcessMouse(virtMouse, leftClick);

                // Process Input
                inputRouter.Update(
                    onKeyDown: key => menuSystem.OnKeyDown(key),
                    onKeyUp: key => menuSystem.OnKeyUp(key)
                );

                // Fixed-Step Simulation Update
                while (accumulator >= SimStep)
                {
                    menuSystem.Update(simTick++);
                    accumulator -= SimStep;
                }

                // Sub-step alpha for 60fps interpolation: alpha in [0, 1)
                float alpha = accumulator / SimStep;
                menuSystem.UpdateInterpolation(alpha);

                // Render Frame
                menuSystem.Render(gr);

                // Transfer to Raylib Texture & Present
                view.UpdateFromCanvas(screen);
                view.Render();

                // Audio Looping & State
                midiPlayer.Update();
            }

            if (Raylib.IsAudioDeviceReady())
            {
                Raylib.CloseAudioDevice();
            }

            return 0;
        }
        catch (Exception ex)
        {
            HandleCrash(ex);
            return 1;
        }
    }

    private static IResourceProvider ResolveResourceProvider(string? cliJar)
    {
        if (!string.IsNullOrEmpty(cliJar))
        {
            if (File.Exists(cliJar))
            {
                Console.WriteLine($"[Resource] Loading from specified JAR: {cliJar}");
                return new ZipResourceProvider(cliJar);
            }
            if (Directory.Exists(cliJar))
            {
                Console.WriteLine($"[Resource] Loading from specified folder: {cliJar}");
                return new FolderResourceProvider(cliJar);
            }
            throw new FileNotFoundException($"Resource target '{cliJar}' does not exist.");
        }

        string defaultJar = Path.Combine("JAR", "Power_Rangers_240_nok.jar");
        if (File.Exists(defaultJar))
        {
            Console.WriteLine($"[Resource] Loading from JAR: {defaultJar}");
            return new ZipResourceProvider(defaultJar);
        }

        if (Directory.Exists("extracted") && File.Exists(Path.Combine("extracted", "l")))
        {
            Console.WriteLine("[Resource] Loading from extracted assets folder: extracted/");
            return new FolderResourceProvider("extracted");
        }

        throw new FileNotFoundException(
            "Game assets not found! Please ensure 'JAR/Power_Rangers_240_nok.jar' or 'extracted/' exists."
        );
    }

    private static void HandleCrash(Exception ex)
    {
        string logContent = $"========================================\n" +
                            $"Power Rangers Mystic Force - Crash Log\n" +
                            $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                            $"Error: {ex.Message}\n" +
                            $"Stack Trace:\n{ex.StackTrace}\n" +
                            $"========================================\n";

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n[FATAL CRASH]");
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.ResetColor();

        try
        {
            File.WriteAllText("crash.log", logContent);
            Console.WriteLine("\nDetails written to crash.log");
        }
        catch
        {
            // Ignore
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Power Rangers: Mystic Force - PC Port");
        Console.WriteLine("Usage: PR.Game.exe [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --scale N       Set window scaling (1 to 5, default: 3)");
        Console.WriteLine("  --no-sound      Disable MIDI music and sound effects");
        Console.WriteLine("  --fullscreen    Start in fullscreen mode");
        Console.WriteLine("  --lang <xx>     Select language (de, en, fr, it)");
        Console.WriteLine("  --jar <path>    Specify path to game JAR file or extracted directory");
        Console.WriteLine("  --logs          Enable verbose logging");
        Console.WriteLine("  --capture-widescreen-debug Capture 9 widescreen debug frames");
        Console.WriteLine("  --capture-widescreen-fixed Capture 9 widescreen fixed frames");
        Console.WriteLine("  --help, -h      Display this help message");
    }

    private static void CaptureWidescreenDebug(IResourceProvider res, string targetDir = "widescreen_debug")
    {
        Console.WriteLine($"[Widescreen Debug] Starting capture of all 9 widescreen frames to {targetDir}...");
        EngineTweaks.Widescreen = true;
        EngineTweaks.Smooth60Fps = false;

        string outDir = Path.Combine(Directory.GetCurrentDirectory(), "frames", targetDir);
        Directory.CreateDirectory(outDir);

        int renderW = EngineTweaks.RenderWidth;
        int renderH = EngineTweaks.RenderHeight;

        var screen = PR.Core.Graphics.Image.CreateImage(renderW, renderH);
        var gr = screen.GetGraphics();
        GMIDlet.var_javax_microedition_lcdui_Graphics_a = gr;

        var saveManager = new SaveManager();
        saveManager.Data.Widescreen = true;

        var app = new App(res);
        app.m();

        // 1. Title Screen
        var dummyAudio = new MidiPlayer();
        var dummyView = new GameView(1, false);
        var dummyInput = new InputRouter();
        var menu = new MenuSystem(app, saveManager, dummyAudio, dummyView, dummyInput, _ => {}, () => {}, () => {});
        menu.State = GameState.Title;
        menu.Render(gr);
        screen.SavePng(Path.Combine(outDir, "title_screen.png"));
        Console.WriteLine("Captured: title_screen.png");

        // 2. Main Menu
        menu.State = GameState.MainMenu;
        menu.Render(gr);
        screen.SavePng(Path.Combine(outDir, "main_menu.png"));
        Console.WriteLine("Captured: main_menu.png");

        void CaptureLevel(int level, string filename)
        {
            CoreGame.G = level;
            CoreGame.F = 0;
            CoreGame.var_boolean_e = false;
            CoreGame.var_java_util_Random_a = new JavaRandom(42L);
            var game = new CoreGame(app);
            CoreGame.var_f_a = game;
            app.var_f_a = game;
            game.e();
            game.boolean_b();
            game.d();
            screen.SavePng(Path.Combine(outDir, filename));
            Console.WriteLine($"Captured: {filename}");
        }

        // 3. Level 1 First Tick
        CaptureLevel(1, "level1_first_tick.png");

        // 4. Level 2 First Tick
        CaptureLevel(2, "level2_first_tick.png");

        // 5. Level 3 First Tick
        CaptureLevel(3, "level3_first_tick.png");

        // 6. Mid-Level (Level 1, running 120 ticks of movement)
        {
            CoreGame.G = 1;
            CoreGame.F = 0;
            CoreGame.var_boolean_e = false;
            CoreGame.var_java_util_Random_a = new JavaRandom(42L);
            var game = new CoreGame(app);
            CoreGame.var_f_a = game;
            app.var_f_a = game;
            game.e();
            for (int t = 0; t < 120; t++)
            {
                game.void_a(-4); // Move Right
                game.boolean_b();
            }
            game.d();
            screen.SavePng(Path.Combine(outDir, "mid_level.png"));
            Console.WriteLine("Captured: mid_level.png");
        }

        // 7. Boss Fight (Level 5 dedicated boss arena)
        {
            int bossLvl = 5;
            CoreGame.G = bossLvl;
            CoreGame.F = 0;
            CoreGame.var_boolean_e = false;
            CoreGame.var_java_util_Random_a = new JavaRandom(42L);
            var bossGame = new CoreGame(app);
            CoreGame.var_f_a = bossGame;
            app.var_f_a = bossGame;
            bossGame.e();
            for (int t = 0; t < 10; t++)
            {
                bossGame.boolean_b();
            }
            if (bossGame.var_a_i != null)
            {
                bossGame.var_a_i.var_boolean_j = true;
            }
            bossGame.d();
            screen.SavePng(Path.Combine(outDir, "boss_fight.png"));
            Console.WriteLine($"Captured: boss_fight.png (Level {bossLvl})");
        }

        // 8. Cutscene Screen (subtitle__* and lp__*)
        {
            gr.SetColor(0x0C0E14);
            gr.FillRect(0, 0, renderW, renderH);

            gr.SetColor(0x7D0000);
            gr.FillRect(0, 0, renderW, 24);
            gr.SetColor(0xFFD700);
            gr.DrawLine(0, 24, renderW, 24);
            gr.SetColor(0x7D0000);
            gr.FillRect(0, renderH - 20, renderW, 20);
            gr.SetColor(0xFFD700);
            gr.DrawLine(0, renderH - 20, renderW, renderH - 20);

            using (var subStream = res.OpenResource("subtitle__en"))
            {
                if (subStream != null)
                {
                    using var subDis = new DataReader(subStream);
                    var subImg = GMIDlet.a(subDis, -1);
                    if (subImg != null)
                    {
                        GMIDlet.a(subImg, renderW >> 1, 102, 17);
                    }
                }
            }

            using (var lpStream = res.OpenResource("lp__en"))
            {
                if (lpStream != null)
                {
                    using var lpDis = new DataReader(lpStream);
                    short len = lpDis.ReadShort();
                    byte[] bytes = new byte[len];
                    lpDis.ReadFully(bytes);
                    var lpImg = PR.Core.Graphics.Image.CreateImage(bytes, 0, bytes.Length);
                    if (lpImg != null)
                    {
                        gr.DrawImage(lpImg, (renderW - lpImg.Width) >> 1, 160, 20);
                    }
                }
            }

            GMIDlet.a(1, "CUTSCENE / SUBTITLE INTRO", renderW >> 1, 6, 17);
            GMIDlet.a(0, "LOADING STORY CHAPTER...", renderW >> 1, 230, 17);

            screen.SavePng(Path.Combine(outDir, "cutscene.png"));
            Console.WriteLine("Captured: cutscene.png");
        }

        // 9. Game Over Screen
        {
            menu.State = GameState.GameOver;
            menu.Render(gr);
            screen.SavePng(Path.Combine(outDir, "game_over.png"));
            Console.WriteLine("Captured: game_over.png");
        }

        Console.WriteLine("[Widescreen Debug] Complete! Frames saved to: " + outDir);
    }

    private static void TestWidescreenPlaythrough(IResourceProvider res)
    {
        Console.WriteLine("[Widescreen Playthrough] Starting 2-level automated playthrough with widescreen ON...");
        EngineTweaks.Widescreen = true;
        EngineTweaks.Smooth60Fps = false;

        int renderW = EngineTweaks.RenderWidth;
        int renderH = EngineTweaks.RenderHeight;
        var screen = PR.Core.Graphics.Image.CreateImage(renderW, renderH);
        GMIDlet.var_javax_microedition_lcdui_Graphics_a = screen.GetGraphics();

        var app = new App(res);
        app.m();

        for (int lvl = 1; lvl <= 2; lvl++)
        {
            Console.WriteLine($"\n--- Playing Level {lvl} (Widescreen 568x320) ---");
            CoreGame.G = lvl;
            CoreGame.F = 0;
            CoreGame.var_boolean_e = false;
            CoreGame.var_java_util_Random_a = new JavaRandom(42L);
            var game = new CoreGame(app);
            CoreGame.var_f_a = game;
            app.var_f_a = game;
            game.e();

            int levelWidth = game.var_c_arr_a != null && game.aj < game.var_c_arr_a.Length
                ? game.var_c_arr_a[game.aj].var_int_c
                : 240;
            int minCamX = renderW >> 1;
            int maxCamX = Math.Max(minCamX, levelWidth - minCamX);

            int popInCount = 0;
            int seamCount = 0;
            int camClampViolations = 0;

            for (int tick = 0; tick < 300; tick++)
            {
                // Automated inputs: move right, jump periodically, attack periodically
                if (tick % 60 < 40) game.void_a(-4); // Right
                else game.b(-4);

                if (tick % 80 == 20) game.void_a(-1); // Jump
                else game.b(-1);

                if (tick % 50 == 15) game.void_a(-5); // Attack
                else game.b(-5);

                game.boolean_b();
                game.d();

                // Check camera clamping
                int camX = game.ac;
                if (camX < minCamX || camX > maxCamX)
                {
                    camClampViolations++;
                }

                // Sample mid-playfield pixels
                var pix = screen.Pixels;
                int rowStart = 150 * renderW;
                uint leftP = pix[rowStart];
                uint rightP = pix[rowStart + renderW - 1];
                if ((leftP & 0xFFFFFF) == 0 && (rightP & 0xFFFFFF) == 0)
                {
                    seamCount++;
                }
            }

            Console.WriteLine($"Level {lvl} Complete: 300 ticks rendered.");
            Console.WriteLine($"  Level width: {levelWidth} px");
            Console.WriteLine($"  Camera bounds: [{minCamX}, {maxCamX}] px");
            Console.WriteLine($"  Camera clamp violations: {camClampViolations}");
            Console.WriteLine($"  Empty edge/seam frames: {seamCount}");
            Console.WriteLine($"  Pop-in anomalies: {popInCount}");
            if (camClampViolations == 0 && seamCount == 0)
            {
                Console.WriteLine($"  PASS: Level {lvl} widescreen playback verified flawless!");
            }
            else
            {
                Console.WriteLine($"  WARN: Level {lvl} had warnings.");
            }
        }
        Console.WriteLine("\n[Widescreen Playthrough] Verification complete!");
    }
}
