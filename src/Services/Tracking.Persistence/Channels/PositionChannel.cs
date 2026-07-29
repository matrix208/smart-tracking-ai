using System.Threading.Channels;
using Tracking.SDK.Models;

namespace Tracking.Persistence.Channels;

public sealed class PositionChannel
{
    private readonly Channel<Position> _channel;


    public PositionChannel()
    {
        _channel =
            Channel.CreateBounded<Position>(
                new BoundedChannelOptions(50000)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait,

                    SingleReader = true,

                    SingleWriter = false
                });
    }



    // إضافة موقع للطابور
    public async ValueTask WriteAsync(
        Position position,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(
            position,
            cancellationToken);
    }



    // قراءة المواقع عند وصولها
    public IAsyncEnumerable<Position> ReadAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }



    // إغلاق القناة
    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}