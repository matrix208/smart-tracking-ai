namespace Tracking.Storage.Entities;

public sealed class AlarmEntity
{
    public long Id { get; set; }

    public string DeviceId { get; set; } = string.Empty;

    public DeviceEntity? Device { get; set; }

    public byte AlarmCode { get; set; }

    public DateTime DeviceTime { get; set; }

    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
