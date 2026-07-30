using Tracking.Plugin.GT06.Protocol.CRC;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public static class Gt06CommandEncoder
{
    public static byte[] Heartbeat(
        ushort serial)
    {
        Span<byte> buffer = stackalloc byte[10];

        buffer[0] = 0x78;
        buffer[1] = 0x78;

        buffer[2] = 0x05;

        buffer[3] = 0x23;

        buffer[4] = (byte)(serial >> 8);
        buffer[5] = (byte)serial;

        ushort crc =
            Crc16.Compute(
                buffer.Slice(2, 4));

        buffer[6] = (byte)(crc >> 8);
        buffer[7] = (byte)crc;

        buffer[8] = 0x0D;
        buffer[9] = 0x0A;

        return buffer.ToArray();
    }
}