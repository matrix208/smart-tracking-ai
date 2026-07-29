using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class HeartbeatDecoder
{
    public HeartbeatMessage Decode(BinaryReader reader)
    {
        // GT06 Heartbeat payload:
        // Terminal information + Voltage + GSM + Serial
        // بعض الأجهزة ترسل Serial فقط

        ushort serial = 0;

        if (reader.Remaining >= 2)
        {
            serial = reader.ReadUInt16BE();
        }

        return new HeartbeatMessage
        {
            Timestamp = DateTime.UtcNow,
            Serial = serial
        };
    }
}