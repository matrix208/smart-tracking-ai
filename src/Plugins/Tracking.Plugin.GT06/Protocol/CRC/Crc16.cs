namespace Tracking.Plugin.GT06.Protocol.CRC;

public static class Crc16
{
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;

        foreach (byte b in data)
        {
            crc ^= b;

            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0x8408;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

return (ushort)(crc ^ 0xFFFF);
    }
}