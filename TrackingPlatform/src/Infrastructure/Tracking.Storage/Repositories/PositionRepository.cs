using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Entities;
using Tracking.Storage.Data;

namespace Tracking.Storage.Repositories;

public sealed class PositionRepository : IPositionRepository
{
    private readonly TrackingDbContext _db;

    public PositionRepository(TrackingDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        PositionEntity position,
        CancellationToken cancellationToken = default)
    {
        _db.Positions.Add(position);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PositionEntity?> GetLatestAsync(
        string imei,
        CancellationToken cancellationToken = default)
    {
        return await _db.Positions
            .Where(x => x.DeviceId == imei)
            .OrderByDescending(x => x.ServerTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PositionEntity>> GetLatestAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _db.Positions
            .OrderByDescending(x => x.ServerTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PositionEntity>> GetByDeviceAsync(
        string imei,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<PositionEntity> query =
            _db.Positions.Where(x => x.DeviceId == imei);

        if (from.HasValue)
            query = query.Where(x => x.ServerTime >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.ServerTime <= to.Value);

        return await query
            .OrderByDescending(x => x.ServerTime)
            .ToListAsync(cancellationToken);
    }
}