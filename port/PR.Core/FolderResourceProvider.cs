using System;
using System.IO;

namespace PR.Core;

public sealed class FolderResourceProvider : IResourceProvider
{
    private readonly string _baseDir;

    public FolderResourceProvider(string baseDir)
    {
        _baseDir = baseDir;
    }

    private string ResolvePath(string path)
    {
        string clean = path.TrimStart('/', '\\');
        return Path.Combine(_baseDir, clean);
    }

    public Stream? OpenResource(string path)
    {
        string full = ResolvePath(path);
        if (File.Exists(full))
        {
            return File.OpenRead(full);
        }
        return null;
    }

    public byte[]? GetResourceBytes(string path)
    {
        string full = ResolvePath(path);
        if (File.Exists(full))
        {
            return File.ReadAllBytes(full);
        }
        return null;
    }

    public bool ResourceExists(string path)
    {
        return File.Exists(ResolvePath(path));
    }

    public void Dispose()
    {
    }
}
