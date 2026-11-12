using System.Threading.Channels;
using Tracking.SDK.Models;

namespace Tracking.Persistence.Channels;

public sealed class AlarmChannel
{
    private readonly Channel<Alarm> _channel;


    public AlarmChannel()
    {
        _channel =
            Channel.CreateBounded<Alarm>(
                new BoundedChannelOptions(50000)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait,

                    SingleReader = true,

                    SingleWriter = false
                });
    }


    public async ValueTask WriteAsync(
        Alarm alarm,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(
            alarm,
            cancellationToken);
    }


    public IAsyncEnumerable<Alarm> ReadAllAsync(
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