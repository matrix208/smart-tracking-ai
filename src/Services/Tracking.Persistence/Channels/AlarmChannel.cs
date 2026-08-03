using System.Threading.Channels;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Channels;

public sealed class AlarmChannel
{
    private readonly Channel<AlarmEntity> _channel;


    public AlarmChannel()
    {
        _channel =
            Channel.CreateBounded<AlarmEntity>(
                new BoundedChannelOptions(50000)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait,

                    SingleReader = true,

                    SingleWriter = false
                });
    }



    // إضافة إنذار للطابور
    public async ValueTask WriteAsync(
        AlarmEntity alarm,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(
            alarm,
            cancellationToken);
    }



    // قراءة الإنذارات عند وصولها
    public IAsyncEnumerable<AlarmEntity> ReadAllAsync(
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