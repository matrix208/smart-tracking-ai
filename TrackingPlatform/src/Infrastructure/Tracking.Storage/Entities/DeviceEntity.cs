namespace Tracking.Storage.Entities;

public sealed class DeviceEntity
{
    public long Id { get; set; }

    public string Imei { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Protocol { get; set; } = "GT06";

    public long? DeviceModelId { get; set; }

    public DeviceModelEntity? DeviceModel { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime LastSeen { get; set; }

    public bool IsOnline { get; set; }

    public double? LastLatitude { get; set; }

    public double? LastLongitude { get; set; }

    public double? LastSpeed { get; set; }

    public double? LastCourse { get; set; }

    public DateTime? LastPositionTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PositionEntity> Positions { get; set; } = new();

    public List<DevicePeripheralEntity> Peripherals { get; set; } = new();
}