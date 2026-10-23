using System;
using System.IO;
using System.Numerics;
using Raylib_cs;
using PR.Core;
using PR.Core.Graphics;
using PR.Game.Audio;
using PR.Game.Input;
using PR.Game.Saves;
using CoreGame = PR.Core.Game;
using CoreImage = PR.Core.Graphics.Image;

namespace PR.Game.UI;

public enum GameState
{
    Title,
    MainMenu,
    CharacterSelect,
    LevelSelect,
    HighScores,
    Options,
    Rebind,
    Help,
    InGame,
    Paused,
    NameEntry,
    GameOver,
    Victory
}

public sealed class MenuSystem
{
    private readonly App _app;
    private readonly SaveManager _saveManager;
    private readonly MidiPlayer _audio;
    private readonly GameView _view;
    private readonly InputRouter _inputRouter;
    private readonly Action<int> _onScaleChange;
    private readonly Action _onFullscreenToggle;
    private readonly Action _onWidescreenToggle;

    public GameState State { get; internal set; } = GameState.Title;
    public CoreGame? ActiveGame { get; private set; }

    private int _menuIndex = 0;
    private int _subIndex = 0;
    private int _selectedCharacter = 0;
    private int _selectedLevel = 1;
    private bool _isSurvivalMode = false;
    private int _lastFinalScore = 0;
    private char[] _nameInput = new char[4] { 'A', 'A', 'A', ' ' };
    private int _nameCursor = 0;
    private int _animTick = 0;
    private bool _returnToPause = false;
    private int _rebindingAction = -1;

    private static readonly string[] RangerNames = { "NICK (RED)", "CHIP (YELLOW)", "MADISON (BLUE)", "VIDA (PINK)", "XANDER (GREEN)" };
    private static readonly string[] RangerElements = { "FIRE / BALANCED", "LIGHTNING / SPEED", "WATER / MAGIC", "WIND / AGILITY", "EARTH / STRENGTH" };
    private static readonly string[] RangerDescriptionsFallback = {
        "His fireballs destroy enemies and walls.",
        "Can electrocute enemies and run devices.",
        "Her watery double confuses or deluges.",
        "She can roll through enemies or narrow spots.",
        "Can open a way or paralyze by sprouting."
    };

    private static readonly string[] StageNames = {
        "MAGICAL FOREST", "MAGICAL FOREST", "MAGICAL FOREST",
        "CITY CENTER", "CITY CENTER", "CITY CENTER",
        "BEACH", "BEACH", "BEACH",
        "HEADQUARTERS", "HEADQUARTERS",
        "BOSS ENCOUNTER",
        "SURVIVAL ARENA"
    };

    private readonly Actor?[] _characterPreviews = new Actor?[5];

    public MenuSystem(
        App app,
        SaveManager saveManager,
        MidiPlayer audio,
        GameView view,
        InputRouter inputRouter,
        Action<int> onScaleChange,
        Action onFullscreenToggle,
        Action onWidescreenToggle)
    {
        _app = app;
        _saveManager = saveManager;
        _audio = audio;
        _view = view;
        _inputRouter = inputRouter;
        _onScaleChange = onScaleChange;
        _onFullscreenToggle = onFullscreenToggle;
        _onWidescreenToggle = onWidescreenToggle;

        _selectedCharacter = Math.Clamp(_saveManager.Data.SelectedCharacter, 0, 4);
        _audio.IsMuted = !_saveManager.Data.SoundEnabled;
        _audio.Volume = _saveManager.Data.Volume;
        PR.Core.EngineTweaks.Smooth60Fps = _saveManager.Data.Smooth60Fps;
        PR.Core.EngineTweaks.Widescreen = _saveManager.Data.Widescreen;
        _view.FitScaling = _saveManager.Data.FitScaling;

        PlayMenuMusic();
    }

    public void UpdateInterpolation(float alpha)
    {
        if (State == GameState.InGame && ActiveGame != null)
        {
            ActiveGame.UpdateInterpolation(alpha);
        }
    }

    private int _audioTestState = 0; // 0=STOPPED, 1=SOUND 1, 2=SOUND 2, 3=STOP

    private void PlayMenuMusic()
    {
        PR.Core.Audio.SoundEngine.Play(0);
    }

    private void PlayGameMusic()
    {
        PR.Core.Audio.SoundEngine.Play(1);
    }

    public void HandleDebugF3()
    {
        _audioTestState = (_audioTestState + 1) % 3 + 1; // Cycle 1 -> 2 -> 3
        if (_audioTestState == 1) PR.Core.Audio.SoundEngine.PlaySoundDirect(1, true);
        else if (_audioTestState == 2) PR.Core.Audio.SoundEngine.PlaySoundDirect(2, true);
        else
        {
            _audioTestState = 0;
            PR.Core.Audio.SoundEngine.Stop();
        }
    }

    public void Update(int tick)
    {
        _animTick++;

        // Advance character preview animation frame periodically
        if (State == GameState.CharacterSelect && (_animTick % 4) == 0)
        {
            _characterPreviews[_selectedCharacter]?.void_j();
        }

        if (State == GameState.InGame)
        {
            if (ActiveGame != null)
            {
                bool done = ActiveGame.boolean_b();
                if (done)
                {
                    _lastFinalScore = App.var_int_g;
                    ActiveGame = null;

                    bool qualifies = _saveManager.CheckHighScore(_lastFinalScore, _isSurvivalMode, out _);
                    if (qualifies)
                    {
                        _nameCursor = 0;
                        State = GameState.NameEntry;
                    }
                    else
                    {
                        State = GameState.GameOver;
                    }
                    PlayMenuMusic();
                }
            }
        }
    }

