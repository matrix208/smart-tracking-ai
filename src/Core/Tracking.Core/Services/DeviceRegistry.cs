using System.Collections.Concurrent;
using Tracking.Core.Models;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceRegistry
{
    private readonly ConcurrentDictionary<string, ConnectedDevice> _devices = new();

    // جميع الأجهزة المسجلة
    public IEnumerable<ConnectedDevice> Devices => _devices.Values;

    // تسجيل جهاز أو تحديث اتصاله
    public ConnectedDevice Register(string imei, string connectionId)
    {
        return _devices.AddOrUpdate(
            imei,
            _ => new ConnectedDevice
            {
                Imei = imei,
                ConnectionId = connectionId,
                Online = true,
                ConnectedAt = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.ConnectionId = connectionId;
                existing.Online = true;
                existing.LastSeen = DateTime.UtcNow;
                return existing;
            });
    }

    // البحث عن جهاز
    public bool TryGet(string imei, out ConnectedDevice? device)
    {
        return _devices.TryGetValue(imei, out device);
    }

    // تحديث آخر Heartbeat
    public void UpdateHeartbeat(string imei)
    {
        if (_devices.TryGetValue(imei, out var device))
        {
            device.LastSeen = DateTime.UtcNow;
        }
    }

    // تحديث آخر موقع
    public void UpdatePosition(string imei, Position position)
    {
        if (_devices.TryGetValue(imei, out var device))
        {
            device.LastPosition = position;
            device.LastSeen = DateTime.UtcNow;
            device.PacketCount++;
        }
    }

    // فصل الجهاز
    public void Disconnect(string imei)
    {
        if (_devices.TryGetValue(imei, out var device))
        {
            device.Online = false;
            device.LastSeen = DateTime.UtcNow;
        }
    }
}