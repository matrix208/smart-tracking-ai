namespace Tracking.Storage.Entities;

public sealed class DeviceEntity
{
    public long Id { get; set; }

    public string Imei { get; set; } = string.Empty;

    public string Protocol { get; set; } = "GT06";

    public DateTime LastSeen { get; set; }

    public bool Online { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PositionEntity> Positions { get; set; } = new();
}
