using Tracking.Commands.Models;

namespace Tracking.Commands.Services;

public sealed class CommandDispatcher
{
    private readonly CommandService _service;

    public CommandDispatcher(
        CommandService service)
    {
        _service = service;
    }

    public Task SendAsync(
        DeviceCommand command)
    {
        return _service.SendAsync(command);
    }

    public Task RequestPositionAsync(
        string deviceId)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.RequestPosition
            });
    }

    public Task RequestStatusAsync(
        string deviceId)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.RequestStatus
            });
    }

    public Task RelayAsync(
        string deviceId,
        bool enabled)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.RelayControl,
                Parameters = new object[]
                {
                    enabled
                }
            });
    }

    public Task OutputAsync(
        string deviceId,
        int output,
        bool enabled)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.OutputControl,
                Parameters = new object[]
                {
                    output,
                    enabled
                }
            });
    }

    public Task RebootAsync(
        string deviceId)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.Reboot
            });
    }

    public Task TimeSyncAsync(
        string deviceId)
    {
        return SendAsync(
            new DeviceCommand
            {
                DeviceId = deviceId,
                Type = CommandType.TimeSync
            });
    }
}