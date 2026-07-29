namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class HeartbeatMessage
{
    public string? DeviceId { get; init; }

    public ushort Serial { get; init; }

    public byte TerminalInfo { get; init; }

    public byte VoltageLevel { get; init; }

    public byte GsmSignal { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}