namespace Tracking.Application.DTOs;

public sealed class TripDto
{
    public long Id { get; set; }

    public string TripNumber { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long DriverVehicleAssignmentId { get; set; }

    public long DriverId { get; set; }

    public string DriverName { get; set; } = string.Empty;

    public long VehicleId { get; set; }

    public string VehicleName { get; set; } = string.Empty;

    public string? PlateNumber { get; set; }

    public string? StartLocation { get; set; }

    public string? EndLocation { get; set; }

    public DateTime ScheduledStartAt { get; set; }

    public DateTime? ScheduledEndAt { get; set; }

    public DateTime? ActualStartAt { get; set; }

    public DateTime? ActualEndAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int TaskCount { get; set; }
}

public sealed class TripRequestDto
{
    public string TripNumber { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long DriverVehicleAssignmentId { get; set; }

    public long DriverId { get; set; }

    public long VehicleId { get; set; }

    public string? StartLocation { get; set; }

    public string? EndLocation { get; set; }

    public DateTime ScheduledStartAt { get; set; }

    public DateTime? ScheduledEndAt { get; set; }

    public string? Notes { get; set; }
}
