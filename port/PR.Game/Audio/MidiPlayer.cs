using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    private readonly object _lock = new();
    private readonly Thread _workerThread;
    private readonly AutoResetEvent _wakeEvent = new(false);

    private volatile bool _isRunning = true;
    private volatile string? _currentAlias;
    private volatile bool _isMuted;
    private volatile bool _isLooping;
    private volatile int _volume = 800; // 0..1000

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
            lock (_lock)
            {
                if (_currentAlias != null && OperatingSystem.IsWindows())
                {
                    WinMm.mciSendString($"setaudio {_currentAlias} volume to {_volume}", null, 0, IntPtr.Zero);
                }
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

        _workerThread = new Thread(BackgroundLoopWorker)
        {
            Name = "MidiPlayer_Worker",
            IsBackground = true,
            Priority = ThreadPriority.Normal
        };
        _workerThread.Start();

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
                lock (_lock)
                {
                    StopInternal();

                    string alias = "pr_bgm_" + DateTime.UtcNow.Ticks;
                    string absPath = Path.GetFullPath(filePath);

                    int openResult = WinMm.mciSendString($"open \"{absPath}\" type sequencer alias {alias}", null, 0, IntPtr.Zero);
                    if (openResult == 0)
                    {
                        _currentAlias = alias;
                        _isLooping = loop;
                        WinMm.mciSendString($"setaudio {alias} volume to {_volume}", null, 0, IntPtr.Zero);
                        WinMm.mciSendString($"play {alias} from 0", null, 0, IntPtr.Zero);
                    }
                }
                _wakeEvent.Set();
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
            if (!File.Exists(outPath))
            {
                File.WriteAllBytes(outPath, midiBytes);
            }
            PlayMidi(outPath, loop);
        }
        catch
        {
            // Fallback
        }
    }

    /// <summary>
    /// Per-frame update hook. Main thread does 0 ms work; all status polling and loop rewind
    /// are offloaded to BackgroundLoopWorker to prevent 60 FPS gameplay stutter.
    /// </summary>
    public void Update()
    {
        // No-op on the main thread: prevents MCI sequencer rewind blocking (which takes ~180ms)
        // from freezing the render loop.
    }

    /// <summary>
    /// Dedicated audio worker thread running loop detection and track rewind in the background.
    /// </summary>
    private void BackgroundLoopWorker()
    {
        var sb = new StringBuilder(64);
        while (_isRunning)
        {
            string? alias;
            bool loop;
            lock (_lock)
            {
                alias = _currentAlias;
                loop = _isLooping && !_isMuted;
            }

            if (alias != null && loop && OperatingSystem.IsWindows())
            {
                sb.Clear();
                int res = WinMm.mciSendString($"status {alias} mode", sb, sb.Capacity, IntPtr.Zero);
                if (res == 0 && sb.ToString().Equals("stopped", StringComparison.OrdinalIgnoreCase))
                {
                    // Track reached the end! Restart playback on this background worker thread.
                    // The synchronous mciseq.drv rewind (~180ms) happens here without blocking the game's 60 FPS render thread.
                    lock (_lock)
                    {
                        if (_isLooping && !_isMuted && _currentAlias == alias)
                        {
                            WinMm.mciSendString($"play {alias} from 0", null, 0, IntPtr.Zero);
                        }
                    }
                }

                // Poll every 12ms while active to ensure minimal audio gap on track end
                _wakeEvent.WaitOne(12);
            }
            else
            {
                // Idle when not playing or not looping
                _wakeEvent.WaitOne(50);
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            StopInternal();
        }
        _wakeEvent.Set();
    }

    private void StopInternal()
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
        _isRunning = false;
        _wakeEvent.Set();
        if (_workerThread.IsAlive)
        {
            _workerThread.Join(200);
        }
        _wakeEvent.Dispose();

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
