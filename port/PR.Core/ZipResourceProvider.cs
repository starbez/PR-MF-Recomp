using System;
using System.IO;
using System.IO.Compression;

namespace PR.Core;

public sealed class ZipResourceProvider : IResourceProvider
{
    private readonly ZipArchive _zip;
    private readonly bool _ownsStream;
    private readonly Stream? _underlyingStream;

    public ZipResourceProvider(string zipPath)
    {
        _underlyingStream = File.OpenRead(zipPath);
        _zip = new ZipArchive(_underlyingStream, ZipArchiveMode.Read, leaveOpen: false);
        _ownsStream = true;
    }

    public ZipResourceProvider(Stream stream, bool leaveOpen = false)
    {
        _underlyingStream = stream;
        _zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: leaveOpen);
        _ownsStream = !leaveOpen;
    }

    private ZipArchiveEntry? FindEntry(string path)
    {
        string clean = path.TrimStart('/', '\\').Replace('\\', '/');
        ZipArchiveEntry? entry = _zip.GetEntry(clean);
        if (entry != null) return entry;

        foreach (var e in _zip.Entries)
        {
            if (string.Equals(e.FullName, clean, StringComparison.OrdinalIgnoreCase))
                return e;
        }
        return null;
    }

    public Stream? OpenResource(string path)
    {
        var entry = FindEntry(path);
        if (entry == null) return null;
        var ms = new MemoryStream((int)entry.Length);
        using (var s = entry.Open())
        {
            s.CopyTo(ms);
        }
        ms.Position = 0;
        return ms;
    }

    public byte[]? GetResourceBytes(string path)
    {
        var entry = FindEntry(path);
        if (entry == null) return null;
        byte[] bytes = new byte[entry.Length];
        using (var s = entry.Open())
        {
            int total = 0;
            while (total < bytes.Length)
            {
                int r = s.Read(bytes, total, bytes.Length - total);
                if (r <= 0) break;
                total += r;
            }
        }
        return bytes;
    }

    public bool ResourceExists(string path)
    {
        return FindEntry(path) != null;
    }

    public void Dispose()
    {
        _zip.Dispose();
        if (_ownsStream && _underlyingStream != null)
        {
            _underlyingStream.Dispose();
        }
    }
}
