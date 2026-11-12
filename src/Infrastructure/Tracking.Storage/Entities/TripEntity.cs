namespace Tracking.Storage.Entities;

public sealed class TripEntity
{
    public long Id { get; set; }

    public string TripNumber { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long DriverVehicleAssignmentId { get; set; }

    public DriverVehicleAssignmentEntity DriverVehicleAssignment { get; set; } = null!;

    public long DriverId { get; set; }

    public DriverEntity Driver { get; set; } = null!;

    public long VehicleId { get; set; }

    public VehicleEntity Vehicle { get; set; } = null!;

    public string? StartLocation { get; set; }

    public string? EndLocation { get; set; }

    public DateTime ScheduledStartAt { get; set; }

    public DateTime? ScheduledEndAt { get; set; }

    public DateTime? ActualStartAt { get; set; }

    public DateTime? ActualEndAt { get; set; }

    public string Status { get; set; } = "Draft";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
