using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PR.Game.Saves;

public sealed class SaveData
{
    public int LanguageIndex { get; set; } = 1; // 0=de, 1=en, 2=fr, 3=it
    public bool SoundEnabled { get; set; } = true;
    public int Volume { get; set; } = 800; // 0..1000
    public int Scale { get; set; } = 3;
    public bool Fullscreen { get; set; } = false;
    public bool Smooth60Fps { get; set; } = true; // Default Smooth (60)
    public int TargetFps { get; set; } = 60; // 14, 60, 120, 144, 240, 0 (unlimited)
    public bool Widescreen { get; set; } = true;  // Default Widescreen
    public bool FitScaling { get; set; } = false; // Default Integer scaling
    public int SelectedCharacter { get; set; } = 0; // 0..4
    public int HighestUnlockedLevel { get; set; } = 1; // Default 1 for authentic progression
    public bool GodMode { get; set; } = false; // Infinite HP debug cheat
    public bool InfiniteMagic { get; set; } = false; // Infinite Magic debug cheat

    public int[] StoryHighScores { get; set; } = new int[5] { 50000, 40000, 30000, 20000, 10000 };
    public string[] StoryHighScoreNames { get; set; } = new string[5] { "RED", "YEL", "BLU", "PNK", "GRN" };

    public int[] SurvivalHighScores { get; set; } = new int[5] { 25000, 20000, 15000, 10000, 5000 };
    public string[] SurvivalHighScoreNames { get; set; } = new string[5] { "NICK", "CHIP", "MADI", "VIDA", "XAND" };
}

public sealed class SaveManager
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PowerRangers"
    );

    private static readonly string SaveFilePath = Path.Combine(AppDataDir, "save.json");

    public SaveData Data { get; private set; }

    public SaveManager()
    {
        Data = Load();
    }

    public SaveData Load()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                var loaded = JsonSerializer.Deserialize<SaveData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (loaded != null)
                {
                    // Ensure arrays are 5 elements
                    if (loaded.StoryHighScores == null || loaded.StoryHighScores.Length != 5)
                        loaded.StoryHighScores = new int[5] { 50000, 40000, 30000, 20000, 10000 };
                    if (loaded.StoryHighScoreNames == null || loaded.StoryHighScoreNames.Length != 5)
                        loaded.StoryHighScoreNames = new string[5] { "RED", "YEL", "BLU", "PNK", "GRN" };
                    if (loaded.SurvivalHighScores == null || loaded.SurvivalHighScores.Length != 5)
                        loaded.SurvivalHighScores = new int[5] { 25000, 20000, 15000, 10000, 5000 };
                    if (loaded.SurvivalHighScoreNames == null || loaded.SurvivalHighScoreNames.Length != 5)
                        loaded.SurvivalHighScoreNames = new string[5] { "NICK", "CHIP", "MADI", "VIDA", "XAND" };

                    loaded.HighestUnlockedLevel = Math.Clamp(loaded.HighestUnlockedLevel, 1, 12);

                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveManager] Failed to load save file: {ex.Message}");
        }

        return new SaveData();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppDataDir);
            string json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveManager] Failed to write save file: {ex.Message}");
        }
    }

    public void ResetToDefaults()
    {
        Data.LanguageIndex = 1;
        Data.SoundEnabled = true;
        Data.Volume = 800;
        Data.Scale = 3;
        Data.Fullscreen = false;
        Data.Smooth60Fps = true;
        Data.TargetFps = 60;
        Data.Widescreen = true;
        Data.FitScaling = false;
        Data.HighestUnlockedLevel = 1;
        Data.GodMode = false;
        Data.InfiniteMagic = false;
        Save();
    }

    public bool CheckHighScore(int score, bool survival, out int rank)
    {
        int[] table = survival ? Data.SurvivalHighScores : Data.StoryHighScores;
        for (int i = 0; i < 5; ++i)
        {
            if (score > table[i])
            {
                rank = i;
                return true;
            }
        }
        rank = -1;
        return false;
    }

    public void InsertHighScore(int score, string name, bool survival)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "RNG";
        name = name.Trim().ToUpperInvariant();
        if (name.Length > 4) name = name[..4];

        int[] scores = survival ? Data.SurvivalHighScores : Data.StoryHighScores;
        string[] names = survival ? Data.SurvivalHighScoreNames : Data.StoryHighScoreNames;

        for (int i = 0; i < 5; ++i)
        {
            if (score > scores[i])
            {
                for (int j = 4; j > i; --j)
                {
                    scores[j] = scores[j - 1];
                    names[j] = names[j - 1];
                }
                scores[i] = score;
                names[i] = name;
                Save();
                break;
            }
        }
    }

    public void ResetHighScores()
    {
        Data.StoryHighScores = new int[5] { 50000, 40000, 30000, 20000, 10000 };
        Data.StoryHighScoreNames = new string[5] { "RED", "YEL", "BLU", "PNK", "GRN" };
        Data.SurvivalHighScores = new int[5] { 25000, 20000, 15000, 10000, 5000 };
        Data.SurvivalHighScoreNames = new string[5] { "NICK", "CHIP", "MADI", "VIDA", "XAND" };
        Save();
    }
}
