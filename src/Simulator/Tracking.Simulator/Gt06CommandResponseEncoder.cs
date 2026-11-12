using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.CRC;

namespace Tracking.Simulator;

public static class Gt06CommandResponseEncoder
{
    public static byte[] BuildCommandResponse(
        ushort serverFlag,
        string result = "OK")
    {
        using var ms = new MemoryStream();

        // Header
        ms.WriteByte(0x78);
        ms.WriteByte(0x78);

        // Length
        // Protocol + Flag(2) + ASCII + Serial(2) + CRC(2)
        byte length =
            (byte)(1 + 2 + result.Length + 2 + 2);

        ms.WriteByte(length);

        // Protocol Number (Information Transmission)
        ms.WriteByte(0x80);

        // Server Flag
        Span<byte> flag = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(flag, serverFlag);
        ms.Write(flag);

        // ASCII Result
        foreach (char c in result)
            ms.WriteByte((byte)c);

        // Serial Number (always 1)
        Span<byte> serial = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(serial, 1);
        ms.Write(serial);

        // CRC
        ushort crc =
            Crc16.Compute(ms.ToArray()[2..]);

        Span<byte> crcBytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(crcBytes, crc);
        ms.Write(crcBytes);

        // Tail
        ms.WriteByte(0x0D);
        ms.WriteByte(0x0A);

        return ms.ToArray();
    }
}