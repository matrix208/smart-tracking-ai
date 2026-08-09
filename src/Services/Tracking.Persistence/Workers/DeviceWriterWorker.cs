using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tracking.Persistence.Channels;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

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
        Console.WriteLine("Device Writer Worker Started");

        await foreach (var device in _channel.ReadAllAsync(stoppingToken))
        {
            await using var db =
                await _factory.CreateDbContextAsync(stoppingToken);

            var existing =
                await db.Devices.FirstOrDefaultAsync(
                    x => x.Imei == device.Imei,
                    stoppingToken);

            if (existing == null)
            {
                existing = new DeviceEntity
                {
                    Imei = device.Imei,
                    Protocol = device.Protocol,
                    IsOnline = device.IsOnline,
                    LastSeen = device.LastSeen,

                    LastLatitude = device.LastLatitude,
                    LastLongitude = device.LastLongitude,
                    LastSpeed = device.LastSpeed,
                    LastCourse = device.LastCourse,
                    LastPositionTime = device.LastPositionTime

                };

                await db.Devices.AddAsync(
                    existing,
                    stoppingToken);
            }
            else
            {
                existing.Protocol = device.Protocol;
                existing.IsOnline = device.IsOnline;
                existing.LastSeen = device.LastSeen;

                // لا نستبدل القيم إذا كانت الرسالة لا تحتوي GPS
                if (device.LastLatitude.HasValue)
                    existing.LastLatitude = device.LastLatitude;

                if (device.LastLongitude.HasValue)
                    existing.LastLongitude = device.LastLongitude;

                if (device.LastSpeed.HasValue)
                    existing.LastSpeed = device.LastSpeed;

                if (device.LastCourse.HasValue)
                    existing.LastCourse = device.LastCourse;

                    if (device.LastPositionTime.HasValue)
                  existing.LastPositionTime = device.LastPositionTime;
            }

            await db.SaveChangesAsync(stoppingToken);

            Console.WriteLine(
                $"[DB] Device Updated : {device.Imei}");
        }
    }
}