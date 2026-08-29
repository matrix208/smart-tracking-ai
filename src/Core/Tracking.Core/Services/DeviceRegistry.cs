using System.Collections.Concurrent;
using Tracking.Core.Models;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceRegistry
{
    public void Touch(string imei)
{
    if (_devices.TryGetValue(
        imei,
        out var device))
    {
        device.PacketCount++;
        device.LastSeen = DateTime.UtcNow;
        device.IsOnline = true;
    }
}
    private readonly ConcurrentDictionary<string, ConnectedDevice> _devices = new();

public bool TryGetBySession(
    IDeviceSession session,
    out ConnectedDevice? device)
{
    device = _devices.Values.FirstOrDefault(
        d => d.Session == session);

    return device != null;
}
    // جميع الأجهزة المسجلة
    public IEnumerable<ConnectedDevice> Devices =>
        _devices.Values;

    // تسجيل جهاز أو تحديث جلسة الاتصال


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
    Touch(imei);
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
    }

    Touch(imei);
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
            device.Session = null;
        }
    }

    // فصل Session الحالية فقط
    // يمنع Session قديمة من فصل Session أحدث بعد إعادة الاتصال.
    public bool DisconnectSession(
        IDeviceSession session)
    {
        if (string.IsNullOrWhiteSpace(session.DeviceId))
            return false;

        if (!_devices.TryGetValue(
            session.DeviceId,
            out var device))
            return false;

        if (!ReferenceEquals(
            device.Session,
            session))
            return false;

        device.IsOnline = false;
        device.Session = null;

        return true;
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