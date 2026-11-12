using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class HeartbeatEncoder
{
    public ReadOnlyMemory<byte> Encode(
        ushort serial)
    {
        return Gt06PacketBuilder.Build(
            Gt06MessageType.Heartbeat,
            ReadOnlySpan<byte>.Empty,
            serial);
    }
}