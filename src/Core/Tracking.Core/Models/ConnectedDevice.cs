using Tracking.SDK.Models;

namespace Tracking.Core.Models;

public sealed class ConnectedDevice
{
    public required string Imei { get; init; }

    public string ConnectionId { get; set; } = string.Empty;

    public bool Online { get; set; }

    public DateTime ConnectedAt { get; set; }

    public DateTime LastSeen { get; set; }

    public Position? LastPosition { get; set; }

    public int PacketCount { get; set; }
}