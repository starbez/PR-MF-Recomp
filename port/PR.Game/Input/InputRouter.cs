using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Raylib_cs;

namespace PR.Game.Input;

public sealed class KeyBindings
{
    public List<KeyboardKey> UpKeys { get; set; } = new() { KeyboardKey.Up, KeyboardKey.W, KeyboardKey.Kp8, KeyboardKey.Kp2 };
    public List<KeyboardKey> DownKeys { get; set; } = new() { KeyboardKey.Down, KeyboardKey.S, KeyboardKey.Kp2, KeyboardKey.Kp8 };
    public List<KeyboardKey> LeftKeys { get; set; } = new() { KeyboardKey.Left, KeyboardKey.A, KeyboardKey.Kp4 };
    public List<KeyboardKey> RightKeys { get; set; } = new() { KeyboardKey.Right, KeyboardKey.D, KeyboardKey.Kp6 };
    public List<KeyboardKey> FireKeys { get; set; } = new() { KeyboardKey.Space, KeyboardKey.Enter, KeyboardKey.Kp5, KeyboardKey.Z, KeyboardKey.J };
    public List<KeyboardKey> MagicKeys { get; set; } = new() { KeyboardKey.LeftControl, KeyboardKey.Zero, KeyboardKey.Kp0, KeyboardKey.X, KeyboardKey.K };
    public List<KeyboardKey> LeftSoftKeys { get; set; } = new() { KeyboardKey.LeftShift, KeyboardKey.Q, KeyboardKey.F1, KeyboardKey.C, KeyboardKey.U };
    public List<KeyboardKey> RightSoftKeys { get; set; } = new() { KeyboardKey.Escape, KeyboardKey.E, KeyboardKey.Backspace, KeyboardKey.F2, KeyboardKey.V, KeyboardKey.I };
}

public sealed class InputRouter
{
    // J2ME Key Codes
    public const int KEY_UP = -1;
    public const int KEY_DOWN = -2;
    public const int KEY_LEFT = -3;
    public const int KEY_RIGHT = -4;
    public const int KEY_FIRE = -5;
    public const int KEY_SOFT_LEFT = -6;
    public const int KEY_SOFT_RIGHT = -7;
    public const int KEY_NUM0 = 48;
    public const int KEY_NUM1 = 49;
    public const int KEY_NUM2 = 50;
    public const int KEY_NUM3 = 51;
    public const int KEY_NUM4 = 52;
    public const int KEY_NUM5 = 53;
    public const int KEY_NUM6 = 54;
    public const int KEY_NUM7 = 55;
    public const int KEY_NUM8 = 56;
    public const int KEY_NUM9 = 57;
    public const int KEY_STAR = 42;
    public const int KEY_POUND = 35;

