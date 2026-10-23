using System;
using System.IO;

namespace PR.Core;

/// <summary>
/// Big-endian stream reader matching Java's DataInputStream semantics.
/// </summary>
public sealed class DataReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buf = new byte[8];

    public DataReader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    public Stream BaseStream => _stream;

    public sbyte ReadByte()
    {
        int b = _stream.ReadByte();
        if (b < 0) throw new EndOfStreamException();
        return (sbyte)(byte)b;
    }

    public int ReadUnsignedByte()
    {
        int b = _stream.ReadByte();
        if (b < 0) throw new EndOfStreamException();
        return b;
    }

    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    public short ReadShort()
    {
        ReadFully(_buf, 0, 2);
        return (short)((_buf[0] << 8) | _buf[1]);
    }

    public int ReadUnsignedShort()
    {
        ReadFully(_buf, 0, 2);
        return ((_buf[0] & 0xFF) << 8) | (_buf[1] & 0xFF);
    }

    public int ReadInt()
    {
        ReadFully(_buf, 0, 4);
        return (_buf[0] << 24) | ((_buf[1] & 0xFF) << 16) | ((_buf[2] & 0xFF) << 8) | (_buf[3] & 0xFF);
    }

    public long ReadLong()
    {
        ReadFully(_buf, 0, 8);
        return ((long)_buf[0] << 56) |
               (((long)_buf[1] & 0xFF) << 48) |
               (((long)_buf[2] & 0xFF) << 40) |
               (((long)_buf[3] & 0xFF) << 32) |
               (((long)_buf[4] & 0xFF) << 24) |
               (((long)_buf[5] & 0xFF) << 16) |
               (((long)_buf[6] & 0xFF) << 8) |
               ((long)_buf[7] & 0xFF);
    }

    public void ReadFully(byte[] buffer)
    {
        ReadFully(buffer, 0, buffer.Length);
    }

    public void ReadFully(byte[] buffer, int offset, int length)
    {
        int total = 0;
        while (total < length)
        {
            int r = _stream.Read(buffer, offset + total, length - total);
            if (r <= 0) throw new EndOfStreamException($"Expected {length} bytes, read {total}");
            total += r;
        }
    }

    public int SkipBytes(int n)
    {
        int total = 0;
        while (total < n)
        {
            int toRead = Math.Min(n - total, _buf.Length);
            int r = _stream.Read(_buf, 0, toRead);
            if (r <= 0) break;
            total += r;
        }
        return total;
    }

    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}
