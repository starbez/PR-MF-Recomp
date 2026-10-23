using System;
using System.Numerics;
using Raylib_cs;
using PR.Core;
using PRImage = PR.Core.Graphics.Image;

namespace PR.Game.UI;

public sealed class GameView : IDisposable
{
    public const int BaseWidth = EngineTweaks.BaseWidth;
    public const int BaseHeight = EngineTweaks.BaseHeight;
    public const int MaxWidth = EngineTweaks.WidescreenWidth;

    private readonly Texture2D _screenTexture;
    private readonly Raylib_cs.Image _raylibImage;
    private int _scale;
    private bool _fullscreen;
    private int _activeCanvasWidth;
    private Rectangle _destRect;

    public bool FitScaling { get; set; } = false;
    public int Scale => _scale;
    public bool IsFullscreen => _fullscreen;
    public int CurrentWidth => EngineTweaks.RenderWidth;
    public int CurrentHeight => EngineTweaks.RenderHeight;

    public Vector2 ScreenToVirtual(Vector2 screenPos)
    {
        if (screenPos.X < _destRect.X || screenPos.X >= _destRect.X + _destRect.Width ||
            screenPos.Y < _destRect.Y || screenPos.Y >= _destRect.Y + _destRect.Height)
        {
            return new Vector2(-1, -1);
        }

        if (_destRect.Width <= 0 || _destRect.Height <= 0) return new Vector2(-1, -1);

        float virtX = (screenPos.X - _destRect.X) * _activeCanvasWidth / _destRect.Width;
        float virtY = (screenPos.Y - _destRect.Y) * BaseHeight / _destRect.Height;
        return new Vector2(virtX, virtY);
    }

    public unsafe GameView(int scale, bool fullscreen)
    {
        _scale = Math.Clamp(scale, 1, 6);
        _fullscreen = fullscreen;
        _activeCanvasWidth = CurrentWidth;

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(CurrentWidth * _scale, CurrentHeight * _scale, "Power Rangers: Mystic Force");
        Raylib.SetTargetFPS(60);

        if (_fullscreen)
        {
            Raylib.ToggleFullscreen();
        }

        // Allocate maximum texture size (568x320)
        _raylibImage = Raylib.GenImageColor(MaxWidth, BaseHeight, Color.Black);
        _screenTexture = Raylib.LoadTextureFromImage(_raylibImage);
        Raylib.SetTextureFilter(_screenTexture, TextureFilter.Point);
    }

    public void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        Raylib.ToggleFullscreen();
    }

    public void SetScale(int scale)
    {
        _scale = Math.Clamp(scale, 1, 6);
        if (!_fullscreen)
        {
            Raylib.SetWindowSize(CurrentWidth * _scale, CurrentHeight * _scale);
        }
    }

    public void UpdateWidescreenMode()
    {
        _activeCanvasWidth = CurrentWidth;
        if (!_fullscreen)
        {
            Raylib.SetWindowSize(CurrentWidth * _scale, CurrentHeight * _scale);
        }
    }

    public unsafe void UpdateFromCanvas(PRImage canvas)
    {
        if (canvas == null) return;
        _activeCanvasWidth = canvas.Width;

        int count = Math.Min(canvas.Width * canvas.Height, MaxWidth * BaseHeight);
        fixed (uint* src = canvas.Pixels)
        {
            Color* dst = (Color*)_raylibImage.Data;
            for (int i = 0; i < count; i++)
            {
                uint pixel = src[i];
                byte a = (byte)(pixel >> 24);
                byte r = (byte)(pixel >> 16);
                byte g = (byte)(pixel >> 8);
                byte b = (byte)pixel;
                dst[i] = new Color(r, g, b, a);
            }
        }

        Raylib.UpdateTexture(_screenTexture, _raylibImage.Data);
    }

    public void Render()
    {
        int winWidth = Raylib.GetScreenWidth();
        int winHeight = Raylib.GetScreenHeight();

        int curW = _activeCanvasWidth;
        int curH = BaseHeight;

        int drawW, drawH;
        if (FitScaling)
        {
            float scaleX = (float)winWidth / curW;
            float scaleY = (float)winHeight / curH;
            float fitScale = Math.Min(scaleX, scaleY);
            drawW = Math.Max(1, (int)(curW * fitScale));
            drawH = Math.Max(1, (int)(curH * fitScale));
        }
        else
        {
            // Calculate maximum integer scale that fits the window
            int scaleX = winWidth / curW;
            int scaleY = winHeight / curH;
            int intScale = Math.Max(1, Math.Min(scaleX, scaleY));
            drawW = curW * intScale;
            drawH = curH * intScale;
        }

        int posX = (winWidth - drawW) / 2;
        int posY = (winHeight - drawH) / 2;
        _destRect = new Rectangle(posX, posY, drawW, drawH);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Draw game screen texture scaled with point filtering
        var srcRec = new Rectangle(0, 0, curW, curH);
        Raylib.DrawTexturePro(_screenTexture, srcRec, _destRect, Vector2.Zero, 0.0f, Color.White);

        Raylib.EndDrawing();
    }

    public void Dispose()
    {
        Raylib.UnloadTexture(_screenTexture);
        Raylib.UnloadImage(_raylibImage);
        Raylib.CloseWindow();
    }
}
