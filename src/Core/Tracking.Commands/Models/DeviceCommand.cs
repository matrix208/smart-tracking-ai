namespace Tracking.Commands.Models;

public sealed class DeviceCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string DeviceId { get; init; }

    public required CommandType Type { get; init; }

    public byte[]? Data { get; set; }

    public object[] Parameters { get; init; } = [];

    public DateTime CreatedAt { get; init; } =
        DateTime.UtcNow;

    public TimeSpan Timeout { get; init; } =
        TimeSpan.FromSeconds(30);

    public bool RequiresResponse { get; init; } = true;

    /// <summary>
    /// Server Flag الخاص ببروتوكول GT06.
    /// يتم توليده قبل الإرسال ويستخدم لمطابقة الرد.
    /// </summary>
    public uint ServerFlag { get; set; }
}