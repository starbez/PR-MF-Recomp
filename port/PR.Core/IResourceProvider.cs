using System;
using System.IO;

namespace PR.Core;

public interface IResourceProvider : IDisposable
{
    Stream? OpenResource(string path);
    byte[]? GetResourceBytes(string path);
    bool ResourceExists(string path);
}
