namespace Tracking.Plugin.GT06.Protocol.Protocols;

public static class Gt06Constants
{
    public const ushort HeaderBasic = 0x7878;

    public const ushort HeaderExtended = 0x7979;

    public const ushort Tail = 0x0D0A;

    public const int ImeiLength = 8;

    public const int SerialLength = 2;

    public const int CrcLength = 2;
}