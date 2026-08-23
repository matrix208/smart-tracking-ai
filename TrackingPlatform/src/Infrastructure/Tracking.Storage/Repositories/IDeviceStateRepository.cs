using Tracking.Storage.Entities;

namespace Tracking.Storage.Repositories;

public interface IDeviceStateRepository
{
    Task<IReadOnlyList<DeviceStateEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DeviceStateEntity?> GetByDeviceIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default);
}