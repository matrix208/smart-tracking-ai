using Microsoft.EntityFrameworkCore;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Commands.Queries;

public sealed class CommandHistoryService
{
    private readonly TrackingDbContext _db;

    public CommandHistoryService(
        TrackingDbContext db)
    {
        _db = db;
    }

    public async Task<List<CommandEntity>> GetAsync(
        string deviceId,
        int take = 20)
    {
        return await _db.Commands
            .Where(x => x.DeviceId == deviceId)
            .OrderByDescending(x => x.SentAt)
            .Take(take)
            .ToListAsync();
    }
}