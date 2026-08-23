namespace Tracking.Application.DTOs;

public sealed class DeviceDto
{
    public string Imei { get; set; } = string.Empty;

    public string Protocol { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; }

    public double? LastLatitude { get; set; }

    public double? LastLongitude { get; set; }

    public double? LastSpeed { get; set; }

    public double? LastCourse { get; set; }

    public DeviceModelDto? Model { get; set; }

    public List<PeripheralDto> Peripherals { get; set; } = [];
}