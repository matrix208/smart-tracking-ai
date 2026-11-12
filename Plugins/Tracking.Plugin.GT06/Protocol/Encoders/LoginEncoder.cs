using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.CRC;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class LoginEncoder
{
    public ReadOnlyMemory<byte> Encode(ushort serial)
    {
        using var stream = new MemoryStream();

        // Header
        stream.WriteByte(0x78);
        stream.WriteByte(0x78);

        // Length
        // Protocol(1) + Serial(2) + CRC(2) + Tail(2)
        stream.WriteByte(0x05);

        // Protocol
        stream.WriteByte((byte)Gt06MessageType.Login);

        // Serial
        Span<byte> serialBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(
            serialBytes,
            serial);

        stream.Write(serialBytes);

        // CRC calculated from Length + Protocol + Serial
        var crcData = stream.ToArray()[2..];

        ushort crc = Crc16.Compute(crcData);

        Span<byte> crcBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(
            crcBytes,
            crc);

        stream.Write(crcBytes);

        // Tail
        stream.WriteByte(0x0D);
        stream.WriteByte(0x0A);

        return stream.ToArray();
    }
}