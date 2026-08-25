using Tracking.Application.DTOs;

namespace Tracking.Application.Interfaces;

public interface IDriverVehicleAssignmentService
{
    Task<IReadOnlyList<DriverVehicleAssignmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto> CreateAsync(
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto?> UpdateAsync(
        long id,
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto> AssignAsync(
        AssignDriverVehicleRequestDto request,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto?> UnassignAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto?> GetActiveDriverForVehicleAsync(
        long vehicleId,
        CancellationToken cancellationToken = default);

    Task<DriverVehicleAssignmentDto?> GetActiveVehicleForDriverAsync(
        long driverId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DriverVehicleAssignmentDto>> GetVehicleAssignmentsAsync(
        long vehicleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DriverVehicleAssignmentDto>> GetDriverAssignmentsAsync(
        long driverId,
        CancellationToken cancellationToken = default);
}
