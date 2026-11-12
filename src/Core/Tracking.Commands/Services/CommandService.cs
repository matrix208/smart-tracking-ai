using Tracking.Commands.Channels;
using Tracking.Commands.Models;

namespace Tracking.Commands.Services;

public sealed class CommandService
{
    private readonly CommandChannel _channel;

    public CommandService(CommandChannel channel)
    {
        _channel = channel;
    }

    public Task SendAsync(
        DeviceCommand command)
    {
        return _channel
            .WriteAsync(command)
            .AsTask();
    }
}