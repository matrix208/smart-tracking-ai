namespace Tracking.Plugin.Teltonika.Protocol.Models;

/// <summary>
/// One TCP packet can contain multiple AVL records (a "batch"),
/// most commonly after the device reconnects following a network
/// drop and flushes buffered positions.
/// </summary>
public sealed class TeltonikaAvlDataMessage
{
    public string Imei { get; set; } = string.Empty;

    public byte CodecId { get; init; }

    public required IReadOnlyList<TeltonikaAvlRecord> Records { get; init; }
}
