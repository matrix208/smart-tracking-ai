namespace Tracking.SDK.Models;

public sealed class Position
{
    /// <summary>
    /// رقم IMEI الخاص بالجهاز
    /// </summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>
    /// وقت تسجيل الموقع في الجهاز
    /// </summary>
    public DateTime DeviceTime { get; set; }

    /// <summary>
    /// وقت استقبال الخادم للبيانات
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// خط العرض
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// السرعة (كم/س)
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// الاتجاه
    /// </summary>
    public double Course { get; set; }

    /// <summary>
    /// الارتفاع
    /// </summary>
    public double Altitude { get; set; }

    /// <summary>
    /// عدد الأقمار الصناعية
    /// </summary>
    public int Satellites { get; set; }

    /// <summary>
    /// هل الموقع صالح؟
    /// </summary>
    public bool Valid { get; set; }

    /// <summary>
    /// بيانات إضافية خاصة بكل بروتوكول
    /// </summary>
    public Dictionary<string, object> Attributes { get; } = new();
}