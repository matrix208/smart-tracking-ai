using Microsoft.EntityFrameworkCore;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Application.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly TrackingDbContext _context;

    public VehicleService(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Include(x => x.Device)
            .OrderBy(x => x.Name)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Include(x => x.Device)
            .Where(x => x.Id == id)
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VehicleDto> CreateAsync(
        VehicleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await ValidateDeviceAsync(
            request.DeviceId,
            null,
            cancellationToken);

        var vehicle = new VehicleEntity
        {
            Name = request.Name.Trim(),
            PlateNumber = Normalize(request.PlateNumber),
            VehicleType = Normalize(request.VehicleType),
            Make = Normalize(request.Make),
            Model = Normalize(request.Model),
            Year = request.Year,
            Color = Normalize(request.Color),
            DeviceId = request.DeviceId,
            Enabled = request.Enabled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            vehicle.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "Vehicle was created but could not be loaded.");
    }

    public async Task<VehicleDto?> UpdateAsync(
        long id,
        VehicleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        await ValidateDeviceAsync(
            request.DeviceId,
            id,
            cancellationToken);

        vehicle.Name = request.Name.Trim();
        vehicle.PlateNumber = Normalize(request.PlateNumber);
        vehicle.VehicleType = Normalize(request.VehicleType);
        vehicle.Make = Normalize(request.Make);
        vehicle.Model = Normalize(request.Model);
        vehicle.Year = request.Year;
        vehicle.Color = Normalize(request.Color);
        vehicle.DeviceId = request.DeviceId;
        vehicle.Enabled = request.Enabled;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidateDeviceAsync(
        long? deviceId,
        long? vehicleId,
        CancellationToken cancellationToken)
    {
        if (!deviceId.HasValue)
        {
            return;
        }

        var deviceExists = await _context.Devices
            .AnyAsync(
                x => x.Id == deviceId.Value,
                cancellationToken);

        if (!deviceExists)
        {
            throw new ArgumentException(
                $"Device with id {deviceId.Value} does not exist.");
        }

        var alreadyAssigned = await _context.Vehicles
            .AnyAsync(
                x =>
                    x.DeviceId == deviceId.Value &&
                    (!vehicleId.HasValue || x.Id != vehicleId.Value),
                cancellationToken);

        if (alreadyAssigned)
        {
            throw new ArgumentException(
                $"Device with id {deviceId.Value} is already assigned to another vehicle.");
        }
    }

    private static void ValidateRequest(
        VehicleRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Vehicle name is required.");
        }

        if (request.Name.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Vehicle name cannot exceed 200 characters.");
        }

        if (request.PlateNumber?.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Plate number cannot exceed 50 characters.");
        }

        if (request.VehicleType?.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Vehicle type cannot exceed 100 characters.");
        }

        if (request.Make?.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Make cannot exceed 100 characters.");
        }

        if (request.Model?.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Model cannot exceed 100 characters.");
        }

        if (request.Color?.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Color cannot exceed 50 characters.");
        }

        if (request.Year.HasValue &&
            (request.Year.Value < 1900 ||
             request.Year.Value > DateTime.UtcNow.Year + 1))
        {
            throw new ArgumentException(
                "Vehicle year is invalid.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static VehicleDto ToDto(
        VehicleEntity x)
    {
        return new VehicleDto
        {
            Id = x.Id,
            Name = x.Name,
            PlateNumber = x.PlateNumber,
            VehicleType = x.VehicleType,
            Make = x.Make,
            Model = x.Model,
            Year = x.Year,
            Color = x.Color,
            DeviceId = x.DeviceId,
            DeviceImei = x.Device != null
                ? x.Device.Imei
                : null,
            Enabled = x.Enabled
        };
    }
}
