namespace Tracking.Plugin.GT06.Protocol.Models;

public sealed class LoginMessage
{
    /// <summary>
    /// Device IMEI.
    /// </summary>
    public string Imei { get; init; } = string.Empty;

    /// <summary>
    /// GT06 packet serial number.
    /// </summary>
    public ushort Serial { get; init; }

    /// <summary>
    /// Time when the login packet was decoded.
    /// </summary>
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Remote endpoint (optional).
    /// </summary>
    public string? RemoteEndpoint { get; set; }
}