    private static readonly string BindingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PowerRangers",
        "bindings.json"
    );

    public KeyBindings Bindings { get; private set; }
    private readonly HashSet<int> _currentlyPressedKeys = new();

    public InputRouter()
    {
        Bindings = LoadBindings();
    }

    private KeyBindings LoadBindings()
    {
        try
        {
            if (File.Exists(BindingsFilePath))
            {
                string json = File.ReadAllText(BindingsFilePath);
                var loaded = JsonSerializer.Deserialize<KeyBindings>(json);
                if (loaded != null) return loaded;
            }
        }
        catch
        {
            // Fallback to default
        }
        return new KeyBindings();
    }

    public void SaveBindings()
    {
        try
        {
            string dir = Path.GetDirectoryName(BindingsFilePath)!;
            Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(Bindings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(BindingsFilePath, json);
        }
        catch
        {
            // Ignore
        }
    }

    public void Update(Action<int> onKeyDown, Action<int> onKeyUp)
    {
        var activeThisFrame = new HashSet<int>();

        // Keyboard checks
        CheckKeyboardAction(Bindings.UpKeys, KEY_UP, activeThisFrame);
        CheckKeyboardAction(Bindings.DownKeys, KEY_DOWN, activeThisFrame);
        CheckKeyboardAction(Bindings.LeftKeys, KEY_LEFT, activeThisFrame);
        CheckKeyboardAction(Bindings.RightKeys, KEY_RIGHT, activeThisFrame);
        CheckKeyboardAction(Bindings.FireKeys, KEY_FIRE, activeThisFrame);
        CheckKeyboardAction(Bindings.MagicKeys, KEY_NUM0, activeThisFrame);
        CheckKeyboardAction(Bindings.LeftSoftKeys, KEY_SOFT_LEFT, activeThisFrame);
        CheckKeyboardAction(Bindings.RightSoftKeys, KEY_SOFT_RIGHT, activeThisFrame);

        // Digits 0..9 (direct phone key entry)
        if (Raylib.IsKeyDown(KeyboardKey.One) || Raylib.IsKeyDown(KeyboardKey.Kp1)) activeThisFrame.Add(KEY_NUM1);
        if (Raylib.IsKeyDown(KeyboardKey.Two) || Raylib.IsKeyDown(KeyboardKey.Kp2)) activeThisFrame.Add(KEY_NUM2);
        if (Raylib.IsKeyDown(KeyboardKey.Three) || Raylib.IsKeyDown(KeyboardKey.Kp3)) activeThisFrame.Add(KEY_NUM3);
        if (Raylib.IsKeyDown(KeyboardKey.Four) || Raylib.IsKeyDown(KeyboardKey.Kp4)) activeThisFrame.Add(KEY_NUM4);
        if (Raylib.IsKeyDown(KeyboardKey.Five) || Raylib.IsKeyDown(KeyboardKey.Kp5)) activeThisFrame.Add(KEY_NUM5);
        if (Raylib.IsKeyDown(KeyboardKey.Six) || Raylib.IsKeyDown(KeyboardKey.Kp6)) activeThisFrame.Add(KEY_NUM6);
        if (Raylib.IsKeyDown(KeyboardKey.Seven) || Raylib.IsKeyDown(KeyboardKey.Kp7)) activeThisFrame.Add(KEY_NUM7);
        if (Raylib.IsKeyDown(KeyboardKey.Eight) || Raylib.IsKeyDown(KeyboardKey.Kp8)) activeThisFrame.Add(KEY_NUM8);
        if (Raylib.IsKeyDown(KeyboardKey.Nine) || Raylib.IsKeyDown(KeyboardKey.Kp9)) activeThisFrame.Add(KEY_NUM9);
        if (Raylib.IsKeyDown(KeyboardKey.Zero) || Raylib.IsKeyDown(KeyboardKey.Kp0)) activeThisFrame.Add(KEY_NUM0);

        // Gamepad (Gamepad 0)
        if (Raylib.IsGamepadAvailable(0))
        {
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.LeftFaceUp) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) < -0.4f)
                activeThisFrame.Add(KEY_UP);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.LeftFaceDown) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) > 0.4f)
                activeThisFrame.Add(KEY_DOWN);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.LeftFaceLeft) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) < -0.4f)
                activeThisFrame.Add(KEY_LEFT);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.LeftFaceRight) || Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) > 0.4f)
                activeThisFrame.Add(KEY_RIGHT);

            if (Raylib.IsGamepadButtonDown(0, GamepadButton.RightFaceDown)) // Button A
                activeThisFrame.Add(KEY_FIRE);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.RightFaceRight)) // Button B
                activeThisFrame.Add(KEY_NUM0);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.RightFaceLeft)) // Button X
                activeThisFrame.Add(KEY_SOFT_LEFT);
            if (Raylib.IsGamepadButtonDown(0, GamepadButton.RightFaceUp) || Raylib.IsGamepadButtonDown(0, GamepadButton.MiddleRight)) // Button Y / Start
                activeThisFrame.Add(KEY_SOFT_RIGHT);
        }

        // Trigger press events
        foreach (int key in activeThisFrame)
        {
            if (!_currentlyPressedKeys.Contains(key))
            {
                _currentlyPressedKeys.Add(key);
                onKeyDown(key);
            }
        }

        // Trigger release events
        var toRemove = new List<int>();
        foreach (int key in _currentlyPressedKeys)
        {
            if (!activeThisFrame.Contains(key))
            {
                toRemove.Add(key);
                onKeyUp(key);
            }
        }

        foreach (int key in toRemove)
        {
            _currentlyPressedKeys.Remove(key);
        }
    }

    private static void CheckKeyboardAction(List<KeyboardKey> keys, int actionCode, HashSet<int> active)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (Raylib.IsKeyDown(keys[i]))
            {
                active.Add(actionCode);
                return;
            }
        }
    }
}
