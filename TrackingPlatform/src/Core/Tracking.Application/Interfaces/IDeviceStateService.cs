using Tracking.Storage.Entities;

namespace Tracking.Application.Interfaces;

public interface IDeviceStateService
{
    Task<IReadOnlyList<DeviceStateEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DeviceStateEntity?> GetByDeviceIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default);
}