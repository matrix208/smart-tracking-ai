namespace Tracking.Plugin.Teltonika.Protocol.CRC;

/// <summary>
/// CRC-16/IBM (a.k.a. CRC-16/ARC): poly 0x8005 (reflected 0xA001),
/// init 0x0000, no final XOR. This is the algorithm Teltonika's
/// wiki explicitly names for AVL data packets — NOT the same
/// algorithm GT06 uses (CRC-16/X-25).
/// </summary>
public static class Crc16Ibm
{
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0x0000;

        foreach (byte b in data)
        {
            crc ^= b;

            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        return crc;
    }
}
