using System.Threading.Channels;
using Tracking.SDK.Models;

namespace Tracking.Persistence.Channels;

public sealed class DeviceChannel
{
    private readonly Channel<DeviceInfo> _channel;


    public DeviceChannel()
    {
        _channel =
            Channel.CreateBounded<DeviceInfo>(
                new BoundedChannelOptions(10000)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait,

                    SingleReader = true,

                    SingleWriter = false
                });
    }


    public async ValueTask WriteAsync(
        DeviceInfo device,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(
            device,
            cancellationToken);
    }


    public IAsyncEnumerable<DeviceInfo> ReadAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }


    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}