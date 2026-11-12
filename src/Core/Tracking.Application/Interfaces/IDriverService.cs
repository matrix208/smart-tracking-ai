using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface IDriverService
{
    Task<IReadOnlyList<DriverDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DriverDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<DriverDto> CreateAsync(
        DriverRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DriverDto?> UpdateAsync(
        long id,
        DriverRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);
}
