using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using PR.Core.Audio;

namespace PR.Game.Audio;

public sealed class MidiPlayer : IDisposable
{
    private static class WinMm
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int mciSendString(string command, StringBuilder? returnString, int returnLength, IntPtr hwndCallback);
    }

    private readonly string _tempDir;
    private string? _currentAlias;
    private bool _isMuted;
    private bool _isLooping;
    private int _volume = 800; // 0..1000

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            if (_isMuted) Stop();
        }
    }

    public int Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, 1000);
            if (_currentAlias != null && OperatingSystem.IsWindows())
            {
                WinMm.mciSendString($"setaudio {_currentAlias} volume to {_volume}", null, 0, IntPtr.Zero);
            }
        }
    }

    public MidiPlayer()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PowerRangersAudio_" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            Directory.CreateDirectory(_tempDir);
        }
        catch
        {
            // Ignore
        }

        AttachSoundEngine();
    }

    public void AttachSoundEngine()
    {
        SoundEngine.OnPlaySound += HandleSoundPlay;
        SoundEngine.OnStopSound += HandleSoundStop;
    }

    public void DetachSoundEngine()
    {
        SoundEngine.OnPlaySound -= HandleSoundPlay;
        SoundEngine.OnStopSound -= HandleSoundStop;
    }

    private void HandleSoundPlay(int soundId, int loopCount)
    {
        if (SoundEngine.Tracks.TryGetValue(soundId, out var track))
        {
            PlayMidiBytes(track.Data, $"sound_{soundId}", loopCount == -1);
        }
    }

    private void HandleSoundStop()
    {
        Stop();
    }

    public void PlayMidi(string filePath, bool loop = true)
    {
        if (_isMuted) return;
        if (!File.Exists(filePath)) return;

        if (OperatingSystem.IsWindows())
        {
            try
            {
                Stop();

                string alias = "pr_bgm_" + DateTime.UtcNow.Ticks;
                string absPath = Path.GetFullPath(filePath);

                int openResult = WinMm.mciSendString($"open \"{absPath}\" type sequencer alias {alias}", null, 0, IntPtr.Zero);
                if (openResult == 0)
                {
                    _currentAlias = alias;
                    _isLooping = loop;
                    WinMm.mciSendString($"setaudio {alias} volume to {_volume}", null, 0, IntPtr.Zero);
                    // Windows MCI MIDI sequencer does not support 'repeat' flag; play directly and loop in Update()
                    string playCmd = $"play {alias} from 0";
                    WinMm.mciSendString(playCmd, null, 0, IntPtr.Zero);
                }
            }
            catch
            {
                // Audio failure shouldn't crash the game
            }
        }
    }

    public void PlayMidiBytes(byte[] midiBytes, string nameHint, bool loop = true)
    {
        if (_isMuted || midiBytes == null || midiBytes.Length == 0) return;
        try
        {
            string outPath = Path.Combine(_tempDir, $"{nameHint}.mid");
            File.WriteAllBytes(outPath, midiBytes);
            PlayMidi(outPath, loop);
        }
        catch
        {
            // Fallback
        }
    }

    public void Update()
    {
        if (_isLooping && _currentAlias != null && OperatingSystem.IsWindows())
        {
            try
            {
                var sb = new StringBuilder(64);
                int res = WinMm.mciSendString($"status {_currentAlias} mode", sb, sb.Capacity, IntPtr.Zero);
                if (res == 0 && sb.ToString().Equals("stopped", StringComparison.OrdinalIgnoreCase))
                {
                    WinMm.mciSendString($"play {_currentAlias} from 0", null, 0, IntPtr.Zero);
                }
            }
            catch
            {
                // Ignore
            }
        }
    }

    public void Stop()
    {
        _isLooping = false;
        if (_currentAlias != null && OperatingSystem.IsWindows())
        {
            try
            {
                WinMm.mciSendString($"stop {_currentAlias}", null, 0, IntPtr.Zero);
                WinMm.mciSendString($"close {_currentAlias}", null, 0, IntPtr.Zero);
            }
            catch
            {
                // Ignore
            }
            finally
            {
                _currentAlias = null;
            }
        }
    }

    public void Dispose()
    {
        DetachSoundEngine();
        Stop();
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore
        }
    }
}
