using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface ITripService
{
    Task<IReadOnlyList<TripDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TripDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TripDto> CreateAsync(
        TripRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TripDto?> UpdateAsync(
        long id,
        TripRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TripDto?> StartAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TripDto?> CompleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TripDto?> CancelAsync(
        long id,
        CancellationToken cancellationToken = default);
}
