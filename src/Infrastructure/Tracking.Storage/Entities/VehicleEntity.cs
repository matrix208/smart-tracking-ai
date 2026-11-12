namespace Tracking.Storage.Entities;

public sealed class VehicleEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PlateNumber { get; set; }

    public string? VehicleType { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public string? Color { get; set; }

    public long? DeviceId { get; set; }

    public DeviceEntity? Device { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
