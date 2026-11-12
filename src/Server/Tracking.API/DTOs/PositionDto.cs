namespace Tracking.API.DTOs;

public sealed class PositionDto
{
    public string DeviceId { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Speed { get; set; }

    public double Course { get; set; }

    public bool Valid { get; set; }

    public DateTime DeviceTime { get; set; }

    public DateTime ServerTime { get; set; }
}