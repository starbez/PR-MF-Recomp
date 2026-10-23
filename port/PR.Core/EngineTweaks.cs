namespace PR.Core;

public static class EngineTweaks
{
    /// <summary>
    /// Smooth 60 FPS sub-frame interpolation for camera and actors.
    /// Logic remains strictly at 70ms step. Disabled in tests.
    /// </summary>
    public static bool Smooth60Fps { get; set; } = false;

    /// <summary>
    /// Widescreen 16:9 view extension. Logic rectangle preserved. Disabled in tests.
    /// </summary>
    public static bool Widescreen { get; set; } = false;
}
