using Tracking.Persistence.Channels;
using Tracking.SDK.Enums;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceManager
{
    private readonly DeviceRegistry _registry;
    private readonly PositionChannel _positionChannel;
    private readonly DeviceChannel _deviceChannel;
    private readonly AlarmChannel _alarmChannel;

    public DeviceManager(
        DeviceRegistry registry,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel,
        AlarmChannel alarmChannel)
    {
        _registry = registry;
        _positionChannel = positionChannel;
        _deviceChannel = deviceChannel;
        _alarmChannel = alarmChannel;
    }

    public async Task ProcessAsync(
        IDeviceSession session,
        DeviceMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Login:
            {
                if (string.IsNullOrWhiteSpace(message.DeviceId))
                    break;

                Console.WriteLine(
                    $"MANAGER Session={session.GetHashCode()} Protocol={session.ProtocolId}");

                await _registry.ReplaceSessionAsync(
                    message.DeviceId,
                    session);

                await UpdateDeviceAsync(
                    session,
                    message.DeviceId);

                Console.WriteLine(
                    $"[Registry] Device Registered : {message.DeviceId}");

                break;
            }

            case MessageType.Heartbeat:
            {
                if (string.IsNullOrWhiteSpace(message.DeviceId))
                    break;

                _registry.UpdateHeartbeat(
                    message.DeviceId);

                await UpdateDeviceAsync(
                    session,
                    message.DeviceId);

                break;
            }

            case MessageType.Position:
            {
                if (string.IsNullOrWhiteSpace(message.DeviceId) ||
                    message.Position == null)
                    break;

                _registry.UpdatePosition(
                    message.DeviceId,
                    message.Position);

                message.Position.DeviceId = message.DeviceId;
                message.Position.Imei = message.DeviceId;

                await _positionChannel.WriteAsync(
                    message.Position);

                await UpdateDeviceAsync(
                    session,
                    message.DeviceId,
                    message.Position);

                Console.WriteLine(
                    $"[Position] Queued : {message.DeviceId}");

                break;
            }

            case MessageType.Alarm:
            {
                if (message.Alarm == null ||
                    string.IsNullOrWhiteSpace(message.DeviceId))
                    break;

                message.Alarm.DeviceId = message.DeviceId;

                await _alarmChannel.WriteAsync(
                    message.Alarm);

                // تحديث آخر اتصال أيضاً
                await UpdateDeviceAsync(
                    session,
                    message.DeviceId);

                Console.WriteLine(
                    $"[Alarm] Queued : {message.DeviceId}");

                break;
            }
        }
    }

    private async Task UpdateDeviceAsync(
        IDeviceSession session,
        string imei,
        Position? position = null)
    {
        await _deviceChannel.WriteAsync(
            new DeviceInfo
            {
                Imei = imei,
                Protocol = session.ProtocolId,
                IsOnline = true,
                LastSeen = DateTime.UtcNow,

                LastLatitude = position?.Latitude,
                LastLongitude = position?.Longitude,
                LastSpeed = position?.Speed,
                LastCourse = position?.Course
            });
    }

    public IEnumerable<Tracking.Core.Models.ConnectedDevice> Devices =>
        _registry.Devices;
}