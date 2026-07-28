namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class AlarmMessage
{
    public string? DeviceId { get; init; }

    public byte AlarmCode { get; init; }

    public DateTime Timestamp { get; init; }
}