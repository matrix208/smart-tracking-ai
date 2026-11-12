namespace Tracking.Plugin.Teltonika.Protocol.Protocols;

public static class TeltonikaConstants
{
    // AVL data packets always start with 4 zero bytes (preamble).
    public const int PreambleLength = 4;

    public const byte Codec8 = 0x08;
    public const byte Codec8Extended = 0x8E;
    public const byte Codec16 = 0x10;

    // Server response to the IMEI handshake: single byte.
    public const byte ImeiAccept = 0x01;
    public const byte ImeiReject = 0x00;

    // Valid IMEI text length range (usually 15 digits).
    public const int MinImeiLength = 10;
    public const int MaxImeiLength = 17;
}
