namespace Tracking.SDK.Models;

public sealed class CommandResult
{
    /// <summary>
    /// رقم الأمر (Server Flag).
    /// </summary>
    public uint ServerFlag { get; init; }

    /// <summary>
    /// IMEI الجهاز.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// هل نفذ الجهاز الأمر بنجاح؟
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// النص الذي أعاده الجهاز.
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// وقت استلام الرد.
    /// </summary>
    public DateTime ReceivedAt { get; init; }
        = DateTime.UtcNow;
}