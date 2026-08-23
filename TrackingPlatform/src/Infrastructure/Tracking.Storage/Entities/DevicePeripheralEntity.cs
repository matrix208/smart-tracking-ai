namespace Tracking.Storage.Entities;

public sealed class DevicePeripheralEntity
{
    public long Id { get; set; }

    public long DeviceId { get; set; }

    public DeviceEntity Device { get; set; } = null!;

    public long PeripheralTypeId { get; set; }

    public PeripheralTypeEntity PeripheralType { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Port { get; set; }

    public bool Enabled { get; set; } = true;

    public string? ConfigurationJson { get; set; }
}