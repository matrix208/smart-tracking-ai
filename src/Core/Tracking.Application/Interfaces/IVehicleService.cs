using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<VehicleDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<VehicleDto> CreateAsync(
        VehicleRequestDto request,
        CancellationToken cancellationToken = default);

    Task<VehicleDto?> UpdateAsync(
        long id,
        VehicleRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);
}
