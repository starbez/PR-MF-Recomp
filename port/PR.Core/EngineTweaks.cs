namespace PR.Core;

public static class EngineTweaks
{
    public const int BaseWidth = 240;
    public const int WidescreenWidth = 568; // 16:9 aspect ratio at 320 height (568x320)
    public const int BaseHeight = 320;

    /// <summary>
    /// Smooth 60 FPS sub-frame interpolation for camera and actors.
    /// Logic remains strictly at 70ms step. Disabled in tests.
    /// </summary>
    public static bool Smooth60Fps { get; set; } = false;

    /// <summary>
    /// Widescreen 16:9 view extension. Logic rectangle preserved. Disabled in tests.
    /// </summary>
    public static bool Widescreen { get; set; } = false;

    public static int RenderWidth => Widescreen ? WidescreenWidth : BaseWidth;
    public static int RenderHeight => BaseHeight;
}
