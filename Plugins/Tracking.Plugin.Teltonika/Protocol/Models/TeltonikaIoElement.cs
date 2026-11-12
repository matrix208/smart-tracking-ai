using System.Buffers.Binary;

namespace Tracking.Plugin.Teltonika.Protocol.Models;

/// <summary>
/// A single IO property (AVL ID) attached to an AVL record.
/// Stored as raw bytes so it works uniformly for the 1/2/4/8-byte
/// elements AND the variable-length "NX" elements in Codec8 Extended.
/// </summary>
public sealed class TeltonikaIoElement
{
    public required int Id { get; init; }

    public required byte[] RawValue { get; init; }

    public ulong AsUInt64()
    {
        return RawValue.Length switch
        {
            1 => RawValue[0],
            2 => BinaryPrimitives.ReadUInt16BigEndian(RawValue),
            4 => BinaryPrimitives.ReadUInt32BigEndian(RawValue),
            8 => BinaryPrimitives.ReadUInt64BigEndian(RawValue),
            _ => 0
        };
    }
}
