using Tracking.SDK.Enums;

namespace Tracking.SDK.Models;

public sealed record DeviceMessage
{
    public MessageType Type { get; init; }

    // رقم الجهاز (IMEI أو أي معرف آخر)
    public string? DeviceId { get; init; }

    // بيانات الموقع إن كانت الرسالة Position
    public Position? Position { get; init; }

    // أي بيانات إضافية (Login, Alarm, Status...)
    public object? Payload { get; init; }
}