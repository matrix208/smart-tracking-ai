namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class GpsMessage
{
   public string DeviceId { get; set; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double Speed { get; init; }

    public double Course { get; init; }

    public bool GpsFix { get; init; }

    public DateTime Timestamp { get; init; }
}