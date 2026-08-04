using Tracking.SDK.Enums;

namespace Tracking.SDK.Models;

public sealed record DeviceMessage
{
    public MessageType Type { get; init; }

    // رقم الجهاز (IMEI أو أي معرف آخر)
    public string? DeviceId { get; init; }

    // بيانات الموقع
    public Position? Position { get; init; }

    // بيانات الإنذار
    public Alarm? Alarm { get; init; }

    // أي بيانات إضافية (Login, Heartbeat, Command...)
    public object? Payload { get; init; }
}