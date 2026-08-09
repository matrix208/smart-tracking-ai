using Microsoft.Extensions.Logging;
using Tracking.Persistence.Channels;
using Tracking.SDK.Enums;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceManager
{
    private readonly ILogger<DeviceManager> _logger;
    private readonly DeviceRegistry _registry;
    private readonly PositionChannel _positionChannel;
    private readonly DeviceChannel _deviceChannel;
    private readonly AlarmChannel _alarmChannel;

    public DeviceManager(
        ILogger<DeviceManager> logger,
        DeviceRegistry registry,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel,
        AlarmChannel alarmChannel)
    {
        _logger = logger;
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

                _logger.LogDebug(
                    "Session={Session} Protocol={Protocol}",
                    session.GetHashCode(),
                    session.ProtocolId);

                await _registry.ReplaceSessionAsync(
                    message.DeviceId,
                    session);

                _registry.Touch(
                    message.DeviceId);

                try
                {
                    await UpdateDeviceAsync(
                        session,
                        message.DeviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update device state for IMEI {Imei}",
                        message.DeviceId);
                }

                _logger.LogInformation(
                    "Device registered. IMEI: {Imei}",
                    message.DeviceId);

                break;
            }

            case MessageType.Heartbeat:
            {
                if (string.IsNullOrWhiteSpace(message.DeviceId))
                    break;

                _registry.UpdateHeartbeat(
                    message.DeviceId);

                try
                {
                    await UpdateDeviceAsync(
                        session,
                        message.DeviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update device state for IMEI {Imei}",
                        message.DeviceId);
                }

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

                try
                {
                    await _positionChannel.WriteAsync(
                        message.Position);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to queue position for IMEI {Imei}",
                        message.DeviceId);

                    return;
                }

                try
                {
                    await UpdateDeviceAsync(
                        session,
                        message.DeviceId,
                        message.Position);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update device state for IMEI {Imei}",
                        message.DeviceId);
                }

                _logger.LogDebug(
                    "Position queued for IMEI {Imei}",
                    message.DeviceId);

                break;
            }

            case MessageType.Alarm:
            {
                if (message.Alarm == null ||
                    string.IsNullOrWhiteSpace(message.DeviceId))
                    break;

                message.Alarm.DeviceId = message.DeviceId;

                _registry.Touch(
                    message.DeviceId);

                try
                {
                    await _alarmChannel.WriteAsync(
                        message.Alarm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to queue alarm for IMEI {Imei}",
                        message.DeviceId);

                    return;
                }

                try
                {
                    await UpdateDeviceAsync(
                        session,
                        message.DeviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update device state for IMEI {Imei}",
                        message.DeviceId);
                }

                _logger.LogDebug(
                    "Alarm queued for IMEI {Imei}",
                    message.DeviceId);

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
                LastCourse = position?.Course,
                LastPositionTime = position?.DeviceTime
            });
    }

    public IEnumerable<Tracking.Core.Models.ConnectedDevice> Devices =>
        _registry.Devices;
}