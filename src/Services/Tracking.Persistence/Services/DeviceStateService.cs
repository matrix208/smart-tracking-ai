using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Persistence.Services;

public sealed class DeviceStateService
{
    private readonly IDbContextFactory<TrackingDbContext> _factory;

    public DeviceStateService(
        IDbContextFactory<TrackingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task UpdatePositionAsync(
        PositionEntity position,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await _factory.CreateDbContextAsync(cancellationToken);

        var state = await db.DeviceStates
            .FirstOrDefaultAsync(
                x => x.DeviceId == position.DeviceId,
                cancellationToken);

        if (state is null)
        {
            state = new DeviceStateEntity
            {
                DeviceId = position.DeviceId
            };

            db.DeviceStates.Add(state);
        }

        state.LastUpdate = position.ServerTime;
        state.Latitude = position.Latitude;
        state.Longitude = position.Longitude;
        state.Speed = position.Speed;
        state.Course = position.Course;
        state.Online = true;

        await db.SaveChangesAsync(cancellationToken);
    }

   public async Task UpdateHeartbeatAsync(
    string deviceId,
    CancellationToken cancellationToken = default)
{
    await using var db =
        await _factory.CreateDbContextAsync(cancellationToken);

    var state = await db.DeviceStates
        .FirstOrDefaultAsync(
            x => x.DeviceId == deviceId,
            cancellationToken);

    if (state is null)
        return;

    state.LastUpdate = DateTime.UtcNow;
    state.Online = true;

    await db.SaveChangesAsync(cancellationToken);
}

public async Task UpdateOfflineAsync(
    string deviceId,
    DateTime lastUpdate,
    CancellationToken cancellationToken = default)
{
    Console.WriteLine(
        $"[DeviceState] OFFLINE requested: {deviceId}");

    await using var db =
        await _factory.CreateDbContextAsync(cancellationToken);

    var state = await db.DeviceStates
        .FirstOrDefaultAsync(
            x => x.DeviceId == deviceId,
            cancellationToken);

    if (state is null)
    {
        Console.WriteLine(
            $"[DeviceState] State NOT FOUND: {deviceId}");

        return;
    }

    Console.WriteLine(
        $"[DeviceState] Before Offline: {deviceId} Online={state.Online}");

    state.LastUpdate = lastUpdate;
    state.Online = false;

    await db.SaveChangesAsync(cancellationToken);

    Console.WriteLine(
        $"[DeviceState] OFFLINE saved: {deviceId}");
}
}