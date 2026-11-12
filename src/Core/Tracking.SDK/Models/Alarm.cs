namespace Tracking.SDK.Models;

public sealed class Alarm
{
    public string DeviceId { get; set; } = string.Empty;

    public byte AlarmCode { get; set; }

    public DateTime DeviceTime { get; set; }

    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
