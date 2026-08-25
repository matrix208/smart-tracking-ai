namespace Tracking.Storage.Entities;

public sealed class DriverEntity
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? LicenseNumber { get; set; }

    public string? EmployeeNumber { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
