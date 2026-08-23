using System.Threading.Channels;
using Tracking.Commands.Models;

namespace Tracking.Commands.Channels;

public sealed class CommandChannel
{
    private readonly Channel<DeviceCommand> _channel =
        Channel.CreateUnbounded<DeviceCommand>();

    public ValueTask WriteAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(
            command,
            cancellationToken);
    }

    public IAsyncEnumerable<DeviceCommand> ReadAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }
}