using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Models;

public sealed class ConnectedDevice
{
    /// <summary>
    /// رقم الجهاز IMEI
    /// </summary>
    public required string Imei { get; init; }


    /// <summary>
    /// جلسة الاتصال الحالية
    /// </summary>
    public IDeviceSession? Session { get; set; }


    /// <summary>
    /// معرف الاتصال
    /// </summary>
    public string ConnectionId =>
        Session?.ConnectionId ?? string.Empty;


    /// <summary>
    /// حالة الاتصال
    /// </summary>
    public bool Online { get; set; }


    /// <summary>
    /// وقت الاتصال
    /// </summary>
    public DateTime ConnectedAt { get; set; }


    /// <summary>
    /// آخر وقت استلام بيانات
    /// </summary>
    public DateTime LastSeen { get; set; }


    /// <summary>
    /// آخر موقع معروف
    /// </summary>
    public Position? LastPosition { get; set; }


    /// <summary>
    /// عدد الحزم المستلمة
    /// </summary>
    public int PacketCount { get; set; }
}