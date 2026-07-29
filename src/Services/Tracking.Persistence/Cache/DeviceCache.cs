using System.Collections.Concurrent;

namespace Tracking.Persistence.Cache;

public sealed class DeviceCache
{
    private readonly ConcurrentDictionary<string, long> _devices = new();


    // إضافة جهاز إلى الذاكرة
    public void Set(
        string imei,
        long deviceId)
    {
        _devices[imei] = deviceId;
    }



    // البحث عن رقم الجهاز في قاعدة البيانات
    public bool TryGet(
        string imei,
        out long deviceId)
    {
        return _devices.TryGetValue(
            imei,
            out deviceId);
    }



    // إزالة جهاز من الذاكرة
    public void Remove(
        string imei)
    {
        _devices.TryRemove(
            imei,
            out _);
    }



    // عدد الأجهزة المخزنة في الذاكرة
    public int Count =>
        _devices.Count;
}