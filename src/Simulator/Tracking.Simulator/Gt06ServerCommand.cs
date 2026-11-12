using Tracking.Plugin.GT06.Protocol.Protocols;

internal sealed class Gt06ServerCommand
{
    public Gt06MessageType Type { get; init; }

    public ushort Serial { get; init; }

    public byte[] Payload { get; init; } = [];
}