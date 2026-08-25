using Microsoft.EntityFrameworkCore;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Application.Services;

public sealed class DriverVehicleAssignmentService
    : IDriverVehicleAssignmentService
{
    private readonly TrackingDbContext _context;

    public DriverVehicleAssignmentService(
        TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DriverVehicleAssignmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.StartAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverVehicleAssignmentDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x => x.Id == id)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DriverVehicleAssignmentDto> CreateAsync(
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await ValidateReferencesAsync(
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        if (request.IsActive)
        {
            await EnsureNoActiveAssignmentAsync(
                request.DriverId,
                request.VehicleId,
                null,
                cancellationToken);
        }

        var entity = new DriverVehicleAssignmentEntity
        {
            DriverId = request.DriverId,
            VehicleId = request.VehicleId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            IsActive = request.IsActive,
            Notes = Normalize(request.Notes),
            CreatedAt = DateTime.UtcNow
        };

        _context.DriverVehicleAssignments.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Assignment was created but could not be loaded.");
    }

    public async Task<DriverVehicleAssignmentDto?> UpdateAsync(
        long id,
        DriverVehicleAssignmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var entity = await _context.DriverVehicleAssignments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        await ValidateReferencesAsync(
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        if (request.IsActive)
        {
            await EnsureNoActiveAssignmentAsync(
                request.DriverId,
                request.VehicleId,
                id,
                cancellationToken);
        }

        entity.DriverId = request.DriverId;
        entity.VehicleId = request.VehicleId;
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.IsActive = request.IsActive;
        entity.Notes = Normalize(request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.DriverVehicleAssignments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.DriverVehicleAssignments.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<DriverVehicleAssignmentDto> AssignAsync(
        AssignDriverVehicleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.DriverId <= 0)
        {
            throw new ArgumentException(
                "DriverId is required.");
        }

        if (request.VehicleId <= 0)
        {
            throw new ArgumentException(
                "VehicleId is required.");
        }

        var startAt = request.StartAt ?? DateTime.UtcNow;

        await ValidateReferencesAsync(
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        await EnsureNoActiveAssignmentAsync(
            request.DriverId,
            request.VehicleId,
            null,
            cancellationToken);

        var entity = new DriverVehicleAssignmentEntity
        {
            DriverId = request.DriverId,
            VehicleId = request.VehicleId,
            StartAt = startAt,
            EndAt = null,
            IsActive = true,
            Notes = Normalize(request.Notes),
            CreatedAt = DateTime.UtcNow
        };

        _context.DriverVehicleAssignments.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Assignment was created but could not be loaded.");
    }

    public async Task<DriverVehicleAssignmentDto?> UnassignAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.DriverVehicleAssignments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.IsActive = false;
        entity.EndAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<DriverVehicleAssignmentDto?>
        GetActiveDriverForVehicleAsync(
            long vehicleId,
            CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x =>
                x.VehicleId == vehicleId &&
                x.IsActive &&
                x.EndAt == null)
            .OrderByDescending(x => x.StartAt)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DriverVehicleAssignmentDto?>
        GetActiveVehicleForDriverAsync(
            long driverId,
            CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x =>
                x.DriverId == driverId &&
                x.IsActive &&
                x.EndAt == null)
            .OrderByDescending(x => x.StartAt)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DriverVehicleAssignmentDto>>
        GetVehicleAssignmentsAsync(
            long vehicleId,
            CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.StartAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DriverVehicleAssignmentDto>>
        GetDriverAssignmentsAsync(
            long driverId,
            CancellationToken cancellationToken = default)
    {
        return await _context.DriverVehicleAssignments
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x => x.DriverId == driverId)
            .OrderByDescending(x => x.StartAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateReferencesAsync(
        long driverId,
        long vehicleId,
        CancellationToken cancellationToken)
    {
        var driverExists = await _context.Drivers
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == driverId && x.Enabled,
                cancellationToken);

        if (!driverExists)
        {
            throw new ArgumentException(
                "Driver not found or disabled.");
        }

        var vehicleExists = await _context.Vehicles
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == vehicleId && x.Enabled,
                cancellationToken);

        if (!vehicleExists)
        {
            throw new ArgumentException(
                "Vehicle not found or disabled.");
        }
    }

    private async Task EnsureNoActiveAssignmentAsync(
        long driverId,
        long vehicleId,
        long? excludeId,
        CancellationToken cancellationToken)
    {
        var driverQuery = _context.DriverVehicleAssignments
            .AsNoTracking()
            .Where(x =>
                x.DriverId == driverId &&
                x.IsActive &&
                x.EndAt == null);

        if (excludeId.HasValue)
        {
            driverQuery = driverQuery.Where(
                x => x.Id != excludeId.Value);
        }

        if (await driverQuery.AnyAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Driver already has an active vehicle assignment.");
        }

        var vehicleQuery = _context.DriverVehicleAssignments
            .AsNoTracking()
            .Where(x =>
                x.VehicleId == vehicleId &&
                x.IsActive &&
                x.EndAt == null);

        if (excludeId.HasValue)
        {
            vehicleQuery = vehicleQuery.Where(
                x => x.Id != excludeId.Value);
        }

        if (await vehicleQuery.AnyAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Vehicle already has an active driver assignment.");
        }
    }

    private static void ValidateRequest(
        DriverVehicleAssignmentRequestDto request)
    {
        if (request.DriverId <= 0)
        {
            throw new ArgumentException(
                "DriverId is required.");
        }

        if (request.VehicleId <= 0)
        {
            throw new ArgumentException(
                "VehicleId is required.");
        }

        if (request.StartAt == default)
        {
            throw new ArgumentException(
                "StartAt is required.");
        }

        if (request.EndAt.HasValue &&
            request.EndAt.Value < request.StartAt)
        {
            throw new ArgumentException(
                "EndAt cannot be earlier than StartAt.");
        }

        if (request.IsActive && request.EndAt.HasValue)
        {
            throw new ArgumentException(
                "An active assignment cannot have EndAt.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static System.Linq.Expressions.Expression<
        Func<DriverVehicleAssignmentEntity,
            DriverVehicleAssignmentDto>> ToDtoExpression()
    {
        return x => new DriverVehicleAssignmentDto
        {
            Id = x.Id,

            DriverId = x.DriverId,
            DriverName = x.Driver.Name,

            VehicleId = x.VehicleId,
            VehicleName = x.Vehicle.Name,
            PlateNumber = x.Vehicle.PlateNumber,

            StartAt = x.StartAt,
            EndAt = x.EndAt,

            IsActive = x.IsActive,

            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        };
    }
}
