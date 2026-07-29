using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Tracking.Persistence.Channels;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Workers;

public sealed class PositionWriterWorker : BackgroundService
{
    private readonly PositionChannel _channel;
    private readonly IDbContextFactory<TrackingDbContext> _factory;


    public PositionWriterWorker(
        PositionChannel channel,
        IDbContextFactory<TrackingDbContext> factory)
    {
        _channel = channel;
        _factory = factory;
    }



    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "Position Writer Worker Started");



        var batch =
            new List<PositionEntity>();



        await foreach (var position in
            _channel.ReadAllAsync(stoppingToken))
        {
            batch.Add(new PositionEntity
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                Speed = position.Speed,
                Course = position.Course,
                Valid = position.Valid,
                DeviceTime = position.DeviceTime,
                ServerTime = position.ServerTime
            });



            // حفظ دفعات
            if (batch.Count >= 500)
            {
                await SaveBatchAsync(
                    batch,
                    stoppingToken);

                batch.Clear();
            }
        }



        // حفظ أي بيانات متبقية عند الإغلاق
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
        await using var db =
            await _factory.CreateDbContextAsync(
                cancellationToken);



        await db.Positions.AddRangeAsync(
            batch,
            cancellationToken);



        await db.SaveChangesAsync(
            cancellationToken);



        Console.WriteLine(
            $"Saved {batch.Count} positions");
    }
}