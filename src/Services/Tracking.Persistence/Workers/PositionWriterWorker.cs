using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tracking.Persistence.Channels;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;
using Microsoft.Extensions.Logging;
using Tracking.Persistence.Services;

namespace Tracking.Persistence.Workers;

public sealed class PositionWriterWorker : BackgroundService
{
    private readonly DeviceStateService _deviceStateService;
    private readonly PositionChannel _channel;
    private readonly IDbContextFactory<TrackingDbContext> _factory;
        private readonly ILogger<PositionWriterWorker> _logger;

            public PositionWriterWorker(
            ILogger<PositionWriterWorker> logger,
            PositionChannel channel,
            IDbContextFactory<TrackingDbContext> factory,
            DeviceStateService deviceStateService)
        {
            _logger = logger;
            _channel = channel;
            _factory = factory;
            _deviceStateService = deviceStateService;
        }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Position Writer Worker Started");

        var batch = new List<PositionEntity>();


        await foreach (var position in
            _channel.ReadAllAsync(stoppingToken))
        {
            Console.WriteLine(
                $"Position Received: {position.DeviceId} " +
                $"{position.Latitude},{position.Longitude}");


            batch.Add(new PositionEntity
            {
                DeviceId = position.DeviceId,

                Latitude = position.Latitude,
                Longitude = position.Longitude,

                Speed = position.Speed,
                Course = position.Course,

                Valid = position.Valid,

                DeviceTime = position.DeviceTime,
                ServerTime = position.ServerTime
            });
            // حفظ فوري للاختبار
            if (batch.Count >= 1)
            {
                Console.WriteLine(
                    $"Batch ready: {batch.Count}");

                await SaveBatchAsync(
                    batch,
                    stoppingToken);

                batch.Clear();
            }
        }
        // حفظ المتبقي عند الإغلاق
        if (batch.Count > 0)
        {
            await SaveBatchAsync(
                batch,
                stoppingToken);
        }
    }

 private async Task SaveBatchAsync(
    List<PositionEntity> batch,
    CancellationToken cancellationToken)
{
    try
    {
        Console.WriteLine($"Saving batch now: {batch.Count}");

        await using var db =
            await _factory.CreateDbContextAsync(cancellationToken);

        // التأكد أن الجهاز موجود
        foreach (var position in batch)
        {
            var deviceExists = await db.Devices.AnyAsync(
                x => x.Imei == position.DeviceId,
                cancellationToken);

            if (!deviceExists)
            {
                Console.WriteLine($"Device not found: {position.DeviceId}");
                return;
            }
        }

        // حفظ المواقع
        await db.Positions.AddRangeAsync(batch, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        // تحديث آخر حالة للجهاز
        foreach (var position in batch)
        {
            await _deviceStateService.UpdatePositionAsync(
                position,
                cancellationToken);
        }

        Console.WriteLine($"Saved {batch.Count} positions");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error saving position batch.");
    }
}


    }