    public void OnKeyDown(int keyCode)
    {
        switch (State)
        {
            case GameState.Title:
                State = GameState.MainMenu;
                _menuIndex = 0;
                break;

            case GameState.MainMenu:
                HandleMainMenuInput(keyCode);
                break;

            case GameState.CharacterSelect:
                HandleCharacterSelectInput(keyCode);
                break;

            case GameState.LevelSelect:
                HandleLevelSelectInput(keyCode);
                break;

            case GameState.HighScores:
                if (keyCode == -7 || keyCode == -5 || keyCode == 48) // Back / Select
                {
                    State = GameState.MainMenu;
                }
                else if (keyCode == -6) // Reset scores
                {
                    _saveManager.ResetHighScores();
                }
                break;

            case GameState.Options:
                HandleOptionsInput(keyCode);
                break;

            case GameState.Rebind:
                HandleRebindInput(keyCode);
                break;

            case GameState.Help:
                if (keyCode == -7 || keyCode == -5 || keyCode == 48)
                {
                    State = GameState.MainMenu;
                }
                break;

            case GameState.InGame:
                if (keyCode == -7) // Pause
                {
                    State = GameState.Paused;
                    _menuIndex = 0;
                    _audio.Stop();
                }
                else
                {
                    ActiveGame?.void_a(keyCode);
                }
                break;

            case GameState.Paused:
                HandlePauseInput(keyCode);
                break;

            case GameState.NameEntry:
                HandleNameEntryInput(keyCode);
                break;

            case GameState.GameOver:
            case GameState.Victory:
                if (keyCode == -7 || keyCode == -5 || keyCode == 48)
                {
                    State = GameState.MainMenu;
                    _menuIndex = 0;
                }
                break;
        }
    }

    public void OnKeyUp(int keyCode)
    {
        if (State == GameState.InGame)
        {
            ActiveGame?.b(keyCode);
        }
    }

    private void HandleMainMenuInput(int key)
    {
        const int MenuCount = 6;
        if (key == -1) // UP
        {
            _menuIndex = (_menuIndex - 1 + MenuCount) % MenuCount;
        }
        else if (key == -2) // DOWN
        {
            _menuIndex = (_menuIndex + 1) % MenuCount;
        }
        else if (key == -5 || key == -6 || key == 53) // FIRE / ENTER
        {
            switch (_menuIndex)
            {
                case 0: // Story Mode
                    _isSurvivalMode = false;
                    State = GameState.CharacterSelect;
                    break;
                case 1: // Survival Mode
                    _isSurvivalMode = true;
                    State = GameState.CharacterSelect;
                    break;
                case 2: // High Scores
                    State = GameState.HighScores;
                    break;
                case 3: // Options
                    State = GameState.Options;
                    _subIndex = 0;
                    break;
                case 4: // Help / Controls
                    State = GameState.Help;
                    break;
                case 5: // Exit
                    Environment.Exit(0);
                    break;
            }
        }
        else if (key == -7) // Back (from Main Menu: Exit)
        {
            Environment.Exit(0);
        }
    }

    private void HandleCharacterSelectInput(int key)
    {
        if (key == -3) // LEFT
        {
            _selectedCharacter = (_selectedCharacter - 1 + 5) % 5;
            _saveManager.Data.SelectedCharacter = _selectedCharacter;
            _saveManager.Save();
        }
        else if (key == -4) // RIGHT
        {
            _selectedCharacter = (_selectedCharacter + 1) % 5;
            _saveManager.Data.SelectedCharacter = _selectedCharacter;
            _saveManager.Save();
        }
        else if (key == -5 || key == -6 || key == 53) // SELECT
        {
            if (_isSurvivalMode)
            {
                StartGame(CoreGame.D, _selectedCharacter, true);
            }
            else
            {
                State = GameState.LevelSelect;
                _selectedLevel = 1;
            }
        }
        else if (key == -7) // BACK
        {
            State = GameState.MainMenu;
        }
    }

    private void HandleLevelSelectInput(int key)
    {
        int maxLvl = Math.Min(12, Math.Max(1, _saveManager.Data.HighestUnlockedLevel));
        if (key == -3 || key == -1) // LEFT/UP
        {
            _selectedLevel = Math.Max(1, _selectedLevel - 1);
        }
        else if (key == -4 || key == -2) // RIGHT/DOWN
        {
            _selectedLevel = Math.Min(maxLvl, _selectedLevel + 1);
        }
        else if (key == -5 || key == -6 || key == 53) // START LEVEL
        {
            StartGame(_selectedLevel, _selectedCharacter, false);
        }
        else if (key == -7) // BACK
        {
            State = GameState.CharacterSelect;
        }
    }

    private void HandleOptionsInput(int key)
    {
        const int OptionsCount = 11;
        if (key == -1) // UP
        {
            _subIndex = (_subIndex - 1 + OptionsCount) % OptionsCount;
        }
        else if (key == -2) // DOWN
        {
            _subIndex = (_subIndex + 1) % OptionsCount;
        }
        else if (key == -3 || key == -4 || key == -5 || key == -6) // LEFT/RIGHT/FIRE
        {
            int dir = key == -3 ? -1 : 1;
            switch (_subIndex)
            {
                case 0: // Sound Toggle
                    _saveManager.Data.SoundEnabled = !_saveManager.Data.SoundEnabled;
                    _audio.IsMuted = !_saveManager.Data.SoundEnabled;
                    PR.Core.Audio.SoundEngine.Enabled = _saveManager.Data.SoundEnabled;
                    if (Raylib.IsAudioDeviceReady())
                    {
                        Raylib.SetMasterVolume(_saveManager.Data.SoundEnabled ? _saveManager.Data.Volume / 1000f : 0f);
                    }
                    if (_saveManager.Data.SoundEnabled) PlayMenuMusic();
                    else _audio.Stop();
                    _saveManager.Save();
                    break;

                case 1: // Volume
                    _saveManager.Data.Volume = Math.Clamp(_saveManager.Data.Volume + dir * 100, 0, 1000);
                    _audio.Volume = _saveManager.Data.Volume;
                    if (Raylib.IsAudioDeviceReady() && _saveManager.Data.SoundEnabled)
                    {
                        Raylib.SetMasterVolume(_saveManager.Data.Volume / 1000f);
                    }
                    _saveManager.Save();
                    break;

                case 2: // Language (0=de, 1=en, 2=fr, 3=it)
                    int nextLang = (_saveManager.Data.LanguageIndex + dir + 4) % 4;
                    _saveManager.Data.LanguageIndex = nextLang;
                    _app.LoadLanguage(nextLang);
                    _saveManager.Save();
                    break;

                case 3: // Frame Rate (Smooth 60 / Original 14)
                    _saveManager.Data.Smooth60Fps = !_saveManager.Data.Smooth60Fps;
                    PR.Core.EngineTweaks.Smooth60Fps = _saveManager.Data.Smooth60Fps;
                    _saveManager.Save();
                    break;

                case 4: // Screen Mode (Widescreen 16:9 / Original 4:3)
                    _saveManager.Data.Widescreen = !_saveManager.Data.Widescreen;
                    PR.Core.EngineTweaks.Widescreen = _saveManager.Data.Widescreen;
                    _onWidescreenToggle();
                    _saveManager.Save();
                    break;

                case 5: // Scaling (Integer vs Fit)
                    _view.FitScaling = !_view.FitScaling;
                    _saveManager.Data.FitScaling = _view.FitScaling;
                    _saveManager.Save();
                    break;

                case 6: // Window Scale
                    int nextScale = Math.Clamp(_saveManager.Data.Scale + dir, 1, 6);
                    _saveManager.Data.Scale = nextScale;
                    _onScaleChange(nextScale);
                    _saveManager.Save();
                    break;

                case 7: // Fullscreen Toggle
                    _onFullscreenToggle();
                    _saveManager.Data.Fullscreen = _view.IsFullscreen;
                    _saveManager.Save();
                    break;

                case 8: // Controls / Rebind
                    State = GameState.Rebind;
                    _subIndex = 0;
                    break;

                case 9: // Reset to Defaults
                    ResetAllToDefaults();
                    break;

                case 10: // Back
                    State = _returnToPause ? GameState.Paused : GameState.MainMenu;
                    break;
            }
        }
        else if (key == -7) // BACK (ESC)
        {
            State = _returnToPause ? GameState.Paused : GameState.MainMenu;
        }
    }

