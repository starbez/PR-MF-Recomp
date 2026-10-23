using System;
using System.Collections.Concurrent;
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

    private enum MidiCommandType
    {
        Play,
        Stop,
        SetVolume
    }

    private sealed class MidiCommand
    {
        public MidiCommandType Type { get; init; }
        public string? FilePath { get; init; }
        public bool Loop { get; init; }
        public int Volume { get; init; }
    }

    private readonly string _tempDir;
    private readonly ConcurrentQueue<MidiCommand> _commands = new();
    private readonly AutoResetEvent _wakeEvent = new(false);
    private readonly Thread _workerThread;

    private volatile bool _isRunning = true;
    private volatile bool _isMuted;
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
            _commands.Enqueue(new MidiCommand
            {
                Type = MidiCommandType.SetVolume,
                Volume = _volume
            });
            _wakeEvent.Set();
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

        _commands.Enqueue(new MidiCommand
        {
            Type = MidiCommandType.Play,
            FilePath = filePath,
            Loop = loop,
            Volume = _volume
        });
        _wakeEvent.Set();
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
    /// Per-frame update hook. Main thread does 0 ms work; all MCI commands, status polling and
    /// track rewinds are offloaded to BackgroundLoopWorker to prevent audio stopping or stutter.
    /// </summary>
    public void Update()
    {
        // No-op on the main thread: prevents MCI sequencer rewind blocking from freezing render loop.
    }

    /// <summary>
    /// Dedicated audio worker thread running ALL Windows MCI operations.
    /// Because MCI device aliases in winmm.dll are bound to the thread that opens them,
    /// executing open, play, setaudio, status, stop, and close on this single thread guarantees
    /// status querying succeeds without MCIERR_INVALID_DEVICE_NAME (263), and looping runs continuously.
    /// </summary>
    private void BackgroundLoopWorker()
    {
        string? currentAlias = null;
        bool currentLoop = false;
        var sb = new StringBuilder(64);

        while (_isRunning)
        {
            // 1. Process all queued commands on this dedicated thread
            while (_commands.TryDequeue(out var cmd))
            {
                switch (cmd.Type)
                {
                    case MidiCommandType.Play:
                        if (OperatingSystem.IsWindows())
                        {
                            if (currentAlias != null)
                            {
                                WinMm.mciSendString($"stop {currentAlias}", null, 0, IntPtr.Zero);
                                WinMm.mciSendString($"close {currentAlias}", null, 0, IntPtr.Zero);
                                currentAlias = null;
                            }

                            if (!string.IsNullOrEmpty(cmd.FilePath) && File.Exists(cmd.FilePath))
                            {
                                string alias = "pr_bgm_" + Environment.TickCount64;
                                string absPath = Path.GetFullPath(cmd.FilePath);
                                int openResult = WinMm.mciSendString($"open \"{absPath}\" type sequencer alias {alias}", null, 0, IntPtr.Zero);
                                if (openResult == 0)
                                {
                                    currentAlias = alias;
                                    currentLoop = cmd.Loop;
                                    WinMm.mciSendString($"setaudio {alias} volume to {cmd.Volume}", null, 0, IntPtr.Zero);
                                    WinMm.mciSendString($"play {alias} from 0", null, 0, IntPtr.Zero);
                                }
                            }
                        }
                        break;

                    case MidiCommandType.Stop:
                        if (OperatingSystem.IsWindows() && currentAlias != null)
                        {
                            WinMm.mciSendString($"stop {currentAlias}", null, 0, IntPtr.Zero);
                            WinMm.mciSendString($"close {currentAlias}", null, 0, IntPtr.Zero);
                            currentAlias = null;
                            currentLoop = false;
                        }
                        break;

                    case MidiCommandType.SetVolume:
                        if (OperatingSystem.IsWindows() && currentAlias != null)
                        {
                            WinMm.mciSendString($"setaudio {currentAlias} volume to {cmd.Volume}", null, 0, IntPtr.Zero);
                        }
                        break;
                }
            }

            // 2. Loop detection if currently playing and loop enabled
            if (currentAlias != null && currentLoop && !_isMuted && OperatingSystem.IsWindows())
            {
                sb.Clear();
                int res = WinMm.mciSendString($"status {currentAlias} mode", sb, sb.Capacity, IntPtr.Zero);
                if (res == 0 && sb.ToString().Equals("stopped", StringComparison.OrdinalIgnoreCase))
                {
                    // Track finished: restart playback from tick 0 seamlessly
                    WinMm.mciSendString($"play {currentAlias} from 0", null, 0, IntPtr.Zero);
                }

                // Poll every 15ms while playing
                _wakeEvent.WaitOne(15);
            }
            else
            {
                // Wait for commands when stopped or idle
                _wakeEvent.WaitOne(50);
            }
        }

        // Cleanup on thread termination
        if (OperatingSystem.IsWindows() && currentAlias != null)
        {
            try
            {
                WinMm.mciSendString($"stop {currentAlias}", null, 0, IntPtr.Zero);
                WinMm.mciSendString($"close {currentAlias}", null, 0, IntPtr.Zero);
            }
            catch
            {
                // Ignore
            }
        }
    }

    public void Stop()
    {
        _commands.Enqueue(new MidiCommand { Type = MidiCommandType.Stop });
        _wakeEvent.Set();
    }

    public void Dispose()
    {
        _isRunning = false;
        _wakeEvent.Set();
        if (_workerThread.IsAlive)
        {
            _workerThread.Join(300);
        }
        _wakeEvent.Dispose();

        DetachSoundEngine();
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
