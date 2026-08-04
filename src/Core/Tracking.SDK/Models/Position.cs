namespace Tracking.SDK.Models;

public sealed class Position
{
    /// <summary>
/// رقم الجهاز
/// </summary>
public string DeviceId { get; set; } = string.Empty;
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

    // ==========================
    // Vehicle State
    // ==========================

    /// <summary>
    /// حالة الإشعال
    /// </summary>
    public bool? Ignition { get; set; }

    /// <summary>
    /// هل الجهاز متصل بالطاقة الخارجية؟
    /// </summary>
    public bool? ExternalPower { get; set; }

    /// <summary>
    /// نسبة البطارية
    /// </summary>
    public double? BatteryLevel { get; set; }

    /// <summary>
    /// دقة الموقع (HDOP)
    /// </summary>
    public double? Hdop { get; set; }

    // ==========================
    // Cellular Network
    // ==========================

    /// <summary>
    /// MCC
    /// </summary>
    public int? Mcc { get; set; }

    /// <summary>
    /// MNC
    /// </summary>
    public int? Mnc { get; set; }

    /// <summary>
    /// LAC
    /// </summary>
    public int? Lac { get; set; }

    /// <summary>
    /// Cell ID
    /// </summary>
    public long? CellId { get; set; }

    // ==========================
    // Extra Protocol Data
    // ==========================

    /// <summary>
    /// بيانات إضافية خاصة بكل بروتوكول
    /// </summary>
    public Dictionary<string, object> Attributes { get; } = new();
}