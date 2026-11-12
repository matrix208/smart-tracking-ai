namespace Tracking.Plugin.Teltonika.Protocol.Models;

/// <summary>
/// Teltonika does not send a framed "login packet" like GT06.
/// The very first thing the device sends after connecting is a
/// 2-byte length prefix followed by the IMEI as ASCII digits.
/// </summary>
public sealed class TeltonikaLoginMessage
{
    public string Imei { get; init; } = string.Empty;

    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
}
