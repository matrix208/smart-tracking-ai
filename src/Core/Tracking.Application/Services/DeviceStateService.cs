using Tracking.Application.Interfaces;
using Tracking.Storage.Entities;
using Tracking.Storage.Repositories;

namespace Tracking.Application.Services;

public sealed class DeviceStateService : IDeviceStateService
{
    private readonly IDeviceStateRepository _repository;

    public DeviceStateService(
        IDeviceStateRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<DeviceStateEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<DeviceStateEntity?> GetByDeviceIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByDeviceIdAsync(
            deviceId,
            cancellationToken);
    }
}