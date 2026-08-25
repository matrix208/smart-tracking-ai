namespace Tracking.Application.DTOs;

public sealed class DriverDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string? EmployeeNumber { get; set; }
    public bool Enabled { get; set; }
}

public sealed class DriverRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string? EmployeeNumber { get; set; }
    public bool Enabled { get; set; } = true;
}
