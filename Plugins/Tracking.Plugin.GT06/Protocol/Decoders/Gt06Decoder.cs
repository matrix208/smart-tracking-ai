using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class Gt06Decoder
{
    private readonly LoginDecoder _login = new();
    private readonly HeartbeatDecoder _heartbeat = new();
    private readonly GpsDecoder _gps = new();

    public object Decode(ReadOnlyMemory<byte> packet)
    {
        var reader = new BinaryReader(packet);

        // Header
        reader.ReadUInt16BE();

        // Length
        reader.ReadByte();

        // Protocol number
        var protocol = (Gt06MessageType)reader.ReadByte();

return protocol switch
{
    Gt06MessageType.Login => _login.Decode(reader),

    Gt06MessageType.GPS => _gps.Decode(reader),

    Gt06MessageType.Heartbeat => _heartbeat.Decode(reader),

    Gt06MessageType.Status => _heartbeat.Decode(reader),

    (Gt06MessageType)0x21 => _commandAck.Decode(reader),

    _ => throw new NotSupportedException(
        $"GT06 protocol {protocol} (0x{(byte)protocol:X2}) is not supported.")
};
    }
}