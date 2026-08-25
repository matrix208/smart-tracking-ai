using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Application.Services;

public sealed class TaskService : ITaskService
{
    private readonly TrackingDbContext _context;

    public TaskService(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Include(x => x.Trip)
            .OrderBy(x => x.TripId)
            .ThenBy(x => x.Sequence)
            .ThenBy(x => x.Id)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskDto>> GetByTripIdAsync(
        long tripId,
        CancellationToken cancellationToken = default)
    {
        if (tripId <= 0)
        {
            throw new ArgumentException("TripId is required.");
        }

        var tripExists = await _context.Trips
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == tripId,
                cancellationToken);

        if (!tripExists)
        {
            throw new ArgumentException("Trip not found.");
        }

        return await _context.Tasks
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.TripId == tripId)
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.Id)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.Id == id)
            .Select(ToDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskDto> CreateAsync(
        TaskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await ValidateTripAsync(
            request.TripId,
            cancellationToken);

        var entity = new TaskEntity
        {
            TripId = request.TripId,
            Title = request.Title.Trim(),
            Description = Normalize(request.Description),
            Type = Normalize(request.Type) ?? "Custom",
            Sequence = request.Sequence,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = Normalize(request.Address),
            ScheduledAt = request.ScheduledAt,
            Status = "Pending",
            Notes = Normalize(request.Notes),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            entity.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Task was created but could not be loaded.");
    }

    public async Task<TaskDto?> UpdateAsync(
        long id,
        TaskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var entity = await _context.Tasks
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Only Pending tasks can be edited.");
        }

        await ValidateTripAsync(
            request.TripId,
            cancellationToken);

        entity.TripId = request.TripId;
        entity.Title = request.Title.Trim();
        entity.Description = Normalize(request.Description);
        entity.Type = Normalize(request.Type) ?? "Custom";
        entity.Sequence = request.Sequence;
        entity.Latitude = request.Latitude;
        entity.Longitude = request.Longitude;
        entity.Address = Normalize(request.Address);
        entity.ScheduledAt = request.ScheduledAt;
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
        var entity = await _context.Tasks
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Only Pending tasks can be deleted.");
        }

        _context.Tasks.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TaskDto?> StartAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (entity.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Only Pending tasks can be started.");
        }

        var trip = await _context.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == entity.TripId,
                cancellationToken);

        if (trip is null)
        {
            throw new InvalidOperationException(
                "Trip not found.");
        }

        if (trip.Status != "InProgress")
        {
            throw new InvalidOperationException(
                "Task can only be started when the trip is InProgress.");
        }

        var activeTaskExists = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TripId == entity.TripId &&
                    x.Id != entity.Id &&
                    x.Status == "InProgress",
                cancellationToken);

        if (activeTaskExists)
        {
            throw new InvalidOperationException(
                "Another task is already InProgress for this trip.");
        }

        var previousTaskIncomplete = await _context.Tasks
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.TripId == entity.TripId &&
                    (
                        x.Sequence < entity.Sequence ||
                        (
                            x.Sequence == entity.Sequence &&
                            x.Id < entity.Id
                        )
                    ) &&
                    x.Status != "Completed",
                cancellationToken);

        if (previousTaskIncomplete)
        {
            throw new InvalidOperationException(
                "Previous task must be completed first.");
        }

        entity.Status = "InProgress";
        entity.StartedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<TaskDto?> CompleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
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
                "Only InProgress tasks can be completed.");
        }

        entity.Status = "Completed";
        entity.CompletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<TaskDto?> CancelAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tasks
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
                "Completed tasks cannot be cancelled.");
        }

        if (entity.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                "Task is already cancelled.");
        }

        entity.Status = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    private async Task ValidateTripAsync(
        long tripId,
        CancellationToken cancellationToken)
    {
        if (tripId <= 0)
        {
            throw new ArgumentException(
                "TripId is required.");
        }

        var tripExists = await _context.Trips
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == tripId,
                cancellationToken);

        if (!tripExists)
        {
            throw new ArgumentException(
                "Trip not found.");
        }
    }

    private static void ValidateRequest(
        TaskRequestDto request)
    {
        if (request.TripId <= 0)
        {
            throw new ArgumentException(
                "TripId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                "Title is required.");
        }

        if (request.Sequence < 0)
        {
            throw new ArgumentException(
                "Sequence cannot be negative.");
        }

        if (request.Latitude.HasValue &&
            (request.Latitude.Value < -90 ||
             request.Latitude.Value > 90))
        {
            throw new ArgumentException(
                "Latitude must be between -90 and 90.");
        }

        if (request.Longitude.HasValue &&
            (request.Longitude.Value < -180 ||
             request.Longitude.Value > 180))
        {
            throw new ArgumentException(
                "Longitude must be between -180 and 180.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static Expression<Func<TaskEntity, TaskDto>>
        ToDtoExpression()
    {
        return x => new TaskDto
        {
            Id = x.Id,
            TripId = x.TripId,
            TripNumber = x.Trip.TripNumber,
            Title = x.Title,
            Description = x.Description,
            Type = x.Type,
            Sequence = x.Sequence,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Address = x.Address,
            ScheduledAt = x.ScheduledAt,
            StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt,
            Status = x.Status,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }
}
