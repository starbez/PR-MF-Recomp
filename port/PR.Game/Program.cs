using System;
using System.IO;
using Raylib_cs;
using PR.Core;
using PR.Core.Graphics;
using PR.Game.Audio;
using PR.Game.Input;
using PR.Game.Saves;
using PR.Game.UI;

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

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLowerInvariant();
            if (arg == "--scale" && i + 1 < args.Length && int.TryParse(args[++i], out int sc)) cliScale = sc;
            else if (arg == "--no-sound") cliNoSound = true;
            else if (arg == "--logs") cliLogs = true;
            else if (arg == "--fullscreen") cliFullscreen = true;
            else if (arg == "--lang" && i + 1 < args.Length) cliLang = args[++i].ToLowerInvariant();
            else if (arg == "--jar" && i + 1 < args.Length) cliJarPath = args[++i];
            else if (arg == "--help" || arg == "-h")
            {
                PrintHelp();
                return 0;
            }
        }

        try
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("Power Rangers: Mystic Force - PC Port");
            Console.WriteLine("==================================================");

            // 1. Locate game resources
            IResourceProvider res = ResolveResourceProvider(cliJarPath);

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
            using var midiPlayer = new MidiPlayer();
            midiPlayer.IsMuted = !saveManager.Data.SoundEnabled;
            midiPlayer.Volume = saveManager.Data.Volume;

            using var view = new GameView(saveManager.Data.Scale, saveManager.Data.Fullscreen);
            var inputRouter = new InputRouter();

            // 5. Setup 240x320 Software Screen
            var screen = PR.Core.Graphics.Image.CreateImage(GameView.BaseWidth, GameView.BaseHeight);
            var gr = screen.GetGraphics();
            GMIDlet.var_javax_microedition_lcdui_Graphics_a = gr;

            // 6. Initialize UI / Menu System
            var menuSystem = new MenuSystem(
                app,
                saveManager,
                midiPlayer,
                onScaleChange: newScale => view.SetScale(newScale),
                onFullscreenToggle: () => view.ToggleFullscreen()
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

                // F11 Fullscreen toggle shortcut
                if (Raylib.IsKeyPressed(KeyboardKey.F11))
                {
                    view.ToggleFullscreen();
                    saveManager.Data.Fullscreen = view.IsFullscreen;
                    saveManager.Save();
                }

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

                // Render Frame
                menuSystem.Render(gr);

                // Transfer to Raylib Texture & Present
                view.UpdateFromCanvas(screen);
                view.Render();
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
        Console.WriteLine("  --help, -h      Display this help message");
    }
}
