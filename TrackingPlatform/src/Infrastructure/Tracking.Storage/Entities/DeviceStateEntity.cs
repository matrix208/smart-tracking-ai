namespace Tracking.Storage.Entities;

public sealed class DeviceStateEntity
{
    public string DeviceId { get; set; } = string.Empty;

    public DateTime LastUpdate { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Speed { get; set; }

    public double Course { get; set; }

    public bool Online { get; set; }

    public bool Ignition { get; set; }

    public int Satellites { get; set; }

    public double? Battery { get; set; }

    public int? Signal { get; set; }
}