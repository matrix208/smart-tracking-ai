using Tracking.Storage.Entities;

namespace Tracking.Storage.Repositories;

public interface IDeviceRepository
{
    Task<List<DeviceEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DeviceEntity?> GetByImeiAsync(
        string imei,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string imei,
        CancellationToken cancellationToken = default);

    Task<DeviceEntity?> GetDetailsAsync(
        string imei,
        CancellationToken cancellationToken = default);
}