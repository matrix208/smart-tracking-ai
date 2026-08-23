using Tracking.Network.Models;

namespace Tracking.Core.Models;

public sealed class PacketContext
{
    public required ClientSession Session { get; init; }

    public required byte[] Packet { get; init; }

    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
}