using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DeviceDto?> GetDetailsAsync(
        string imei,
        CancellationToken cancellationToken = default);
}