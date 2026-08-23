namespace Tracking.Storage.Entities;

public sealed class CommandEntity
{
    public long Id { get; set; }

    public string DeviceId { get; set; } = "";

    public string Command { get; set; } = "";

    public uint ServerFlag { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = "";

    public string? Response { get; set; }

    public string? Protocol { get; set; }
}