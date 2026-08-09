using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Storage.Repositories;

public sealed class DeviceStateRepository : IDeviceStateRepository
{
    private readonly TrackingDbContext _db;

    public DeviceStateRepository(
        TrackingDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DeviceStateEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.DeviceStates
            .AsNoTracking()
            .OrderBy(x => x.DeviceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceStateEntity?> GetByDeviceIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        return await _db.DeviceStates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.DeviceId == deviceId,
                cancellationToken);
    }
}