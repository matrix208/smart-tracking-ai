using System.Buffers.Binary;

namespace Tracking.Plugin.Teltonika.Protocol.Encoders;

/// <summary>
/// After an AVL data packet, the server must reply with the number
/// of records it accepted, as a 4-byte big-endian integer.
/// If this doesn't match what the device sent, it resends the batch.
/// </summary>
public sealed class AvlAckEncoder
{
    public ReadOnlyMemory<byte> Encode(uint acceptedRecordCount)
    {
        var buffer = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, acceptedRecordCount);
        return buffer;
    }
}
