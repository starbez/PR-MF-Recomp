using System;
using System.Collections.Generic;
using PR.Core;

namespace PR.Core.Audio;

public record SoundTrack(int Id, byte[] Data, string MimeType);

public static class SoundEngine
{
    public static readonly Dictionary<int, SoundTrack> Tracks = new();
    public static readonly int[] EventToSoundMap = new int[7];
    public static readonly int[] SoundLoopMap = new int[16];

    public static bool AudioLogging { get; set; } = false;
    public static bool Enabled { get; set; } = true;
    public static int ActiveSoundId { get; private set; } = -1;

    public static event Action<int, int>? OnPlaySound; // (soundId, loopCount)
    public static event Action? OnStopSound;
    public static List<string> EventLog { get; } = new();

    public static void Reset()
    {
        Tracks.Clear();
        Array.Clear(EventToSoundMap, 0, EventToSoundMap.Length);
        Array.Clear(SoundLoopMap, 0, SoundLoopMap.Length);
        ActiveSoundId = -1;
        EventLog.Clear();
    }

    public static void Load(DataReader dis)
    {
        // Mirrors e.a(DataInputStream)
        int soundCount = dis.ReadShort();
        for (int i = 1; i <= soundCount; ++i)
        {
            dis.ReadByte();
            short len = dis.ReadShort();
            byte[] raw = new byte[len + 1];
            dis.ReadFully(raw);

            byte mimeIdx = raw[len];
            byte[] blob = new byte[len];
            Array.Copy(raw, 0, blob, 0, len);

            string mime = mimeIdx switch
            {
                0 => "audio/midi",
                1 => "audio/x-wav",
                2 => "audio/mpeg",
                7 => "audio/amr",
                _ => "audio/midi"
            };

            Tracks[i] = new SoundTrack(i, blob, mime);
            SoundLoopMap[i] = 1;

            int n2 = dis.ReadShort();
            while (n2-- > 0)
            {
                byte ev = (byte)dis.ReadByte();
                if (ev < EventToSoundMap.Length)
                {
                    EventToSoundMap[ev] = i;
                    SoundLoopMap[i] = (ev <= 1) ? -1 : 1;
                }
            }
            dis.ReadShort();
        }
    }

    public static void Play(int eventId, int tick = -1)
    {
        if (!Enabled) return;
        if (eventId < 0 || eventId >= EventToSoundMap.Length) return;
        int soundId = EventToSoundMap[eventId];
        if (soundId <= 0 || !Tracks.ContainsKey(soundId)) return;

        int loop = SoundLoopMap[soundId];
        string logEntry = $"Tick {tick}: Play event={eventId}, sound={soundId}, loop={loop}";
        EventLog.Add(logEntry);
        if (AudioLogging)
        {
            Console.WriteLine($"[Audio] {logEntry} ({Tracks[soundId].Data.Length} bytes, {Tracks[soundId].MimeType})");
        }

        if (ActiveSoundId >= 0 && ActiveSoundId != soundId)
        {
            Stop(tick);
        }

        ActiveSoundId = soundId;
        OnPlaySound?.Invoke(soundId, loop);
    }

    public static void PlaySoundDirect(int soundId, bool loop = true)
    {
        if (!Enabled) return;
        if (!Tracks.ContainsKey(soundId)) return;

        int loopCount = loop ? -1 : 1;
        string logEntry = $"Play direct sound={soundId}, loop={loopCount}";
        EventLog.Add(logEntry);
        if (AudioLogging)
        {
            Console.WriteLine($"[Audio] {logEntry} ({Tracks[soundId].Data.Length} bytes, {Tracks[soundId].MimeType})");
        }

        if (ActiveSoundId >= 0 && ActiveSoundId != soundId)
        {
            Stop();
        }

        ActiveSoundId = soundId;
        OnPlaySound?.Invoke(soundId, loopCount);
    }

    public static void Stop(int tick = -1)
    {
        if (ActiveSoundId >= 0)
        {
            string logEntry = $"Tick {tick}: Stop sound={ActiveSoundId}";
            EventLog.Add(logEntry);
            if (AudioLogging)
            {
                Console.WriteLine($"[Audio] {logEntry}");
            }
            ActiveSoundId = -1;
            OnStopSound?.Invoke();
        }
    }
}
