using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace TucanNet;

public sealed class Packet : IDisposable
{
    private readonly List<byte> _buffer;
    private int _readPosition;
    private bool _isDisposed;

    public Packet()
    {
        _buffer = new List<byte>();
    }

    public int BufferSize
    {
        get
        {
            return _buffer.Count;
        }
    }

    public int UnreadLength
    {
        get
        {
            return BufferSize - _readPosition;
        }
    }

    public byte[] ToArray()
    {
        return _buffer.ToArray();
    }

    public byte[] ReadBytes(int length)
    {
        var data = _buffer.GetRange(_readPosition, length).ToArray();
        _readPosition += length;
        return data;
    }

    public short ReadInt16()
    {
        return BitConverter.ToInt16(ReadBytes(2), 0);
    }

    public int ReadInt32()
    {
        return BitConverter.ToInt32(ReadBytes(4), 0);
    }

    public long ReadInt64()
    {
        return BitConverter.ToInt64(ReadBytes(8), 0);
    }

    public float ReadSingle()
    {
        return BitConverter.ToSingle(ReadBytes(4), 0);
    }

    public string ReadString(Encoding encoding)
    {
        var length = ReadInt32();
        return encoding.GetString(ReadBytes(length), 0, length);
    }
    
    public string ReadString()
    {
        return ReadString(Encoding.ASCII);
    }

    public bool TryReadBytes(int length, out byte[] data)
    {
        data = Array.Empty<byte>();
        if (length > UnreadLength)
        {
            return false;
        }

        data = ReadBytes(length);
        return true;
    }

    public bool TryReadInt16(out short data)
    {
        data = 0;
        if (2 > UnreadLength)
        {
            return false;
        }

        data = ReadInt16();
        return true;
    }

    public bool TryReadInt32(out int data)
    {
        data = 0;
        if (4 > UnreadLength)
        {
            return false;
        }

        data = ReadInt32();
        return true;
    }

    public bool TryReadInt64(out long data)
    {
        data = 0;
        if (8 > UnreadLength)
        {
            return false;
        }

        data = ReadInt64();
        return true;
    }

    public bool TryReadSingle(out float data)
    {
        data = 0;
        if (4 > UnreadLength)
        {
            return false;
        }

        data = ReadSingle();
        return true;
    }

    public bool TryReadString(Encoding encoding, out string data)
    {
        data = string.Empty;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = encoding.GetString(ReadBytes(length), 0, length);
        }

        return true;
    }

    public bool TryReadString(out string data)
    {
        return TryReadString(Encoding.ASCII, out data);
    }

    public bool TryReadInt16Array(out short[]? data)
    {
        data = null;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = new short[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = ReadInt16();
            }
        }
        
        return true;
    }
    
    public bool TryReadInt32Array(out int[]? data)
    {
        data = null;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = new int[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = ReadInt32();
            }
        }
        
        return true;
    }
    
    public bool TryReadInt64Array(out long[]? data)
    {
        data = null;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = new long[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = ReadInt64();
            }
        }
        
        return true;
    }
    
    public bool TryReadSingleArray(out float[]? data)
    {
        data = null;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = new float[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = ReadSingle();
            }
        }
        
        return true;
    }
    
    public bool TryReadStringArray(Encoding encoding, out string[]? data)
    {
        data = null;
        
        if (TryReadInt32(out var length))
        {
            if (length > UnreadLength)
            {
                return false;
            }

            data = new string[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = ReadString(encoding);
            }
        }
        
        return true;
    }

    public bool TryReadStringArray(out string[]? data)
    {
        return TryReadStringArray(Encoding.ASCII, out data);
    }

    public void WriteBytes(IEnumerable<byte> data)
    {
        _buffer.AddRange(data);
    }

    public void WriteInt16(short data)
    {
        var bytes = BitConverter.GetBytes(data);
        _buffer.AddRange(bytes);
    }

    public void WriteInt32(int data)
    {
        var bytes = BitConverter.GetBytes(data);
        _buffer.AddRange(bytes);
    }

    public void WriteInt64(long data)
    {
        var bytes = BitConverter.GetBytes(data);
        _buffer.AddRange(bytes);
    }

    public void WriteSingle(float data)
    {
        var bytes = BitConverter.GetBytes(data);
        _buffer.AddRange(bytes);
    }

    public void WriteString(string? data)
    {
        if (data != null)
        {
            WriteInt32(data.Length);
            _buffer.AddRange(Encoding.ASCII.GetBytes(data));
        }
    }

    public void WriteInt16Array(short[] data)
    {
        WriteInt32(data.Length);
        foreach (var element in data)
        {
            WriteInt16(element);
        }
    }
    
    public void WriteInt32Array(int[] data)
    {
        WriteInt32(data.Length);
        foreach (var element in data)
        {
            WriteInt32(element);
        }
    }
    
    public void WriteInt64Array(long[] data)
    {
        WriteInt32(data.Length);
        foreach (var element in data)
        {
            WriteInt64(element);
        }
    }
    
    public void WriteSingleArray(float[] data)
    {
        WriteInt32(data.Length);
        foreach (var element in data)
        {
            WriteSingle(element);
        }
    }
    
    public void WriteStringArray(string?[] data)
    {
        WriteInt32(data.Length);
        foreach (var element in data)
        {
            WriteString(element);
        }
    }

    public void Clear()
    {
        _buffer.Clear();
        _readPosition = 0;
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Clear();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}