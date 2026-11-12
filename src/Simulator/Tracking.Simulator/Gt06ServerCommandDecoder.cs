using System.Text;

public static class Gt06ServerCommandDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> packet,
        out ushort serverFlag,
        out string command)
    {
        serverFlag = 0;
        command = string.Empty;

        if (packet.Length < 13)
            return false;

        if (packet[0] != 0x78 || packet[1] != 0x78)
            return false;

        if (packet[3] != 0x80)
            return false;

        byte length = packet[2];

        // 78 78 LL 80 09 00 00 00 FLAG CMD... SERIAL CRC 0D 0A
// Command Length
int cmdLength = packet[4];

// Server Flag (2 bytes)
serverFlag = (ushort)((packet[5] << 8) | packet[6]);

// Command starts immediately after ServerFlag
int commandStart = 7;

// Ensure we don't read past the packet
if (cmdLength < 2)
    return false;

int commandLength = cmdLength - 2;

        if (commandLength < 0)
            commandLength = 0;

        command = Encoding.ASCII.GetString(
            packet.Slice(commandStart, commandLength));

        return true;
    }
}