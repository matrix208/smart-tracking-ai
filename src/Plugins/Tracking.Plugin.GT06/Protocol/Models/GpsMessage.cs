namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class GpsMessage
{
    public string? DeviceId { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double Speed { get; init; }

    public DateTime Timestamp { get; init; }
}