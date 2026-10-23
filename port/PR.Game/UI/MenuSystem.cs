using System;
using System.IO;
using PR.Core;
using PR.Core.Graphics;
using PR.Game.Audio;
using PR.Game.Saves;
using CoreGame = PR.Core.Game;

namespace PR.Game.UI;

public enum GameState
{
    Title,
    MainMenu,
    CharacterSelect,
    LevelSelect,
    HighScores,
    Options,
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
    private readonly Action<int> _onScaleChange;
    private readonly Action _onFullscreenToggle;

    public GameState State { get; private set; } = GameState.Title;
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

    private static readonly string[] RangerNames = { "NICK (RED)", "CHIP (YELLOW)", "MADISON (BLUE)", "VIDA (PINK)", "XANDER (GREEN)" };
    private static readonly string[] RangerElements = { "FIRE / BALANCED", "LIGHTNING / SPEED", "WATER / MAGIC", "WIND / AGILITY", "EARTH / STRENGTH" };

    private readonly Action _onWidescreenToggle;

    public MenuSystem(App app, SaveManager saveManager, MidiPlayer audio, Action<int> onScaleChange, Action onFullscreenToggle, Action onWidescreenToggle)
    {
        _app = app;
        _saveManager = saveManager;
        _audio = audio;
        _onScaleChange = onScaleChange;
        _onFullscreenToggle = onFullscreenToggle;
        _onWidescreenToggle = onWidescreenToggle;

        _selectedCharacter = Math.Clamp(_saveManager.Data.SelectedCharacter, 0, 4);
        _audio.IsMuted = !_saveManager.Data.SoundEnabled;
        _audio.Volume = _saveManager.Data.Volume;
        PR.Core.EngineTweaks.Smooth60Fps = _saveManager.Data.Smooth60Fps;
        PR.Core.EngineTweaks.Widescreen = _saveManager.Data.Widescreen;

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
        const int OptionsCount = 8;
        if (key == -1) // UP
        {
            _subIndex = (_subIndex - 1 + OptionsCount) % OptionsCount;
        }
        else if (key == -2) // DOWN
        {
            _subIndex = (_subIndex + 1) % OptionsCount;
        }
        else if (key == -3 || key == -4 || key == -5) // LEFT/RIGHT/FIRE
        {
            int dir = key == -3 ? -1 : 1;
            switch (_subIndex)
            {
                case 0: // Language (0=de, 1=en, 2=fr, 3=it)
                    int nextLang = (_saveManager.Data.LanguageIndex + dir + 4) % 4;
                    _saveManager.Data.LanguageIndex = nextLang;
                    App.var_byte_d = (byte)nextLang;
                    _app.m(); // Reload language text
                    _saveManager.Save();
                    break;

                case 1: // Sound Toggle
                    _saveManager.Data.SoundEnabled = !_saveManager.Data.SoundEnabled;
                    _audio.IsMuted = !_saveManager.Data.SoundEnabled;
                    PR.Core.Audio.SoundEngine.Enabled = _saveManager.Data.SoundEnabled;
                    if (Raylib_cs.Raylib.IsAudioDeviceReady())
                    {
                        Raylib_cs.Raylib.SetMasterVolume(_saveManager.Data.SoundEnabled ? _saveManager.Data.Volume / 1000f : 0f);
                    }
                    if (_saveManager.Data.SoundEnabled) PlayMenuMusic();
                    else _audio.Stop();
                    _saveManager.Save();
                    break;

                case 2: // Volume
                    _saveManager.Data.Volume = Math.Clamp(_saveManager.Data.Volume + dir * 100, 0, 1000);
                    _audio.Volume = _saveManager.Data.Volume;
                    if (Raylib_cs.Raylib.IsAudioDeviceReady() && _saveManager.Data.SoundEnabled)
                    {
                        Raylib_cs.Raylib.SetMasterVolume(_saveManager.Data.Volume / 1000f);
                    }
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

                case 5: // Window Scale
                    int nextScale = Math.Clamp(_saveManager.Data.Scale + dir, 1, 5);
                    _saveManager.Data.Scale = nextScale;
                    _onScaleChange(nextScale);
                    _saveManager.Save();
                    break;

                case 6: // Fullscreen Toggle
                    _onFullscreenToggle();
                    _saveManager.Data.Fullscreen = !_saveManager.Data.Fullscreen;
                    _saveManager.Save();
                    break;

                case 7: // Test Audio
                    _audioTestState = (_audioTestState + dir + 4) % 4;
                    if (_audioTestState == 1) PR.Core.Audio.SoundEngine.PlaySoundDirect(1, true);
                    else if (_audioTestState == 2) PR.Core.Audio.SoundEngine.PlaySoundDirect(2, true);
                    else PR.Core.Audio.SoundEngine.Stop();
                    break;
            }
        }
        else if (key == -7) // BACK
        {
            State = GameState.MainMenu;
        }
    }

