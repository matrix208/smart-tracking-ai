namespace Tracking.Plugin.GT06.Protocol.Frames;

using Tracking.Plugin.GT06.Protocol.Protocols;

public sealed class Gt06Frame
{
    public ushort Header { get; init; }

    public byte Length { get; init; }

    public Gt06MessageType MessageType { get; init; }

    public ReadOnlyMemory<byte> Payload { get; init; }

    public ushort Serial { get; init; }

    public ushort Crc { get; init; }
}