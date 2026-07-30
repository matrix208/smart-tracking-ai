using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class LoginEncoder
{
    public ReadOnlyMemory<byte> Encode(
        ushort serial)
    {
        return Gt06PacketBuilder.Build(
            Gt06MessageType.Login,
            ReadOnlySpan<byte>.Empty,
            serial);
    }
}