namespace Tracking.Application.DTOs;

public sealed class VehicleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }
    public string? VehicleType { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Color { get; set; }
    public long? DeviceId { get; set; }
    public bool Enabled { get; set; } = true;
}
