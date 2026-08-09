namespace Tracking.SDK.Models;

public sealed class DeviceInfo
{
    public string Imei { get; set; } = string.Empty;

    public string Protocol { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    // آخر موقع معروف
    public double? LastLatitude { get; set; }

    public double? LastLongitude { get; set; }

    public double? LastSpeed { get; set; }

    public double? LastCourse { get; set; }
    public DateTime? LastPositionTime { get; set; }
}