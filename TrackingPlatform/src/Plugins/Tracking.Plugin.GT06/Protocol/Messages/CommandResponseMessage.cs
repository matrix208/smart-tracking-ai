namespace Tracking.Plugin.GT06.Protocol.Messages;

public sealed class CommandResponseMessage
{
    /// <summary>
    /// رقم الأمر الذي أرسله السيرفر (Server Flag / Serial).
    /// </summary>
    public ushort ServerFlag { get; init; }

    /// <summary>
    /// هل نفذ الجهاز الأمر بنجاح؟
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// النص الذي أعاده الجهاز.
    /// </summary>
    public string Text { get; init; } = string.Empty;
}