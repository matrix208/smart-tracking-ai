using System.Buffers.Binary;
using Tracking.Plugin.Teltonika.Protocol.Protocols;

namespace Tracking.Plugin.Teltonika.Protocol.Frames;

/// <summary>
/// Unlike GT06 (fixed 0x7878/0x7979 header on every packet),
/// Teltonika sends two structurally different packet shapes on
/// the same TCP session:
///
///   1) IMEI handshake  — ONLY the very first packet:
///        [UInt16 length][ASCII digits]  e.g. 000F 333536...
///
///   2) AVL data packet — every packet after that:
///        [0x00000000 preamble][UInt32 length][codec id]...
///
/// There is no shared magic byte between them, so both shapes
/// must be detected independently.
/// </summary>
public static class TeltonikaPacketInspector
{
    public static bool IsImeiHandshake(ReadOnlySpan<byte> packet, out int imeiLength)
    {
        imeiLength = 0;

        if (packet.Length < 2 + TeltonikaConstants.MinImeiLength)
            return false;

        var declaredLength = BinaryPrimitives.ReadUInt16BigEndian(packet);

        if (declaredLength < TeltonikaConstants.MinImeiLength ||
            declaredLength > TeltonikaConstants.MaxImeiLength)
            return false;

        if (packet.Length != 2 + declaredLength)
            return false;

        var body = packet.Slice(2, declaredLength);

        foreach (var b in body)
        {
            if (b < (byte)'0' || b > (byte)'9')
                return false;
        }

        imeiLength = declaredLength;
        return true;
    }

    public static bool IsAvlDataPacket(ReadOnlySpan<byte> packet, out byte codecId)
    {
        codecId = 0;

        if (packet.Length < TeltonikaConstants.PreambleLength + 6)
            return false;

        // Preamble must be exactly four zero bytes.
        for (var i = 0; i < TeltonikaConstants.PreambleLength; i++)
        {
            if (packet[i] != 0x00)
                return false;
        }

        var candidateCodec = packet[8];

        if (candidateCodec != TeltonikaConstants.Codec8 &&
            candidateCodec != TeltonikaConstants.Codec8Extended &&
            candidateCodec != TeltonikaConstants.Codec16)
            return false;

        codecId = candidateCodec;
        return true;
    }
}
