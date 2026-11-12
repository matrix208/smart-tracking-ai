using Tracking.Storage.Entities;

namespace Tracking.Storage.Repositories;

public interface IPositionRepository
{
    Task AddAsync(PositionEntity position, CancellationToken cancellationToken = default);

    Task<PositionEntity?> GetLatestAsync(
        string imei,
        CancellationToken cancellationToken = default);

    Task<List<PositionEntity>> GetLatestAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<List<PositionEntity>> GetByDeviceAsync(
        string imei,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}