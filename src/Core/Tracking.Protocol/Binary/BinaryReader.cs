using System.Buffers.Binary;
using System.Text;

namespace Tracking.Protocol.Binary;

public sealed class BinaryReader
{
    private readonly ReadOnlyMemory<byte> _buffer;

    public int Position { get; private set; }

    public int Length => _buffer.Length;

    public int Remaining => Length - Position;

    public bool EndOfBuffer => Position >= Length;

    public BinaryReader(ReadOnlyMemory<byte> buffer)
    {
        _buffer = buffer;
    }

    public void Seek(int position)
    {
        if (position < 0 || position > Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        Position = position;
    }

    public void Skip(int count)
    {
        Seek(Position + count);
    }

    public byte PeekByte()
    {
        if (Remaining < 1)
            throw new EndOfStreamException();

        return _buffer.Span[Position];
    }

    public byte ReadByte()
    {
        var value = PeekByte();
        Position++;
        return value;
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public ReadOnlySpan<byte> ReadSpan(int length)
    {
        if (Remaining < length)
            throw new EndOfStreamException();

        var span = _buffer.Span.Slice(Position, length);

        Position += length;

        return span;
    }

    public byte[] ReadBytes(int length)
    {
        return ReadSpan(length).ToArray();
    }

    // =====================================================
    // UInt16
    // =====================================================

    public ushort ReadUInt16BigEndian()
    {
        var value = BinaryPrimitives.ReadUInt16BigEndian(ReadSpan(2));
        return value;
    }

    public ushort ReadUInt16LittleEndian()
    {
        var value = BinaryPrimitives.ReadUInt16LittleEndian(ReadSpan(2));
        return value;
    }

    // =====================================================
    // UInt32
    // =====================================================

    public uint ReadUInt32BigEndian()
    {
        var value = BinaryPrimitives.ReadUInt32BigEndian(ReadSpan(4));
        return value;
    }

    public uint ReadUInt32LittleEndian()
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(ReadSpan(4));
        return value;
    }

    // =====================================================
    // Int16
    // =====================================================

    public short ReadInt16BigEndian()
    {
        var value = BinaryPrimitives.ReadInt16BigEndian(ReadSpan(2));
        return value;
    }

    public short ReadInt16LittleEndian()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(ReadSpan(2));
        return value;
    }

    // =====================================================
    // Int32
    // =====================================================

    public int ReadInt32BigEndian()
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(ReadSpan(4));
        return value;
    }

    public int ReadInt32LittleEndian()
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(ReadSpan(4));
        return value;
    }

    // =====================================================
    // Int64
    // =====================================================

    public long ReadInt64BigEndian()
    {
        var value = BinaryPrimitives.ReadInt64BigEndian(ReadSpan(8));
        return value;
    }

    public long ReadInt64LittleEndian()
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(ReadSpan(8));
        return value;
    }

    // =====================================================
    // Strings
    // =====================================================

    public string ReadAscii(int length)
    {
        return Encoding.ASCII.GetString(ReadSpan(length));
    }

    public string ReadUtf8(int length)
    {
        return Encoding.UTF8.GetString(ReadSpan(length));
    }

    // =====================================================
    // Peek
    // =====================================================

    public ushort PeekUInt16BigEndian()
    {
        if (Remaining < 2)
            throw new EndOfStreamException();

        return BinaryPrimitives.ReadUInt16BigEndian(
            _buffer.Span.Slice(Position, 2));
    }

    public ushort PeekUInt16LittleEndian()
    {
        if (Remaining < 2)
            throw new EndOfStreamException();

        return BinaryPrimitives.ReadUInt16LittleEndian(
            _buffer.Span.Slice(Position, 2));
    }

    // =====================================================
    // Slice
    // =====================================================

    public ReadOnlyMemory<byte> Slice(int length)
    {
        if (Remaining < length)
            throw new EndOfStreamException();

        var memory = _buffer.Slice(Position, length);

        Position += length;

        return memory;
    }
    // =====================================================
// Compatibility aliases
// =====================================================

public ushort ReadUInt16BE()
    => ReadUInt16BigEndian();

public ushort ReadUInt16LE()
    => ReadUInt16LittleEndian();

public uint ReadUInt32BE()
    => ReadUInt32BigEndian();

public uint ReadUInt32LE()
    => ReadUInt32LittleEndian();

public short ReadInt16BE()
    => ReadInt16BigEndian();

public short ReadInt16LE()
    => ReadInt16LittleEndian();

public int ReadInt32BE()
    => ReadInt32BigEndian();

public int ReadInt32LE()
    => ReadInt32LittleEndian();

public long ReadInt64BE()
    => ReadInt64BigEndian();

public long ReadInt64LE()
    => ReadInt64LittleEndian();

public ushort PeekUInt16BE()
    => PeekUInt16BigEndian();

public ushort PeekUInt16LE()
    => PeekUInt16LittleEndian();

public string ReadImei()
{
    Span<char> chars = stackalloc char[16];

    for (int i = 0; i < 8; i++)
    {
        byte b = ReadByte();

        chars[i * 2] = (char)('0' + ((b >> 4) & 0x0F));
        chars[i * 2 + 1] = (char)('0' + (b & 0x0F));
    }

    return new string(chars);
}
}