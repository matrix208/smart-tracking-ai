using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface IPositionService
{
    Task<IReadOnlyList<PositionDto>> GetLatestAsync(
        int count,
        CancellationToken cancellationToken = default);
}