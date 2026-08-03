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



        var batch =
            new List<AlarmEntity>();



        await foreach (var alarm in
            _channel.ReadAllAsync(stoppingToken))
        {
            batch.Add(alarm);



            if (batch.Count >= 500)
            {
                await SaveBatchAsync(
                    batch,
                    stoppingToken);

                batch.Clear();
            }
        }



        if (batch.Count > 0)
        {
            await SaveBatchAsync(
                batch,
                stoppingToken);
        }
    }



    private async Task SaveBatchAsync(
        List<AlarmEntity> batch,
        CancellationToken cancellationToken)
    {
        await using var db =
            await _factory.CreateDbContextAsync(
                cancellationToken);



        await db.Alarms.AddRangeAsync(
            batch,
            cancellationToken);



        await db.SaveChangesAsync(
            cancellationToken);



        Console.WriteLine(
            $"Saved {batch.Count} alarms");
    }
}