using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.CRC;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

internal static class Gt06PacketBuilder
{
    public static ReadOnlyMemory<byte> Build(
        Gt06MessageType type,
        ReadOnlySpan<byte> payload,
        ushort serial)
    {
        using var stream = new MemoryStream();

        // Header
        Span<byte> header = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(
            header,
            Gt06Constants.HeaderBasic);

        stream.Write(header);

        // Length
        // Protocol + Payload + Serial + CRC
        byte length = (byte)(
            1 +
            payload.Length +
            2 +
            2);

        stream.WriteByte(length);

        // Protocol
        stream.WriteByte((byte)type);

        // Payload
        stream.Write(payload);

        // Serial
        Span<byte> serialBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(
            serialBytes,
            serial);

        stream.Write(serialBytes);

        // CRC (من Length وحتى Serial)
        byte[] packet = stream.ToArray();

        ushort crc = Crc16.Compute(
            packet.AsSpan(2));

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