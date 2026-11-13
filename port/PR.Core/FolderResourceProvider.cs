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
        return CustomResources.TryOpen(path);
    }

    public byte[]? GetResourceBytes(string path)
    {
        string full = ResolvePath(path);
        if (File.Exists(full))
        {
            return File.ReadAllBytes(full);
        }
        if (CustomResources.TryGet(path, out var data))
        {
            return data;
        }
        return null;
    }

    public bool ResourceExists(string path)
    {
        return File.Exists(ResolvePath(path)) || CustomResources.TryGet(path, out _);
    }

    public void Dispose()
    {
    }
}