    private void HandleRebindInput(int key)
    {
        if (_rebindingAction >= 0) return;

        const int RebindCount = 10;
        if (key == -1) // UP
        {
            _subIndex = (_subIndex - 1 + RebindCount) % RebindCount;
        }
        else if (key == -2) // DOWN
        {
            _subIndex = (_subIndex + 1) % RebindCount;
        }
        else if (key == -5 || key == -6) // FIRE / ENTER
        {
            if (_subIndex < 8)
            {
                _rebindingAction = _subIndex;
            }
            else if (_subIndex == 8) // Reset controls
            {
                _inputRouter.ResetToDefaults();
            }
            else if (_subIndex == 9) // Back
            {
                State = _returnToPause ? GameState.Paused : GameState.Options;
            }
        }
        else if (key == -7) // BACK (ESC)
        {
            State = _returnToPause ? GameState.Paused : GameState.Options;
        }
    }

    public void CheckRebindingInput()
    {
        if (_rebindingAction < 0) return;

        // Cancel on Escape
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            _rebindingAction = -1;
            return;
        }

        // Check Keyboard
        int pressed = (int)Raylib.GetKeyPressed();
        if (pressed > 0)
        {
            _inputRouter.Bindings.SetKeyboardBinding(_rebindingAction, (KeyboardKey)pressed);
            _inputRouter.SaveBindings();
            _rebindingAction = -1;
            return;
        }

