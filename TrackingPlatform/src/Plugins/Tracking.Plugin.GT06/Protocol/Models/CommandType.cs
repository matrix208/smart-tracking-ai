namespace Tracking.Commands.Models;

/// <summary>
/// أوامر عامة مستقلة عن البروتوكول.
/// كل Plugin يحولها إلى أوامر البروتوكول الخاص به.
/// </summary>
public enum CommandType
{
    /// <summary>
    /// طلب الموقع الحالي.
    /// </summary>
    RequestPosition,

    /// <summary>
    /// طلب حالة الجهاز.
    /// </summary>
    RequestStatus,

    /// <summary>
    /// التحكم بالريليه.
    /// </summary>
    RelayControl,

    /// <summary>
    /// التحكم بالمخارج الرقمية.
    /// </summary>
    OutputControl,

    /// <summary>
    /// إعادة تشغيل الجهاز.
    /// </summary>
    Reboot,

    /// <summary>
    /// مزامنة الوقت.
    /// </summary>
    TimeSync,

    /// <summary>
    /// أمر خاص بالبروتوكول.
    /// </summary>
    Custom
}