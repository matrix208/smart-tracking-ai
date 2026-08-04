using System.Collections.Concurrent;
using Tracking.Core.Models;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceRegistry
{
    private readonly ConcurrentDictionary<string, ConnectedDevice> _devices = new();


    // جميع الأجهزة المسجلة
    public IEnumerable<ConnectedDevice> Devices =>
        _devices.Values;



    // تسجيل جهاز أو تحديث جلسة الاتصال
    public ConnectedDevice Register(
        string imei,
        IDeviceSession session)
    {
        return _devices.AddOrUpdate(
            imei,

            // جهاز جديد
            _ => new ConnectedDevice
            {
                Imei = imei,
                Session = session,
                IsOnline = true,
                ConnectedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            },


            // جهاز موجود
            (_, existing) =>
            {
                existing.Session = session;
                existing.IsOnline = true;
                existing.LastSeen = DateTime.UtcNow;

                return existing;
            });
    }



    // البحث عن جهاز
    public bool TryGet(
        string imei,
        out ConnectedDevice? device)
    {
        return _devices.TryGetValue(
            imei,
            out device);
    }



    // تحديث آخر Heartbeat
    public void UpdateHeartbeat(
        string imei)
    {
        if (_devices.TryGetValue(
            imei,
            out var device))
        {
            device.LastSeen = DateTime.UtcNow;
        }
    }



    // تحديث آخر موقع
    public void UpdatePosition(
        string imei,
        Position position)
    {
        if (_devices.TryGetValue(
            imei,
            out var device))
        {
            device.LastPosition = position;
            device.LastSeen = DateTime.UtcNow;
            device.PacketCount++;
        }
    }



    // إرسال أمر للجهاز
    public async Task<bool> SendAsync(
        string imei,
        ReadOnlyMemory<byte> data)
    {
        if (!_devices.TryGetValue(
            imei,
            out var device))
            return false;


        if (!device.IsOnline)
            return false;


        if (device.Session == null)
            return false;



        await device.Session.SendAsync(
            data);


        return true;
    }



    // فصل الجهاز
    public void Disconnect(
        string imei)
    {
        if (_devices.TryGetValue(
            imei,
            out var device))
        {
            device.IsOnline = false;
            device.LastSeen = DateTime.UtcNow;
            device.Session = null;
        }
    }



    // استبدال جلسة الجهاز عند إعادة الاتصال
    public async Task ReplaceSessionAsync(
        string imei,
        IDeviceSession newSession)
    {
        if (_devices.TryGetValue(
            imei,
            out var existing))
        {
            if (existing.Session != null &&
                existing.Session != newSession)
            {
                try
                {
                    
                    await existing.Session.CloseAsync();
                }
                catch
                {
                }
            }



            existing.Session = newSession;
            existing.IsOnline = true;
            existing.LastSeen = DateTime.UtcNow;

            return;
        }



        _devices.TryAdd(
            imei,
            new ConnectedDevice
            {
                Imei = imei,
                Session = newSession,
                IsOnline = true,
                ConnectedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            });
    }
}