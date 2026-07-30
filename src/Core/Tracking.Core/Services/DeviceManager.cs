using Tracking.Persistence.Channels;
using Tracking.SDK.Enums;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;
using Tracking.Storage.Entities;

namespace Tracking.Core.Services;

public sealed class DeviceManager
{
    private readonly DeviceRegistry _registry;
    private readonly PositionChannel _positionChannel;
    private readonly DeviceChannel _deviceChannel;


    public DeviceManager(
        DeviceRegistry registry,
        PositionChannel positionChannel,
        DeviceChannel deviceChannel)
    {
        _registry = registry;
        _positionChannel = positionChannel;
        _deviceChannel = deviceChannel;
    }



    public async Task ProcessAsync(
        IDeviceSession session,
        DeviceMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Login:

                if (!string.IsNullOrWhiteSpace(message.DeviceId))
                {
                  Console.WriteLine(
    $"MANAGER Session={session.GetHashCode()} Protocol={session.ProtocolId}");
    
                    await _registry.ReplaceSessionAsync(
                        message.DeviceId,
                        session);



                    await _deviceChannel.WriteAsync(
                        new DeviceEntity
                        {
                            Imei = message.DeviceId,
                            Protocol = "GT06",
                            Online = true,
                            LastSeen = DateTime.UtcNow
                        });



                    Console.WriteLine(
                        $"[Registry] Device Registered : {message.DeviceId}");
                }

                break;



            case MessageType.Heartbeat:

                if (!string.IsNullOrWhiteSpace(message.DeviceId))
                {
                    _registry.UpdateHeartbeat(
                        message.DeviceId);



                    await _deviceChannel.WriteAsync(
                        new DeviceEntity
                        {
                            Imei = message.DeviceId,
                            Protocol = "GT06",
                            Online = true,
                            LastSeen = DateTime.UtcNow
                        });
                }

                break;



            case MessageType.Position:

                if (!string.IsNullOrWhiteSpace(message.DeviceId) &&
                    message.Position != null)
                {
                    _registry.UpdatePosition(
                        message.DeviceId,
                        message.Position);



                    message.Position.Imei =
                        message.DeviceId;



                    await _positionChannel.WriteAsync(
                        message.Position);



                    Console.WriteLine(
                        $"[Position] Queued : {message.DeviceId}");
                }

                break;
        }
    }



    public IEnumerable<Tracking.Core.Models.ConnectedDevice> Devices =>
        _registry.Devices;
}