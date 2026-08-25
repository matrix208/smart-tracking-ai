namespace Tracking.Application.DTOs;

public sealed class DriverVehicleAssignmentDto
{
    public long Id { get; set; }

    public long DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;

    public long VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class DriverVehicleAssignmentRequestDto
{
    public long DriverId { get; set; }

    public long VehicleId { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}

public sealed class AssignDriverVehicleRequestDto
{
    public long DriverId { get; set; }

    public long VehicleId { get; set; }

    public DateTime? StartAt { get; set; }

    public string? Notes { get; set; }
}