        // Check Gamepad
        if (Raylib.IsGamepadAvailable(0))
        {
            foreach (GamepadButton btn in Enum.GetValues<GamepadButton>())
            {
                if (Raylib.IsGamepadButtonPressed(0, btn))
                {
                    _inputRouter.Bindings.SetGamepadBinding(_rebindingAction, btn);
                    _inputRouter.SaveBindings();
                    _rebindingAction = -1;
                    return;
                }
            }
        }
    }

    public void ResetAllToDefaults()
    {
        _saveManager.ResetToDefaults();
        _inputRouter.ResetToDefaults();
        _audio.IsMuted = false;
        _audio.Volume = 800;
        if (Raylib.IsAudioDeviceReady())
        {
            Raylib.SetMasterVolume(0.8f);
        }
        PR.Core.Audio.SoundEngine.Enabled = true;
        _app.LoadLanguage(1);
        PR.Core.EngineTweaks.Smooth60Fps = true;
        PR.Core.EngineTweaks.Widescreen = true;
        _view.FitScaling = false;
        _view.SetScale(3);
        if (_view.IsFullscreen)
        {
            _onFullscreenToggle();
        }
        _onWidescreenToggle();
        _saveManager.Save();
        PlayMenuMusic();
    }

    private void HandlePauseInput(int key)
    {
        const int PauseCount = 4;
        if (key == -1) _menuIndex = (_menuIndex - 1 + PauseCount) % PauseCount;
        else if (key == -2) _menuIndex = (_menuIndex + 1) % PauseCount;
        else if (key == -5 || key == -6)
        {
            switch (_menuIndex)
            {
                case 0: // Resume
                    State = GameState.InGame;
                    if (_saveManager.Data.SoundEnabled) PlayGameMusic();
                    break;
                case 1: // Settings
                    _returnToPause = true;
                    State = GameState.Options;
                    _subIndex = 0;
                    break;
                case 2: // Rebind Controls
                    _returnToPause = true;
                    State = GameState.Rebind;
                    _subIndex = 0;
                    break;
                case 3: // Quit to Title
                    State = GameState.MainMenu;
                    ActiveGame = null;
                    PlayMenuMusic();
                    break;
            }
        }
        else if (key == -7) // Resume shortcut
        {
            State = GameState.InGame;
            if (_saveManager.Data.SoundEnabled) PlayGameMusic();
        }
    }

    private void HandleNameEntryInput(int key)
    {
        if (key == -1) // Next char
        {
            char c = _nameInput[_nameCursor];
            c = c >= 'Z' ? 'A' : (char)(c + 1);
            _nameInput[_nameCursor] = c;
        }
        else if (key == -2) // Prev char
        {
            char c = _nameInput[_nameCursor];
            c = c <= 'A' ? 'Z' : (char)(c - 1);
            _nameInput[_nameCursor] = c;
        }
        else if (key == -4 || key == -5) // Advance cursor / submit
        {
            if (_nameCursor < 3)
            {
                _nameCursor++;
            }
            else
            {
                string name = new string(_nameInput).Trim();
                _saveManager.InsertHighScore(_lastFinalScore, name, _isSurvivalMode);
                State = GameState.HighScores;
            }
        }
        else if (key == -3) // Cursor back
        {
            if (_nameCursor > 0) _nameCursor--;
        }
    }

    public void StartGame(int level, int character, bool survival)
    {
        CoreGame.G = level;
        CoreGame.F = character;
        CoreGame.var_boolean_e = survival;
        CoreGame.var_java_util_Random_a = new JavaRandom(Environment.TickCount64);

        ActiveGame = new CoreGame(_app);
        CoreGame.var_f_a = ActiveGame;
        _app.var_f_a = ActiveGame;

        ActiveGame.e();
        State = GameState.InGame;

        if (_saveManager.Data.SoundEnabled)
        {
            PlayGameMusic();
        }
    }

    // =========================================================================
    // AUTHENTIC OG J2ME RENDERING ENGINE
    // =========================================================================

    public void Render(Graphics gr)
    {
        if (State == GameState.InGame)
        {
            ActiveGame?.d();
            return;
        }

        int screenW = PR.Core.EngineTweaks.RenderWidth;
        int screenH = PR.Core.EngineTweaks.RenderHeight;

        gr.Translate(-gr.GetTranslateX(), -gr.GetTranslateY());
        gr.SetClip(0, 0, screenW, screenH);

        switch (State)
        {
            case GameState.Title:
                RenderTitleScreen(gr, screenW, screenH);
                break;
            case GameState.MainMenu:
                RenderMainMenu(gr, screenW, screenH);
                break;
            case GameState.CharacterSelect:
                RenderCharacterSelect(gr, screenW, screenH);
                break;
            case GameState.LevelSelect:
                RenderLevelSelect(gr, screenW, screenH);
                break;
            case GameState.HighScores:
                RenderHighScores(gr, screenW, screenH);
                break;
            case GameState.Options:
                RenderOptions(gr, screenW, screenH);
                break;
            case GameState.Rebind:
                RenderRebind(gr, screenW, screenH);
                break;
            case GameState.Help:
                RenderHelp(gr, screenW, screenH);
                break;
            case GameState.Paused:
                ActiveGame?.d();
                RenderPauseOverlay(gr, screenW, screenH);
                break;
            case GameState.NameEntry:
                RenderNameEntry(gr, screenW, screenH);
                break;
            case GameState.GameOver:
            case GameState.Victory:
                RenderGameOver(gr, screenW, screenH);
                break;
        }
    }

    /// <summary>
    /// Renders the authentic Living Mobile frame (crimson wallpaper pattern tile Image[28],
    /// vertical column borders Image[9] & Image[8], top frame Image[6], and bottom softkey frame Image[7]).
    /// </summary>
    private static void DrawOGMenuFrame(Graphics gr, int renderW, int renderH, string? leftSoftkey = null, string? rightSoftkey = null, bool drawWatermark = true)
    {
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        if (images == null)
        {
            gr.SetColor(0x180004);
            gr.FillRect(0, 0, renderW, renderH);
            return;
        }

        CoreImage? bgTile = images.Length > 28 ? images[28] : null;
        CoreImage? topBorder = images.Length > 6 ? images[6] : null;
        CoreImage? bottomBorder = images.Length > 7 ? images[7] : null;
        CoreImage? rightCol = images.Length > 8 ? images[8] : null;
        CoreImage? leftCol = images.Length > 9 ? images[9] : null;
        CoreImage? watermark = images.Length > 27 ? images[27] : null;

        // 1. Tile wallpaper across the entire virtual buffer
        if (bgTile != null)
        {
            int tileW = bgTile.Width;
            int tileH = bgTile.Height;
            for (int y = 0; y < renderH; y += tileH)
            {
                for (int x = 0; x < renderW; x += tileW)
                {
                    gr.DrawImage(bgTile, x, y, 20); // 20 = LEFT | TOP
                }
            }
        }
        else
        {
            gr.SetColor(0x180004);
            gr.FillRect(0, 0, renderW, renderH);
        }

        // 2. Character art watermark emblem in bottom right (as in original g.b())
        if (drawWatermark && watermark != null)
        {
            int colW = rightCol?.Width ?? 9;
            gr.DrawImage(watermark, renderW - colW, renderH - 10, 40); // 40 = RIGHT | BOTTOM
        }

        // 3. Left and right ornamental columns tiled vertically
        if (leftCol != null)
        {
            for (int y = 0; y < renderH; y += leftCol.Height)
            {
                gr.DrawImage(leftCol, 0, y, 20); // 20 = LEFT | TOP
            }
        }
        if (rightCol != null)
        {
            for (int y = 0; y < renderH; y += rightCol.Height)
            {
                gr.DrawImage(rightCol, renderW, y, 24); // 24 = RIGHT | TOP
            }
        }

        // 4. Top ornamental frame (240x16)
        if (topBorder != null)
        {
            for (int x = 0; x < renderW; x += topBorder.Width)
            {
                gr.DrawImage(topBorder, x, 0, 20);
            }
        }

        // 5. Bottom softkey bar frame (240x25)
        if (bottomBorder != null)
        {
            for (int x = 0; x < renderW; x += bottomBorder.Width)
            {
                gr.DrawImage(bottomBorder, x, renderH, 36); // 36 = LEFT | BOTTOM
            }
        }

        // 6. Draw softkey labels in Font 1 (authentic white font)
        if (!string.IsNullOrEmpty(leftSoftkey))
        {
            GMIDlet.a(1, leftSoftkey, 4, renderH - 4, 36);
        }
        if (!string.IsNullOrEmpty(rightSoftkey))
        {
            GMIDlet.a(1, rightSoftkey, renderW - 4, renderH - 4, 40);
        }
    }

    /// <summary>
    /// Authentic horizontal dividing line from original g.d(int y).
    /// </summary>
    private static void DrawDivider(Graphics gr, int y, int renderW)
    {
        int margin = 18;
        int x1 = margin;
        int x2 = renderW - margin;
        gr.SetColor(0xFFFFFF);
        gr.DrawLine(x1, y, x2, y);
        gr.DrawLine(x1, y + 3, x2, y + 3);
        gr.SetColor(0x498178); // Living Mobile signature divider color
        gr.DrawLine(x1, y + 1, x2, y + 1);
        gr.DrawLine(x1, y + 2, x2, y + 2);
    }

    /// <summary>
    /// Draws the authentic 12x12 arrow sprite from Image[3].
    /// In the Living Mobile sprite sheet, 2 columns x 1 row represents:
    /// Column 0 = Left arrow, Column 1 = Right arrow.
    /// </summary>
    private static void DrawArrow(Graphics gr, int x, int y, bool isRight)
    {
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        if (images != null && images.Length > 3 && images[3] != null)
        {
            GMIDlet.a(images[3]!, x, y, 2, 1, isRight ? 1 : 0, 0);
        }
    }

    /// <summary>
    /// Authentic subtle selection highlight box.
    /// </summary>
    private static void DrawSelectionHighlight(Graphics gr, int x, int y, int w, int h)
    {
        gr.SetColor(0x501010);
        gr.FillRect(x, y, w, h);
        gr.SetColor(0xFFD700);
        gr.DrawRect(x, y, w - 1, h - 1);
    }

    private void RenderTitleScreen(Graphics gr, int screenW, int screenH)
    {
        // 1. Black backdrop
        gr.SetColor(0x000000);
        gr.FillRect(0, 0, screenW, screenH);

        int centerX = screenW >> 1;
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;

        // 2. Full Ranger artwork Image[1] (240x210) centered at (centerX, (screenH >> 1) + 30)
        CoreImage? titleArt = (images != null && images.Length > 1) ? images[1] : null;
        if (titleArt != null)
        {
            gr.DrawImage(titleArt, centerX, (screenH >> 1) + 30, 3); // 3 = HCENTER | VCENTER
        }

        // 3. Top "Power Rangers" Lightning Logo Image[0] (205x112)
        CoreImage? logoTop = (images != null && images.Length > 0) ? images[0] : null;
        if (logoTop != null)
        {
            gr.DrawImage(logoTop, centerX, 0, 17); // 17 = HCENTER | TOP
        }
        else
        {
            GMIDlet.a(3, "POWER RANGERS", centerX, 30, 17);
        }

        // 4. "Mystic Force" Subtitle Banner Image[5] (166x25)
        CoreImage? logoSub = (images != null && images.Length > 5) ? images[5] : null;
        if (logoSub != null)
        {
            gr.DrawImage(logoSub, centerX, 94, 17);
        }
        else
        {
            GMIDlet.a(3, "MYSTIC FORCE", centerX, 94, 17);
        }

        // 5. Blinking "PRESS ANY KEY TO START" in Font 0
        if ((_animTick % 30) < 20)
        {
            GMIDlet.a(0, "PRESS ANY KEY TO START", centerX, 270, 17);
        }

        // 6. Copyright line
        GMIDlet.a(0, "(C) 2006 DISNEY / LIVING MOBILE", centerX, 290, 17);

        // 7. Softkeys
        CoreImage? bottomBorder = (images != null && images.Length > 7) ? images[7] : null;
        if (bottomBorder != null)
        {
            for (int x = 0; x < screenW; x += bottomBorder.Width)
            {
                gr.DrawImage(bottomBorder, x, screenH, 36);
            }
        }
        GMIDlet.a(1, "MENU", 4, screenH - 4, 36);
        GMIDlet.a(1, "EXIT", screenW - 4, screenH - 4, 40);
    }

    private void RenderMainMenu(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "SELECT", "EXIT", drawWatermark: true);
        int centerX = screenW >> 1;

        // Golden Shield Header Emblem Image[10] (93x106)
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        CoreImage? shield = (images != null && images.Length > 10) ? images[10] : null;
        if (shield != null)
        {
            gr.DrawImage(shield, centerX, 15, 17);
        }
        else
        {
            GMIDlet.a(3, "MAIN MENU", centerX, 20, 17);
            DrawDivider(gr, 40, screenW);
        }

        string[] items = {
            "STORY MODE",
            "SURVIVAL MODE",
            "HIGH SCORES",
            "OPTIONS",
            "CONTROLS & HELP",
            "EXIT"
        };

        int startY = 135;
        const int itemH = 25;
        for (int i = 0; i < items.Length; i++)
        {
            int y = startY + i * itemH;
            bool sel = i == _menuIndex;
            if (sel)
            {
                DrawSelectionHighlight(gr, centerX - 85, y - 2, 170, 20);
                DrawArrow(gr, centerX - 94, y + 2, isRight: false);
                DrawArrow(gr, centerX + 88, y + 2, isRight: true);
                GMIDlet.a(3, items[i], centerX, y, 17);
            }
            else
            {
                GMIDlet.a(1, items[i], centerX, y, 17);
            }
        }
    }

    private string GetRangerDescription(int rangerIdx)
    {
        try
        {
            if (App.var_java_lang_String_arr_arr_a != null &&
                App.var_java_lang_String_arr_arr_a.Length > 0 &&
                App.var_java_lang_String_arr_arr_a[0] != null)
            {
                int descIdx = rangerIdx switch
                {
                    0 => 18, // Nick
                    1 => 21, // Chip
                    2 => 20, // Madison
                    3 => 22, // Vida
                    4 => 19, // Xander
                    _ => 18
                };
                if (descIdx < App.var_java_lang_String_arr_arr_a[0].Length)
                {
                    return App.var_java_lang_String_arr_arr_a[0][descIdx];
                }
            }
        }
        catch { }

        return RangerDescriptionsFallback[Math.Clamp(rangerIdx, 0, 4)];
    }

    private void RenderCharacterSelect(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "SELECT", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        // Header
        GMIDlet.a(3, "CHOOSE RANGER", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        // Ranger Name with selector arrows
        string rangerName = RangerNames[_selectedCharacter];
        int nameW = GMIDlet.a(3, rangerName);
        GMIDlet.a(3, rangerName, centerX, 36, 17);
        DrawArrow(gr, centerX - (nameW / 2) - 14, 38, isRight: false);
        DrawArrow(gr, centerX + (nameW / 2) + 8, 38, isRight: true);
        GMIDlet.a(0, RangerElements[_selectedCharacter], centerX, 54, 17);
        DrawDivider(gr, 68, screenW);

        // Word-wrapped Ranger description
        string desc = GetRangerDescription(_selectedCharacter);
        string[] descLines = GMIDlet.WordWrap(desc, 195, 1);
        for (int i = 0; i < descLines.Length; i++)
        {
            GMIDlet.a(1, descLines[i], centerX, 76 + i * 16, 17);
        }

        DrawDivider(gr, 126, screenW);

        // Character Sprite Preview
        int previewX = centerX - 55;
        int previewY = 240;
        if (_characterPreviews[_selectedCharacter] == null)
        {
            try
            {
                var preview = App.a_a(0);
                preview.w = _selectedCharacter;
                preview.d(_selectedCharacter);
                preview.var_int_d = previewX;
                preview.var_int_e = previewY;
                preview.a(1, false);
                preview.InterpState.Snap(previewX, previewY);
                preview.RenderX = previewX;
                preview.RenderY = previewY;
                _characterPreviews[_selectedCharacter] = preview;
            }
            catch { }
        }

        var actor = _characterPreviews[_selectedCharacter];
        if (actor != null)
        {
            actor.var_int_d = previewX;
            actor.var_int_e = previewY;
            actor.RenderX = previewX;
            actor.RenderY = previewY;
            actor.a(0, 0);
        }

        // Stat Bars on the right
        int statX = centerX + 2;
        int barW = 55;

        GMIDlet.a(0, "HEALTH:", statX, 145, 20);
        RenderStatBar(gr, statX + 46, 148, barW, (_selectedCharacter == 4 ? 100 : 75 + _selectedCharacter * 5));

        GMIDlet.a(0, "MAGIC:", statX, 175, 20);
        RenderStatBar(gr, statX + 46, 178, barW, (_selectedCharacter == 2 ? 100 : 60 + (4 - _selectedCharacter) * 8));

        GMIDlet.a(0, "SPEED:", statX, 205, 20);
        RenderStatBar(gr, statX + 46, 208, barW, (_selectedCharacter == 1 || _selectedCharacter == 3 ? 100 : 70));

        DrawDivider(gr, 272, screenW);
    }

    private static void RenderStatBar(Graphics gr, int x, int y, int maxW, int percent)
    {
        int curW = maxW * percent / 100;
        gr.SetColor(0x300000);
        gr.FillRect(x, y, maxW, 8);
        gr.SetColor(0x00FF88);
        gr.FillRect(x, y, curW, 8);
        gr.SetColor(0xFFD700);
        gr.DrawRect(x, y, maxW - 1, 7);
    }

    private void RenderLevelSelect(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "START", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        int maxLvl = Math.Min(12, Math.Max(1, _saveManager.Data.HighestUnlockedLevel));

        GMIDlet.a(3, "SELECT LEVEL", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        string lvlStr = _selectedLevel == 13 ? "SURVIVAL ARENA" : $"STAGE {_selectedLevel} OF 12";
        int lvlW = GMIDlet.a(3, lvlStr);
        GMIDlet.a(3, lvlStr, centerX, 38, 17);
        DrawArrow(gr, centerX - (lvlW / 2) - 14, 40, isRight: false);
        DrawArrow(gr, centerX + (lvlW / 2) + 8, 40, isRight: true);

        // Stage details box
        gr.SetColor(0x280808);
        gr.FillRect(centerX - 95, 62, 190, 75);
        gr.SetColor(0xFFD700);
        gr.DrawRect(centerX - 95, 62, 189, 74);

        string stageName = _selectedLevel <= 13 ? StageNames[_selectedLevel - 1] : "MYSTIC REALM";
        GMIDlet.a(3, stageName, centerX, 74, 17);
        GMIDlet.a(1, _selectedLevel == 13 ? "DEFEAT ALL WAVES!" : $"MISSION LEVEL {_selectedLevel}", centerX, 94, 17);
        GMIDlet.a(0, $"UNLOCKED: {maxLvl}/12", centerX, 114, 17);

        // World Map Artwork Image[23] if available
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        CoreImage? mapArt = (images != null && images.Length > 23) ? images[23] : null;
        if (mapArt != null)
        {
            gr.DrawImage(mapArt, centerX, 145, 17);

            // Level node highlight ring
            gr.SetColor(0xFFD700);
            int nodeX = centerX - 60 + ((_selectedLevel - 1) % 4) * 40;
            int nodeY = 165 + ((_selectedLevel - 1) / 4) * 28;
            gr.DrawRect(nodeX - 4, nodeY - 4, 8, 8);
        }
        else
        {
            gr.SetColor(0x180004);
            gr.FillRect(centerX - 95, 145, 190, 115);
            gr.SetColor(0xFFD700);
            gr.DrawRect(centerX - 95, 145, 189, 114);
            GMIDlet.a(1, "WORLD MAP", centerX, 185, 17);
            GMIDlet.a(0, "USE ARROWS TO CHOOSE STAGE", centerX, 210, 17);
        }

        DrawDivider(gr, 272, screenW);
    }

    private void RenderHighScores(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "RESET", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        GMIDlet.a(3, "HALL OF FAME", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        // Story Mode High Scores
        GMIDlet.a(3, "-- STORY MODE --", centerX, 36, 17);
        for (int i = 0; i < 5; i++)
        {
            string line = $"{i + 1}.  {_saveManager.Data.StoryHighScoreNames[i],-4}   {_saveManager.Data.StoryHighScores[i],7}";
            GMIDlet.a(1, line, centerX, 56 + i * 18, 17);
        }

        DrawDivider(gr, 154, screenW);

        // Survival Mode High Scores
        GMIDlet.a(3, "-- SURVIVAL MODE --", centerX, 162, 17);
        for (int i = 0; i < 5; i++)
        {
            string line = $"{i + 1}.  {_saveManager.Data.SurvivalHighScoreNames[i],-4}   {_saveManager.Data.SurvivalHighScores[i],7}";
            GMIDlet.a(1, line, centerX, 182 + i * 18, 17);
        }

        DrawDivider(gr, 278, screenW);
    }

    private void RenderOptions(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "CHANGE", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        GMIDlet.a(3, "SETTINGS", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        string[] langNames = { "DEUTSCH", "ENGLISH", "FRANCAIS", "ITALIANO" };
        string curLang = langNames[Math.Clamp(_saveManager.Data.LanguageIndex, 0, 3)];

        string[] items = {
            $"SOUND: {(_saveManager.Data.SoundEnabled ? "ON" : "OFF")}",
            $"VOLUME: {_saveManager.Data.Volume / 10}%",
            $"LANGUAGE: {curLang}",
            $"FRAME RATE: {(_saveManager.Data.Smooth60Fps ? "SMOOTH (60)" : "ORIGINAL (14)")}",
            $"SCREEN: {(_saveManager.Data.Widescreen ? "WIDESCREEN" : "ORIGINAL")}",
            $"SCALING: {(_view.FitScaling ? "FIT" : "INTEGER")}",
            $"WINDOW SCALE: {_saveManager.Data.Scale}X",
            $"FULLSCREEN: {(_view.IsFullscreen ? "YES" : "NO")}",
            "CONTROLS: REBIND",
            "RESET TO DEFAULTS",
            "BACK"
        };

        const int startY = 34;
        const int itemH = 23;
        int boxW = Math.Min(200, screenW - 36);
        int boxX = centerX - (boxW / 2);
        for (int i = 0; i < items.Length; i++)
        {
            int y = startY + i * itemH;
            bool sel = i == _subIndex;
            if (sel)
            {
                DrawSelectionHighlight(gr, boxX, y - 2, boxW, 20);
                if (i < 8) // Options with left/right choices
                {
                    DrawArrow(gr, boxX + 4, y + 2, isRight: false);
                    DrawArrow(gr, boxX + boxW - 10, y + 2, isRight: true);
                }
                GMIDlet.a(3, items[i], centerX, y, 17);
            }
            else
            {
                GMIDlet.a(1, items[i], centerX, y, 17);
            }
        }
    }

    private void RenderRebind(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "SELECT", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        GMIDlet.a(3, "CONTROLS & REBINDING", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        string[] items = new string[10];
        for (int i = 0; i < 8; i++)
        {
            items[i] = $"{_inputRouter.Bindings.GetActionName(i)}: {_inputRouter.Bindings.GetBindingDisplay(i)}";
        }
        items[8] = "RESET TO DEFAULTS";
        items[9] = "BACK";

        const int startY = 36;
        const int itemH = 24;
        int boxW = Math.Min(200, screenW - 36);
        int boxX = centerX - (boxW / 2);
        for (int i = 0; i < items.Length; i++)
        {
            int y = startY + i * itemH;
            bool sel = i == _subIndex;
            if (sel)
            {
                DrawSelectionHighlight(gr, boxX, y - 2, boxW, 20);
                GMIDlet.a(3, items[i], centerX, y, 17);
            }
            else
            {
                GMIDlet.a(1, items[i], centerX, y, 17);
            }
        }

        if (_rebindingAction >= 0)
        {
            // Modal dialog overlay
            gr.SetColor(unchecked((int)0xF0101018));
            gr.FillRect(centerX - 100, 100, 200, 85);
            gr.SetColor(0xFFD700);
            gr.DrawRect(centerX - 100, 100, 199, 84);
            gr.SetColor(0x7D0000);
            gr.DrawRect(centerX - 98, 102, 195, 80);

            GMIDlet.a(3, $"REBIND: {_inputRouter.Bindings.GetActionName(_rebindingAction)}", centerX, 112, 17);
            GMIDlet.a(1, "PRESS ANY KEY OR BUTTON...", centerX, 137, 17);
            GMIDlet.a(0, "ESC TO CANCEL", centerX, 160, 17);
        }
    }

    private static void RenderHelp(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "OK", "BACK", drawWatermark: false);
        int centerX = screenW >> 1;

        GMIDlet.a(3, "CONTROLS & HELP", centerX, 14, 17);
        DrawDivider(gr, 30, screenW);

        string[] lines = {
            "KEYBOARD CONTROLS:",
            "MOVE: ARROWS / WASD / NUMPAD",
            "ATTACK / JUMP: SPACE / ENTER / 5",
            "MAGIC SPELL: L-CTRL / 0",
            "SPECIAL COMBO: ATTACK + MOVE",
            "PAUSE: ESCAPE / P",
            "FULLSCREEN: ALT+ENTER / F11",
            "",
            "GAMEPAD CONTROLS:",
            "D-PAD: MOVE",
            "A: ATTACK  |  B: MAGIC",
            "START: PAUSE"
        };

        for (int i = 0; i < lines.Length; i++)
        {
            bool isHeader = i == 0 || i == 8;
            GMIDlet.a(isHeader ? 3 : 1, lines[i], centerX, 44 + i * 19, 17);
        }

        DrawDivider(gr, 275, screenW);
    }

    private void RenderPauseOverlay(Graphics gr, int screenW, int screenH)
    {
        int centerX = screenW >> 1;

        // Dimmed translucent overlay over frozen gameplay
        gr.SetColor(unchecked((int)0xCC000000));
        gr.FillRect(0, 0, screenW, screenH);

        // Centered authentic crimson dialog frame
        const int boxW = 200;
        const int boxH = 180;
        int boxX = centerX - (boxW / 2);
        const int boxY = 65;

        gr.SetColor(0x380C0C);
        gr.FillRect(boxX, boxY, boxW, boxH);
        gr.SetColor(0xFFD700);
        gr.DrawRect(boxX, boxY, boxW - 1, boxH - 1);
        gr.SetColor(0x7D0000);
        gr.DrawRect(boxX + 2, boxY + 2, boxW - 5, boxH - 5);

        GMIDlet.a(3, "PAUSED", centerX, 76, 17);
        DrawDivider(gr, 96, screenW);

        string[] items = { "RESUME", "SETTINGS", "REBIND CONTROLS", "QUIT TO TITLE" };
        for (int i = 0; i < items.Length; i++)
        {
            int y = 108 + i * 32;
            if (i == _menuIndex)
            {
                DrawSelectionHighlight(gr, boxX + 15, y - 2, boxW - 30, 24);
                DrawArrow(gr, boxX + 5, y + 4, isRight: false);
                DrawArrow(gr, boxX + boxW - 12, y + 4, isRight: true);
                GMIDlet.a(3, items[i], centerX, y + 2, 17);
            }
            else
            {
                GMIDlet.a(1, items[i], centerX, y + 2, 17);
            }
        }

        // Softkeys
        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        CoreImage? bottomBorder = (images != null && images.Length > 7) ? images[7] : null;
        if (bottomBorder != null)
        {
            for (int x = 0; x < screenW; x += bottomBorder.Width)
            {
                gr.DrawImage(bottomBorder, x, screenH, 36);
            }
        }
        GMIDlet.a(1, "SELECT", 4, screenH - 4, 36);
        GMIDlet.a(1, "RESUME", screenW - 4, screenH - 4, 40);
    }

    private void RenderNameEntry(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "NEXT", "SUBMIT", drawWatermark: false);
        int centerX = screenW >> 1;

        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        CoreImage? shield = (images != null && images.Length > 10) ? images[10] : null;
        if (shield != null)
        {
            gr.DrawImage(shield, centerX, 15, 17);
        }

        GMIDlet.a(3, "NEW HIGH SCORE!", centerX, 125, 17);
        GMIDlet.a(1, $"SCORE: {_lastFinalScore}", centerX, 145, 17);

        GMIDlet.a(0, "ENTER YOUR INITIALS:", centerX, 172, 17);

        int startX = centerX - 36;
        for (int i = 0; i < 4; i++)
        {
            bool cur = i == _nameCursor;
            gr.SetColor(cur ? 0xFFD700 : 0x555555);
            gr.FillRect(startX + i * 18, 195, 14, 22);

            gr.SetColor(0x000000);
            gr.FillRect(startX + i * 18 + 1, 196, 12, 20);

            GMIDlet.a(cur ? 3 : 1, _nameInput[i].ToString(), startX + i * 18 + 7, 198, 17);
        }

        GMIDlet.a(0, "UP/DOWN: CHANGE LETTER", centerX, 235, 17);
        GMIDlet.a(0, "ENTER: NEXT / SUBMIT", centerX, 255, 17);
    }

    private void RenderGameOver(Graphics gr, int screenW, int screenH)
    {
        DrawOGMenuFrame(gr, screenW, screenH, "SELECT", "EXIT", drawWatermark: true);
        int centerX = screenW >> 1;

        var images = GMIDlet.var_javax_microedition_lcdui_Image_arr_a;
        CoreImage? shield = (images != null && images.Length > 10) ? images[10] : null;
        if (shield != null)
        {
            gr.DrawImage(shield, centerX, 15, 17);
        }

        GMIDlet.a(3, State == GameState.Victory ? "CONGRATULATIONS!" : "GAME OVER", centerX, 130, 17);
        GMIDlet.a(1, $"FINAL SCORE: {_lastFinalScore}", centerX, 160, 17);

        if ((_animTick % 30) < 20)
        {
            GMIDlet.a(0, "PRESS ENTER TO CONTINUE", centerX, 220, 17);
        }
    }

    public void ProcessMouse(Vector2 mouseVirtPos, bool leftClick)
    {
        if (mouseVirtPos.X < 0 || mouseVirtPos.Y < 0) return;

        int screenW = PR.Core.EngineTweaks.RenderWidth;
        int screenH = PR.Core.EngineTweaks.RenderHeight;
        int centerX = screenW >> 1;
        float localX = mouseVirtPos.X;
        float localY = mouseVirtPos.Y;

        // Check Bottom Softkey Bar clicks
        if (localY >= screenH - 30)
        {
            if (leftClick)
            {
                if (localX < 80) // Left Softkey clicked
                {
                    OnKeyDown(-5);
                    return;
                }
                else if (localX > screenW - 80) // Right Softkey clicked
                {
                    OnKeyDown(-7);
                    return;
                }
            }
        }

        switch (State)
        {
            case GameState.Title:
                if (leftClick)
                {
                    State = GameState.MainMenu;
                    _menuIndex = 0;
                }
                break;

            case GameState.MainMenu:
                if (localX >= centerX - 90 && localX <= centerX + 90)
                {
                    const int startY = 135;
                    const int itemH = 25;
                    if (localY >= startY && localY < startY + 6 * itemH)
                    {
                        int idx = (int)((localY - startY) / itemH);
                        if (idx >= 0 && idx < 6)
                        {
                            _menuIndex = idx;
                            if (leftClick) HandleMainMenuInput(-5);
                        }
                    }
                }
                break;

            case GameState.CharacterSelect:
                if (leftClick)
                {
                    if (localX < centerX - 70 && localY >= 30 && localY <= 65) HandleCharacterSelectInput(-3);
                    else if (localX > centerX + 70 && localY >= 30 && localY <= 65) HandleCharacterSelectInput(-4);
                    else if (localY >= 270) HandleCharacterSelectInput(-5);
                }
                break;

            case GameState.LevelSelect:
                if (leftClick)
                {
                    if (localX < centerX - 70 && localY >= 30 && localY <= 65) HandleLevelSelectInput(-3);
                    else if (localX > centerX + 70 && localY >= 30 && localY <= 65) HandleLevelSelectInput(-4);
                    else if (localY >= 270) HandleLevelSelectInput(-5);
                }
                break;

            case GameState.Options:
                if (localX >= centerX - 110 && localX <= centerX + 110)
                {
                    const int startY = 34;
                    const int itemH = 23;
                    if (localY >= startY && localY < startY + 11 * itemH)
                    {
                        int idx = (int)((localY - startY) / itemH);
                        if (idx >= 0 && idx < 11)
                        {
                            _subIndex = idx;
                            if (leftClick)
                            {
                                if (idx == 8)
                                {
                                    State = GameState.Rebind;
                                    _subIndex = 0;
                                }
                                else if (idx == 9)
                                {
                                    ResetAllToDefaults();
                                }
                                else if (idx == 10)
                                {
                                    State = _returnToPause ? GameState.Paused : GameState.MainMenu;
                                }
                                else
                                {
                                    int dir = localX < centerX ? -3 : -4;
                                    HandleOptionsInput(dir);
                                }
                            }
                        }
                    }
                }
                break;

            case GameState.Rebind:
                if (_rebindingAction >= 0) return;
                if (localX >= centerX - 110 && localX <= centerX + 110)
                {
                    const int startY = 36;
                    const int itemH = 24;
                    if (localY >= startY && localY < startY + 10 * itemH)
                    {
                        int idx = (int)((localY - startY) / itemH);
                        if (idx >= 0 && idx < 10)
                        {
                            _subIndex = idx;
                            if (leftClick)
                            {
                                if (idx < 8)
                                {
                                    _rebindingAction = idx;
                                }
                                else if (idx == 8)
                                {
                                    _inputRouter.ResetToDefaults();
                                }
                                else if (idx == 9)
                                {
                                    State = _returnToPause ? GameState.Paused : GameState.Options;
                                }
                            }
                        }
                    }
                }
                break;

            case GameState.Paused:
                if (localX >= centerX - 90 && localX <= centerX + 90)
                {
                    const int startY = 108;
                    const int itemH = 32;
                    if (localY >= startY && localY < startY + 4 * itemH)
                    {
                        int idx = (int)((localY - startY) / itemH);
                        if (idx >= 0 && idx < 4)
                        {
                            _menuIndex = idx;
                            if (leftClick) HandlePauseInput(-5);
                        }
                    }
                }
                break;

            case GameState.HighScores:
            case GameState.Help:
            case GameState.GameOver:
            case GameState.Victory:
                if (leftClick && localY < screenH - 30)
                {
                    State = GameState.MainMenu;
                    _menuIndex = 0;
                }
                break;
        }
    }
}
