namespace Tracking.Storage.Entities;

public sealed class DeviceModelEntity
{
    public long Id { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Protocol { get; set; } = string.Empty;

    public string? Firmware { get; set; }

    public string? Description { get; set; }

    public List<DeviceEntity> Devices { get; set; } = new();
}