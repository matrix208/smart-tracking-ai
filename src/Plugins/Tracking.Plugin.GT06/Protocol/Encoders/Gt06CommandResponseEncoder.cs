using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.CRC;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public static class Gt06CommandResponseEncoder
{
    public static byte[] BuildCommandResponse(
        ushort serial)
    {
        using var ms = new MemoryStream();

        ms.WriteByte(0x78);
        ms.WriteByte(0x78);

        // Length
        ms.WriteByte(0x05);

        // Command response protocol
        ms.WriteByte(0x21);

        Span<byte> buffer = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            buffer,
            serial);

        ms.Write(buffer);

        var data = ms.ToArray()[2..];

        ushort crc = Crc16.Compute(data);

        BinaryPrimitives.WriteUInt16BigEndian(
            buffer,
            crc);

        ms.Write(buffer);

        ms.WriteByte(0x0D);
        ms.WriteByte(0x0A);

        return ms.ToArray();
    }
}