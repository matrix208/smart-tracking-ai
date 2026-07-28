using Tracking.SDK.Enums;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Core.Services;

public sealed class DeviceManager
{
    private readonly DeviceRegistry _registry;

    public DeviceManager(DeviceRegistry registry)
    {
        _registry = registry;
    }

    public Task ProcessAsync(
        IDeviceSession session,
        DeviceMessage message)
    {
        switch (message.Type)
        {
            case MessageType.Login:

                if (!string.IsNullOrWhiteSpace(message.DeviceId))
                {
                    _registry.Register(
                        message.DeviceId,
                        session.ConnectionId);

                    Console.WriteLine(
                        $"[Registry] Device Registered : {message.DeviceId}");
                }

                break;

            case MessageType.Heartbeat:

                if (!string.IsNullOrWhiteSpace(message.DeviceId))
                {
                    _registry.UpdateHeartbeat(message.DeviceId);
                }

                break;

            case MessageType.Position:

                if (!string.IsNullOrWhiteSpace(message.DeviceId) &&
                    message.Position != null)
                {
                    _registry.UpdatePosition(
                        message.DeviceId,
                        message.Position);
                }

                break;
        }

        return Task.CompletedTask;
    }

    public IEnumerable<Tracking.Core.Models.ConnectedDevice> Devices =>
        _registry.Devices;
}