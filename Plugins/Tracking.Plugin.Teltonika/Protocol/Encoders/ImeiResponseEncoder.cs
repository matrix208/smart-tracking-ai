using Tracking.Plugin.Teltonika.Protocol.Protocols;

namespace Tracking.Plugin.Teltonika.Protocol.Encoders;

/// <summary>
/// Reply to the IMEI handshake: a single binary byte.
/// 0x01 = accepted, 0x00 = rejected.
/// </summary>
public sealed class ImeiResponseEncoder
{
    public ReadOnlyMemory<byte> Encode(bool accept)
    {
        return new byte[]
        {
            accept ? TeltonikaConstants.ImeiAccept : TeltonikaConstants.ImeiReject
        };
    }
}
