namespace Tracking.Storage.Entities;

public sealed class DriverVehicleAssignmentEntity
{
    public long Id { get; set; }

    public long DriverId { get; set; }
    public DriverEntity Driver { get; set; } = null!;

    public long VehicleId { get; set; }
    public VehicleEntity Vehicle { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
