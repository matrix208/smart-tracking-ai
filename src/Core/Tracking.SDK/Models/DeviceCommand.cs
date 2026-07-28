namespace Tracking.SDK.Models;

public sealed class DeviceCommand
{
    public string Name { get; init; } = string.Empty;

    public Dictionary<string, object> Parameters { get; } = new();
}