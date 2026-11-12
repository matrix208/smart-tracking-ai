namespace Tracking.Plugin.Teltonika.Protocol.Models;

public sealed class TeltonikaAvlRecord
{
    public DateTime Timestamp { get; init; }

    public byte Priority { get; init; }

    public double Longitude { get; init; }

    public double Latitude { get; init; }

    public double Altitude { get; init; }

    public double Angle { get; init; }

    public byte Satellites { get; init; }

    public double Speed { get; init; }

    /// <summary>
    /// Not an explicit protocol field — approximated from satellite
    /// count (>= 3 is generally considered a usable fix). Teltonika
    /// only guarantees Speed = 0 when GPS data is invalid.
    /// </summary>
    public bool GpsFix => Satellites >= 3;

    public List<TeltonikaIoElement> IoElements { get; init; } = [];
}
