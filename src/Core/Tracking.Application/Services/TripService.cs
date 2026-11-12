using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Application.Services;

public sealed class TripService : ITripService
{
    private readonly TrackingDbContext _context;

    public TripService(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TripDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .OrderByDescending(x => x.ScheduledStartAt)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<TripDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .AsNoTracking()
            .Include(x => x.Driver)
            .Include(x => x.Vehicle)
            .Where(x => x.Id == id)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TripDto> CreateAsync(
        TripRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await ValidateAssignmentAsync(
            request.DriverVehicleAssignmentId,
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        var tripNumber = request.TripNumber.Trim();

        var duplicate = await _context.Trips
            .AsNoTracking()
            .AnyAsync(
                x => x.TripNumber == tripNumber,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Trip number already exists.");
        }

        var entity = new TripEntity
        {
            TripNumber = tripNumber,
            Name = Normalize(request.Name),
            Description = Normalize(request.Description),

            DriverVehicleAssignmentId =
                request.DriverVehicleAssignmentId,

            DriverId = request.DriverId,
            VehicleId = request.VehicleId,

            StartLocation = Normalize(request.StartLocation),
            EndLocation = Normalize(request.EndLocation),

            ScheduledStartAt = request.ScheduledStartAt,
            ScheduledEndAt = request.ScheduledEndAt,

            Status = "Draft",

            Notes = Normalize(request.Notes),

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Trips.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            entity.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Trip was created but could not be loaded.");
    }

    public async Task<TripDto?> UpdateAsync(
        long id,
        TripRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var entity = await _context.Trips
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft trips can be edited.");
        }

        await ValidateAssignmentAsync(
            request.DriverVehicleAssignmentId,
            request.DriverId,
            request.VehicleId,
            cancellationToken);

        var tripNumber = request.TripNumber.Trim();

        var duplicate = await _context.Trips
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id != id &&
                    x.TripNumber == tripNumber,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Trip number already exists.");
        }

        entity.TripNumber = tripNumber;
        entity.Name = Normalize(request.Name);
        entity.Description = Normalize(request.Description);

        entity.DriverVehicleAssignmentId =
            request.DriverVehicleAssignmentId;

        entity.DriverId = request.DriverId;
        entity.VehicleId = request.VehicleId;

        entity.StartLocation =
            Normalize(request.StartLocation);

        entity.EndLocation =
            Normalize(request.EndLocation);

        entity.ScheduledStartAt =
            request.ScheduledStartAt;

        entity.ScheduledEndAt =
            request.ScheduledEndAt;

        entity.Notes = Normalize(request.Notes);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Trips
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft trips can be deleted.");
        }

        _context.Trips.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TripDto?> StartAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Trips
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft trips can be started.");
        }

        entity.Status = "InProgress";
        entity.ActualStartAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<TripDto?> CompleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Trips
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != "InProgress")
        {
            throw new InvalidOperationException(
                "Only InProgress trips can be completed.");
        }

        var incompleteTasksExist = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TripId == entity.Id &&
                    x.Status != "Completed" &&
                    x.Status != "Cancelled",
                cancellationToken);

        if (incompleteTasksExist)
        {
            throw new InvalidOperationException(
                "Trip cannot be completed until all tasks are completed or cancelled.");
        }

        entity.Status = "Completed";
        entity.ActualEndAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<TripDto?> CancelAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Trips
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status == "Completed")
        {
            throw new InvalidOperationException(
                "Completed trips cannot be cancelled.");
        }

        if (entity.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                "Trip is already cancelled.");
        }

        entity.Status = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    private async Task ValidateAssignmentAsync(
        long assignmentId,
        long driverId,
        long vehicleId,
        CancellationToken cancellationToken)
    {
        if (assignmentId <= 0)
        {
            throw new ArgumentException(
                "DriverVehicleAssignmentId is required.");
        }

        var assignment = await _context
            .DriverVehicleAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == assignmentId &&
                    x.IsActive &&
                    x.EndAt == null,
                cancellationToken);

        if (assignment is null)
        {
            throw new ArgumentException(
                "Driver vehicle assignment was not found or is not active.");
        }

        if (assignment.DriverId != driverId)
        {
            throw new ArgumentException(
                "DriverId does not match the selected assignment.");
        }

        if (assignment.VehicleId != vehicleId)
        {
            throw new ArgumentException(
                "VehicleId does not match the selected assignment.");
        }

        var driverExists = await _context.Drivers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == driverId &&
                    x.Enabled,
                cancellationToken);

        if (!driverExists)
        {
            throw new ArgumentException(
                "Driver not found or disabled.");
        }

        var vehicleExists = await _context.Vehicles
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == vehicleId &&
                    x.Enabled,
                cancellationToken);

        if (!vehicleExists)
        {
            throw new ArgumentException(
                "Vehicle not found or disabled.");
        }
    }

    private static void ValidateRequest(
        TripRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(
                request.TripNumber))
        {
            throw new ArgumentException(
                "TripNumber is required.");
        }

        if (request.DriverVehicleAssignmentId <= 0)
        {
            throw new ArgumentException(
                "DriverVehicleAssignmentId is required.");
        }

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

        if (request.ScheduledStartAt == default)
        {
            throw new ArgumentException(
                "ScheduledStartAt is required.");
        }

        if (
            request.ScheduledEndAt.HasValue &&
            request.ScheduledEndAt.Value <
            request.ScheduledStartAt)
        {
            throw new ArgumentException(
                "ScheduledEndAt cannot be earlier than ScheduledStartAt.");
        }
    }

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static Expression<
        Func<TripEntity, TripDto>> ToDtoExpression()
    {
        return x => new TripDto
        {
            Id = x.Id,

            TripNumber = x.TripNumber,

            Name = x.Name,

            Description = x.Description,

            DriverVehicleAssignmentId =
                x.DriverVehicleAssignmentId,

            DriverId = x.DriverId,

            DriverName = x.Driver.Name,

            VehicleId = x.VehicleId,

            VehicleName = x.Vehicle.Name,

            PlateNumber = x.Vehicle.PlateNumber,

            StartLocation = x.StartLocation,

            EndLocation = x.EndLocation,

            ScheduledStartAt =
                x.ScheduledStartAt,

            ScheduledEndAt =
                x.ScheduledEndAt,

            ActualStartAt =
                x.ActualStartAt,

            ActualEndAt =
                x.ActualEndAt,

            Status = x.Status,

            Notes = x.Notes,

            CreatedAt = x.CreatedAt,

            UpdatedAt = x.UpdatedAt,

            TaskCount = x.Tasks.Count
        };
    }
}
