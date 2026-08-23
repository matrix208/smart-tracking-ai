using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Storage.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly TrackingDbContext _context;

    public DeviceRepository(
        TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeviceEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceEntity?> GetByImeiAsync(
        string imei,
        CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Imei == imei,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string imei,
        CancellationToken cancellationToken = default)
    {
        return await _context.Devices
            .AnyAsync(
                x => x.Imei == imei,
                cancellationToken);
    }
    public async Task<DeviceEntity?> GetDetailsAsync(
    string imei,
    CancellationToken cancellationToken = default)
{
    return await _context.Devices
        .AsNoTracking()
        .Include(x => x.DeviceModel)
        .Include(x => x.Peripherals)
            .ThenInclude(x => x.PeripheralType)
        .FirstOrDefaultAsync(
            x => x.Imei == imei,
            cancellationToken);
}
}