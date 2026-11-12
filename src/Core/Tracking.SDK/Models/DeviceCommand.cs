namespace Tracking.SDK.Models;

public sealed class DeviceCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string DeviceId { get; init; }

    public string Name { get; init; } = string.Empty;

    public Dictionary<string, object> Parameters { get; init; } = new();

    public byte[]? Data { get; set; }

    public uint ServerFlag { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    public bool RequiresResponse { get; init; } = true;
}