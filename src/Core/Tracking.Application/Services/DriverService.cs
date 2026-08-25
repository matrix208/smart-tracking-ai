using Microsoft.EntityFrameworkCore;
using Tracking.Application.DTOs;
using Tracking.Application.Interfaces;
using Tracking.Storage.Data;
using Tracking.Storage.Entities;

namespace Tracking.Application.Services;

public sealed class DriverService : IDriverService
{
    private readonly TrackingDbContext _context;

    public DriverService(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DriverDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new DriverDto
            {
                Id = x.Id,
                Name = x.Name,
                PhoneNumber = x.PhoneNumber,
                LicenseNumber = x.LicenseNumber,
                EmployeeNumber = x.EmployeeNumber,
                Enabled = x.Enabled
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DriverDto
            {
                Id = x.Id,
                Name = x.Name,
                PhoneNumber = x.PhoneNumber,
                LicenseNumber = x.LicenseNumber,
                EmployeeNumber = x.EmployeeNumber,
                Enabled = x.Enabled
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DriverDto> CreateAsync(
        DriverRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var driver = new DriverEntity
        {
            Name = request.Name.Trim(),
            PhoneNumber = Normalize(request.PhoneNumber),
            LicenseNumber = Normalize(request.LicenseNumber),
            EmployeeNumber = Normalize(request.EmployeeNumber),
            Enabled = request.Enabled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(driver.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "Driver was created but could not be loaded.");
    }

    public async Task<DriverDto?> UpdateAsync(
        long id,
        DriverRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var driver = await _context.Drivers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (driver is null)
        {
            return null;
        }

        driver.Name = request.Name.Trim();
        driver.PhoneNumber = Normalize(request.PhoneNumber);
        driver.LicenseNumber = Normalize(request.LicenseNumber);
        driver.EmployeeNumber = Normalize(request.EmployeeNumber);
        driver.Enabled = request.Enabled;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (driver is null)
        {
            return false;
        }

        _context.Drivers.Remove(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateRequest(DriverRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Driver name is required.");
        }

        if (request.Name.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Driver name cannot exceed 200 characters.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
