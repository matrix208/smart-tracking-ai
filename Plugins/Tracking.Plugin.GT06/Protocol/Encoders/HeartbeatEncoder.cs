using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.CRC;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class HeartbeatEncoder
{
    public ReadOnlyMemory<byte> Encode(ushort serial)
    {
        using var stream = new MemoryStream();

        // Header
        Span<byte> header = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            header,
            Gt06Constants.HeaderBasic);

        stream.Write(header);

        // Length
        // Protocol(1) + Serial(2) + CRC(2) + Tail(2)
        stream.WriteByte(0x05);

        // Protocol
        stream.WriteByte((byte)Gt06MessageType.Heartbeat);

        // Serial
        Span<byte> serialBytes = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            serialBytes,
            serial);

        stream.Write(serialBytes);

        // CRC
        var crcData = stream.ToArray()[2..];

        ushort crc = Crc16.Compute(crcData);

        Span<byte> crcBytes = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            crcBytes,
            crc);

        stream.Write(crcBytes);

        // Tail
        Span<byte> tail = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            tail,
            Gt06Constants.Tail);

        stream.Write(tail);

        return stream.ToArray();
    }
}