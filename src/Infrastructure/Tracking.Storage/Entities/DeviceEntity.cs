namespace Tracking.Storage.Entities;

public sealed class DeviceEntity
{
    public long Id { get; set; }

    public string Imei { get; set; } = string.Empty;

    public string Protocol { get; set; } = "GT06";

    // آخر اتصال من الجهاز (Heartbeat أو GPS أو Login)
    public DateTime LastSeen { get; set; }

    public bool IsOnline { get; set; }

    // آخر موقع معروف
    public double? LastLatitude { get; set; }

    public double? LastLongitude { get; set; }

    public double? LastSpeed { get; set; }

    public double? LastCourse { get; set; }

    // وقت آخر إحداثية أرسلها الجهاز
    public DateTime? LastPositionTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PositionEntity> Positions { get; set; } = new();
}