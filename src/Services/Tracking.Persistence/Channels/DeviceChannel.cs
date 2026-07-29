using System.Threading.Channels;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Channels;

public sealed class DeviceChannel
{
    private readonly Channel<DeviceEntity> _channel;


    public DeviceChannel()
    {
        _channel =
            Channel.CreateBounded<DeviceEntity>(
                new BoundedChannelOptions(10000)
                {
                    FullMode =
                        BoundedChannelFullMode.Wait,

                    SingleReader = true,

                    SingleWriter = false
                });
    }



    // إضافة جهاز للطابور
    public async ValueTask WriteAsync(
        DeviceEntity device,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(
            device,
            cancellationToken);
    }



    // قراءة الأجهزة عند وصولها
    public IAsyncEnumerable<DeviceEntity> ReadAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }



    // إغلاق القناة مستقبلاً
    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}