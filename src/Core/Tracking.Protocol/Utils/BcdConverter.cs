namespace Tracking.Protocol.Utils;

public static class BcdConverter
{
    public static int DecodeByte(byte value)
    {
        return ((value >> 4) * 10) + (value & 0x0F);
    }

    public static string Decode(ReadOnlySpan<byte> data)
    {
        Span<char> chars = stackalloc char[data.Length * 2];

        int index = 0;

        foreach (byte b in data)
        {
            chars[index++] = (char)('0' + ((b >> 4) & 0x0F));
            chars[index++] = (char)('0' + (b & 0x0F));
        }

        return new string(chars);
    }
}