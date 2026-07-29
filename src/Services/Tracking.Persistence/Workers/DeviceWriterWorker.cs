using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tracking.Persistence.Channels;
using Tracking.Storage.Data;

namespace Tracking.Persistence.Workers;

public sealed class DeviceWriterWorker : BackgroundService
{
    private readonly DeviceChannel _channel;
    private readonly IDbContextFactory<TrackingDbContext> _factory;


    public DeviceWriterWorker(
        DeviceChannel channel,
        IDbContextFactory<TrackingDbContext> factory)
    {
        _channel = channel;
        _factory = factory;
    }



    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Device Writer Worker Started");



        await foreach (var device in _channel.ReadAllAsync(stoppingToken))
        {
            await using var db =
                await _factory.CreateDbContextAsync(
                    stoppingToken);



            var existing =
                await db.Devices
                    .FirstOrDefaultAsync(
                        x => x.Imei == device.Imei,
                        stoppingToken);



            if (existing == null)
            {
                await db.Devices.AddAsync(
                    device,
                    stoppingToken);
            }
            else
            {
                existing.Protocol =
                    device.Protocol;

                existing.Online =
                    device.Online;

                existing.LastSeen =
                    device.LastSeen;
            }



            await db.SaveChangesAsync(
                stoppingToken);



            Console.WriteLine(
                $"[DB] Device Saved : {device.Imei}");
        }
    }
}