    private void HandlePauseInput(int key)
    {
        const int PauseCount = 3;
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
                case 1: // Restart
                    StartGame(CoreGame.G, CoreGame.F, CoreGame.var_boolean_e);
                    break;
                case 2: // Exit to Menu
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

    public void Render(Graphics gr)
    {
        if (State == GameState.InGame)
        {
            ActiveGame?.d();
            return;
        }

        int screenW = PR.Core.EngineTweaks.RenderWidth;
        int screenH = PR.Core.EngineTweaks.RenderHeight;

        // Draw backdrop
        gr.Translate(-gr.GetTranslateX(), -gr.GetTranslateY());
        gr.SetClip(0, 0, screenW, screenH);
        gr.SetColor(0x0C0E14);
        gr.FillRect(0, 0, screenW, screenH);

        // Header decorative bars
        gr.SetColor(0x7D0000);
        gr.FillRect(0, 0, screenW, 24);
        gr.SetColor(0xFFD700);
        gr.DrawLine(0, 24, screenW, 24);
        gr.SetColor(0x7D0000);
        gr.FillRect(0, screenH - 20, screenW, 20);
        gr.SetColor(0xFFD700);
        gr.DrawLine(0, screenH - 20, screenW, screenH - 20);

        int centerOffset = PR.Core.EngineTweaks.Widescreen ? (screenW - 240) / 2 : 0;
        gr.Translate(centerOffset, 0);

        switch (State)
        {
            case GameState.Title:
                RenderTitleScreen(gr);
                break;
            case GameState.MainMenu:
                RenderMainMenu(gr);
                break;
            case GameState.CharacterSelect:
                RenderCharacterSelect(gr);
                break;
            case GameState.LevelSelect:
                RenderLevelSelect(gr);
                break;
            case GameState.HighScores:
                RenderHighScores(gr);
                break;
            case GameState.Options:
                RenderOptions(gr);
                break;
            case GameState.Help:
                RenderHelp(gr);
                break;
            case GameState.Paused:
                ActiveGame?.d();
                RenderPauseOverlay(gr);
                break;
            case GameState.NameEntry:
                RenderNameEntry(gr);
                break;
            case GameState.GameOver:
                RenderGameOver(gr);
                break;
        }

        gr.Translate(-centerOffset, 0);
    }

    private void RenderTitleScreen(Graphics gr)
    {
        GMIDlet.a(1, "POWER RANGERS", 120, 60, 17);
        GMIDlet.a(1, "MYSTIC FORCE", 120, 85, 17);

        // Draw decorative gem
        gr.SetColor(0xFFD700);
        gr.FillRect(110, 120, 20, 20);
        gr.SetColor(0xCC0000);
        gr.FillRect(114, 124, 12, 12);

        if ((_animTick % 30) < 20)
        {
            GMIDlet.a(0, "PRESS ANY KEY TO START", 120, 220, 17);
        }

        GMIDlet.a(0, "(C) 2006 LIVING MOBILE / DISNEY", 120, 290, 17);
    }

    private void RenderMainMenu(Graphics gr)
    {
        GMIDlet.a(1, "MAIN MENU", 120, 6, 17);

        string[] items = {
            "STORY MODE",
            "SURVIVAL MODE",
            "HIGH SCORES",
            "OPTIONS",
            "CONTROLS & HELP",
            "EXIT"
        };

        int startY = 60;
        for (int i = 0; i < items.Length; i++)
        {
            bool sel = i == _menuIndex;
            if (sel)
            {
                gr.SetColor(0x401010);
                gr.FillRect(20, startY + i * 35 - 3, 200, 24);
                gr.SetColor(0xFFD700);
                gr.DrawRect(20, startY + i * 35 - 3, 199, 23);
                GMIDlet.a(1, "> " + items[i] + " <", 120, startY + i * 35, 17);
            }
            else
            {
                GMIDlet.a(0, items[i], 120, startY + i * 35, 17);
            }
        }

        GMIDlet.a(0, "SELECT: ENTER / 5", 6, 304, 20);
    }

    private void RenderCharacterSelect(Graphics gr)
    {
        GMIDlet.a(1, "CHOOSE RANGER", 120, 6, 17);

        GMIDlet.a(1, "< " + RangerNames[_selectedCharacter] + " >", 120, 50, 17);
        GMIDlet.a(0, RangerElements[_selectedCharacter], 120, 75, 17);

        // Stats Box
        gr.SetColor(0x202028);
        gr.FillRect(25, 110, 190, 110);
        gr.SetColor(0xFFD700);
        gr.DrawRect(25, 110, 189, 109);

        GMIDlet.a(0, "HEALTH:", 35, 125, 20);
        RenderStatBar(gr, 95, 128, 100, (_selectedCharacter == 4 ? 100 : 75 + _selectedCharacter * 5));

        GMIDlet.a(0, "MAGIC:", 35, 155, 20);
        RenderStatBar(gr, 95, 158, 100, (_selectedCharacter == 2 ? 100 : 60 + (4 - _selectedCharacter) * 8));

        GMIDlet.a(0, "SPEED:", 35, 185, 20);
        RenderStatBar(gr, 95, 188, 100, (_selectedCharacter == 1 || _selectedCharacter == 3 ? 100 : 70));

        GMIDlet.a(0, "SELECT: ENTER", 6, 304, 20);
        GMIDlet.a(0, "BACK: ESC", 234, 304, 24);
    }

    private static void RenderStatBar(Graphics gr, int x, int y, int maxW, int percent)
    {
        int curW = maxW * percent / 100;
        gr.SetColor(0x400000);
        gr.FillRect(x, y, maxW, 8);
        gr.SetColor(0x00FF88);
        gr.FillRect(x, y, curW, 8);
        gr.SetColor(0xFFFFFF);
        gr.DrawRect(x, y, maxW - 1, 7);
    }

    private void RenderLevelSelect(Graphics gr)
    {
        GMIDlet.a(1, "SELECT LEVEL", 120, 6, 17);

        int maxLvl = Math.Min(12, Math.Max(1, _saveManager.Data.HighestUnlockedLevel));
        GMIDlet.a(1, $"< LEVEL {_selectedLevel} >", 120, 90, 17);

        gr.SetColor(0x202030);
        gr.FillRect(30, 130, 180, 80);
        gr.SetColor(0xFFD700);
        gr.DrawRect(30, 130, 179, 79);

        GMIDlet.a(0, _selectedLevel == 13 ? "SURVIVAL ARENA" : $"STAGE {_selectedLevel} OF 12", 120, 150, 17);
        GMIDlet.a(0, $"UNLOCKED: {maxLvl}/12", 120, 175, 17);

        GMIDlet.a(0, "START: ENTER", 6, 304, 20);
        GMIDlet.a(0, "BACK: ESC", 234, 304, 24);
    }

    private void RenderHighScores(Graphics gr)
    {
        GMIDlet.a(1, "HALL OF FAME", 120, 6, 17);

        GMIDlet.a(1, "-- STORY MODE --", 120, 40, 17);
        for (int i = 0; i < 5; i++)
        {
            string line = $"{i + 1}. {_saveManager.Data.StoryHighScoreNames[i],-4}  {_saveManager.Data.StoryHighScores[i],7}";
            GMIDlet.a(0, line, 120, 65 + i * 20, 17);
        }

        GMIDlet.a(1, "-- SURVIVAL MODE --", 120, 180, 17);
        for (int i = 0; i < 5; i++)
        {
            string line = $"{i + 1}. {_saveManager.Data.SurvivalHighScoreNames[i],-4}  {_saveManager.Data.SurvivalHighScores[i],7}";
            GMIDlet.a(0, line, 120, 205 + i * 20, 17);
        }

        GMIDlet.a(0, "RESET: SHIFT/Q", 6, 304, 20);
        GMIDlet.a(0, "BACK: ESC", 234, 304, 24);
    }

    private void RenderOptions(Graphics gr)
    {
        GMIDlet.a(1, "OPTIONS", 120, 6, 17);

        string[] langNames = { "DEUTSCH", "ENGLISH", "FRANCAIS", "ITALIANO" };
        string curLang = langNames[Math.Clamp(_saveManager.Data.LanguageIndex, 0, 3)];

        string testAudioLabel = _audioTestState switch
        {
            1 => "PLAY SOUND 1",
            2 => "PLAY SOUND 2",
            _ => "IDLE / STOP"
        };

        string[] items = {
            $"LANGUAGE: < {curLang} >",
            $"SOUND: < {(_saveManager.Data.SoundEnabled ? "ON" : "OFF")} >",
            $"VOLUME: < {_saveManager.Data.Volume / 10}% >",
            $"FRAME RATE: < {(_saveManager.Data.Smooth60Fps ? "SMOOTH (60)" : "ORIGINAL (14)")} >",
            $"SCREEN: < {(_saveManager.Data.Widescreen ? "WIDESCREEN" : "ORIGINAL")} >",
            $"SCALE: < {_saveManager.Data.Scale}X >",
            $"FULLSCREEN: < {(_saveManager.Data.Fullscreen ? "YES" : "NO")} >",
            $"TEST AUDIO: < {testAudioLabel} >"
        };

        int startY = 32;
        for (int i = 0; i < items.Length; i++)
        {
            bool sel = i == _subIndex;
            if (sel)
            {
                gr.SetColor(0x401010);
                gr.FillRect(10, startY + i * 31 - 3, 220, 24);
                gr.SetColor(0xFFD700);
                gr.DrawRect(10, startY + i * 31 - 3, 219, 23);
                GMIDlet.a(1, items[i], 120, startY + i * 31, 17);
            }
            else
            {
                GMIDlet.a(0, items[i], 120, startY + i * 31, 17);
            }
        }

        GMIDlet.a(0, "CHANGE: LEFT/RIGHT", 6, 304, 20);
        GMIDlet.a(0, "BACK: ESC", 234, 304, 24);
    }

    private static void RenderHelp(Graphics gr)
    {
        GMIDlet.a(1, "CONTROLS", 120, 6, 17);

        string[] lines = {
            "MOVE: ARROWS / WASD / NUMPAD",
            "ATTACK / JUMP: SPACE / ENTER / 5",
            "MAGIC SPELL: L-CTRL / 0",
            "SPECIAL COMBO: ATTACK + MOVE",
            "PAUSE: ESCAPE / P",
            "FULLSCREEN: F11",
            "",
            "GAMEPAD SUPPORTED:",
            "D-PAD: MOVE",
            "A: ATTACK  |  B: MAGIC",
            "START: PAUSE"
        };

        for (int i = 0; i < lines.Length; i++)
        {
            GMIDlet.a(0, lines[i], 120, 45 + i * 22, 17);
        }

        GMIDlet.a(0, "BACK: ESC / ENTER", 120, 304, 17);
    }

    private void RenderPauseOverlay(Graphics gr)
    {
        gr.SetColor(unchecked((int)0xCC000000));
        gr.FillRect(20, 60, 200, 180);
        gr.SetColor(0xFFD700);
        gr.DrawRect(20, 60, 199, 179);

        GMIDlet.a(1, "PAUSED", 120, 80, 17);

        string[] items = { "RESUME", "RESTART LEVEL", "EXIT TO MENU" };
        for (int i = 0; i < items.Length; i++)
        {
            if (i == _menuIndex)
            {
                GMIDlet.a(1, "> " + items[i] + " <", 120, 130 + i * 30, 17);
            }
            else
            {
                GMIDlet.a(0, items[i], 120, 130 + i * 30, 17);
            }
        }
    }

    private void RenderNameEntry(Graphics gr)
    {
        GMIDlet.a(1, "NEW HIGH SCORE!", 120, 60, 17);
        GMIDlet.a(1, $"SCORE: {_lastFinalScore}", 120, 95, 17);

        GMIDlet.a(0, "ENTER YOUR INITIALS:", 120, 140, 17);

        int startX = 90;
        for (int i = 0; i < 4; i++)
        {
            bool cur = i == _nameCursor;
            gr.SetColor(cur ? 0xFFD700 : 0x555555);
            gr.FillRect(startX + i * 16, 175, 12, 20);

            gr.SetColor(0x000000);
            gr.FillRect(startX + i * 16 + 1, 176, 10, 18);

            GMIDlet.a(1, _nameInput[i].ToString(), startX + i * 16 + 6, 178, 17);
        }

        GMIDlet.a(0, "UP/DOWN: CHANGE LETTER", 120, 230, 17);
        GMIDlet.a(0, "ENTER: NEXT / SUBMIT", 120, 250, 17);
    }

    private void RenderGameOver(Graphics gr)
    {
        GMIDlet.a(1, "GAME OVER", 120, 90, 17);
        GMIDlet.a(1, $"FINAL SCORE: {_lastFinalScore}", 120, 130, 17);

        if ((_animTick % 30) < 20)
        {
            GMIDlet.a(0, "PRESS ENTER TO CONTINUE", 120, 210, 17);
        }
    }
}
