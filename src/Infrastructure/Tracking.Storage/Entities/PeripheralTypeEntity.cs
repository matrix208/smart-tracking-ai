namespace Tracking.Storage.Entities;

public sealed class PeripheralTypeEntity
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Unit { get; set; }

    public List<DevicePeripheralEntity> Peripherals { get; set; } = new();
}