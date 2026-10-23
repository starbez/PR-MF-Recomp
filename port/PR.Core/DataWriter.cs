using System;
using System.IO;

namespace PR.Core;

/// <summary>
/// Big-endian stream writer matching Java's DataOutputStream semantics.
/// </summary>
public sealed class DataWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buf = new byte[8];

    public DataWriter(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    public Stream BaseStream => _stream;

    public void WriteByte(int v)
    {
        _stream.WriteByte((byte)v);
    }

    public void WriteBoolean(bool v)
    {
        _stream.WriteByte(v ? (byte)1 : (byte)0);
    }

    public void WriteShort(int v)
    {
        _buf[0] = (byte)((v >> 8) & 0xFF);
        _buf[1] = (byte)(v & 0xFF);
        _stream.Write(_buf, 0, 2);
    }

    public void WriteInt(int v)
    {
        _buf[0] = (byte)((v >> 24) & 0xFF);
        _buf[1] = (byte)((v >> 16) & 0xFF);
        _buf[2] = (byte)((v >> 8) & 0xFF);
        _buf[3] = (byte)(v & 0xFF);
        _stream.Write(_buf, 0, 4);
    }

    public void WriteLong(long v)
    {
        _buf[0] = (byte)((v >> 56) & 0xFF);
        _buf[1] = (byte)((v >> 48) & 0xFF);
        _buf[2] = (byte)((v >> 40) & 0xFF);
        _buf[3] = (byte)((v >> 32) & 0xFF);
        _buf[4] = (byte)((v >> 24) & 0xFF);
        _buf[5] = (byte)((v >> 16) & 0xFF);
        _buf[6] = (byte)((v >> 8) & 0xFF);
        _buf[7] = (byte)(v & 0xFF);
        _stream.Write(_buf, 0, 8);
    }

    public void Write(byte[] buffer, int offset, int length)
    {
        _stream.Write(buffer, offset, length);
    }

    public void Flush()
    {
        _stream.Flush();
    }

    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}
