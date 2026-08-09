using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Repositories;

namespace Tracking.Application.Services;

public sealed class PositionService : IPositionService
{
    private readonly IPositionRepository _repository;

    public PositionService(IPositionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PositionDto>> GetLatestAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var positions = await _repository.GetLatestAsync(
            count,
            cancellationToken);

        return positions
            .Select(x => new PositionDto
            {
                DeviceId = x.DeviceId,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Speed = x.Speed,
                Course = x.Course,
                Valid = x.Valid,
                DeviceTime = x.DeviceTime,
                ServerTime = x.ServerTime
            })
            .ToList();
    }
}