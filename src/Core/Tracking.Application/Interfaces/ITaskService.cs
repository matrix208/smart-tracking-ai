using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskDto>> GetByTripIdAsync(
        long tripId,
        CancellationToken cancellationToken = default);

    Task<TaskDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TaskDto> CreateAsync(
        TaskRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TaskDto?> UpdateAsync(
        long id,
        TaskRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TaskDto?> StartAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TaskDto?> CompleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<TaskDto?> CancelAsync(
        long id,
        CancellationToken cancellationToken = default);
}
