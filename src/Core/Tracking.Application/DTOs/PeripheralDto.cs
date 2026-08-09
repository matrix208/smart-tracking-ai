namespace Tracking.Application.DTOs;

public sealed class PeripheralDto
{
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}