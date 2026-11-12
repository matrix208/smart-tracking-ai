namespace Tracking.Storage.Entities;

public sealed class PositionEntity
{
    public long Id { get; set; }

   public string DeviceId { get; set; } = string.Empty;

    public DeviceEntity? Device { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double Speed { get; set; }

    public double Course { get; set; }

    public bool Valid { get; set; }

    public DateTime DeviceTime { get; set; }

    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
