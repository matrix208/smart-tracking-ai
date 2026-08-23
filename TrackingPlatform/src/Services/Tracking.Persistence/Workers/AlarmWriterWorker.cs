using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tracking.Persistence.Channels;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Workers;

public sealed class AlarmWriterWorker : BackgroundService
{
    private readonly AlarmChannel _channel;
    private readonly IDbContextFactory<TrackingDbContext> _factory;


    public AlarmWriterWorker(
        AlarmChannel channel,
        IDbContextFactory<TrackingDbContext> factory)
    {
        _channel = channel;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Alarm Writer Worker Started");


        await foreach (var alarm in
            _channel.ReadAllAsync(stoppingToken))
        {
            Console.WriteLine(
    $"[Worker] Alarm Received : {alarm.DeviceId} Code={alarm.AlarmCode}");
            await using var db =
                await _factory.CreateDbContextAsync(
                    stoppingToken);


            var device =
                await db.Devices
                    .FirstOrDefaultAsync(
                        x => x.Imei == alarm.DeviceId,
                        stoppingToken);


            if (device == null)
            {
                Console.WriteLine(
                    $"[Alarm] Device not found : {alarm.DeviceId}");

                continue;
            }

            await db.Alarms.AddAsync(
                new AlarmEntity
                {
                   DeviceId = device.Imei,

                    AlarmCode = alarm.AlarmCode,

                    DeviceTime = alarm.DeviceTime,

                    ServerTime = alarm.ServerTime
                },
                stoppingToken);
            await db.SaveChangesAsync(
                stoppingToken);
            Console.WriteLine(
                $"[DB] Alarm Saved : {alarm.DeviceId}");
        }
    }
}