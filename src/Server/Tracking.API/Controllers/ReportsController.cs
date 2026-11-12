using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracking.API.Auth;
using Tracking.API.Models;
using Tracking.Storage.Data;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "permission.reports.view")]
public class ReportsController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        TrackingDbContext db,
        ILogger<ReportsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/reports/summary?dateFrom=..&dateTo=..&vehicleId=..&driverId=..
    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] long? vehicleId,
        [FromQuery] long? driverId)
    {
        var tripsQuery = _db.Trips
            .AsNoTracking()
            .AsQueryable();

        if (dateFrom.HasValue)
        {
            tripsQuery = tripsQuery.Where(x =>
                x.ScheduledStartAt >= dateFrom.Value ||
                x.ActualStartAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            tripsQuery = tripsQuery.Where(x =>
                x.ScheduledStartAt <= dateTo.Value ||
                x.ActualStartAt <= dateTo.Value);
        }

        if (vehicleId.HasValue)
            tripsQuery = tripsQuery.Where(x => x.VehicleId == vehicleId.Value);

        if (driverId.HasValue)
            tripsQuery = tripsQuery.Where(x => x.DriverId == driverId.Value);

        var trips = await tripsQuery
            .Select(x => new
            {
                x.Id,
                x.VehicleId,
                x.DriverId,
                x.ActualStartAt,
                x.ActualEndAt
            })
            .ToListAsync();

        var totalTimeMinutes = trips.Sum(x =>
            x.ActualStartAt.HasValue && x.ActualEndAt.HasValue
                ? Math.Max(
                    0,
                    (int)(x.ActualEndAt.Value - x.ActualStartAt.Value).TotalMinutes)
                : 0);

        var positionQuery = _db.Positions
            .AsNoTracking()
            .Where(x => x.Valid);

        if (dateFrom.HasValue)
            positionQuery = positionQuery.Where(x => x.DeviceTime >= dateFrom.Value);

        if (dateTo.HasValue)
            positionQuery = positionQuery.Where(x => x.DeviceTime <= dateTo.Value);

        if (vehicleId.HasValue)
        {
            var deviceImei = await _db.Vehicles
                .AsNoTracking()
                .Where(x => x.Id == vehicleId.Value)
                .Select(x => x.Device != null ? x.Device.Imei : null)
                .FirstOrDefaultAsync();

            if (deviceImei == null)
            {
                return Ok(new ReportSummaryDto
                {
                    TotalDistanceKm = 0,
                    TotalTimeMinutes = totalTimeMinutes,
                    MaxSpeedKmh = 0,
                    TotalTrips = trips.Count
                });
            }

            positionQuery = positionQuery.Where(x => x.DeviceId == deviceImei);
        }
        else
        {
            var deviceIds = await _db.Vehicles
                .AsNoTracking()
                .Where(x => x.DeviceId.HasValue)
                .Select(x => x.Device!.Imei)
                .ToListAsync();

            if (deviceIds.Count > 0)
                positionQuery = positionQuery.Where(x => deviceIds.Contains(x.DeviceId));
        }

        var positions = await positionQuery
            .OrderBy(x => x.DeviceId)
            .ThenBy(x => x.DeviceTime)
            .Select(x => new PositionSample(
                x.Latitude,
                x.Longitude,
                x.Speed,
                x.DeviceTime))
            .ToListAsync();

        var totalDistanceKm = CalculateDistanceKm(positions);

        var maxSpeedKmh = positions.Count == 0
            ? 0
            : positions.Max(x => x.Speed);

        var summary = new ReportSummaryDto
        {
            TotalDistanceKm = Math.Round(totalDistanceKm, 2),
            TotalTimeMinutes = totalTimeMinutes,
            MaxSpeedKmh = Math.Round(maxSpeedKmh, 2),
            TotalTrips = trips.Count
        };

        return Ok(summary);
    }

    // GET /api/reports/details?dateFrom=..&dateTo=..&vehicleId=..&driverId=..&page=1&pageSize=20
    [HttpGet("details")]
    public async Task<ActionResult<ReportDetailsResponseDto>> GetDetails(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] long? vehicleId,
        [FromQuery] long? driverId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Trips
            .AsNoTracking()
            .Include(x => x.Vehicle)
            .Include(x => x.Driver)
            .AsQueryable();

        if (dateFrom.HasValue)
        {
            query = query.Where(x =>
                x.ScheduledStartAt >= dateFrom.Value ||
                x.ActualStartAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x =>
                x.ScheduledStartAt <= dateTo.Value ||
                x.ActualStartAt <= dateTo.Value);
        }

        if (vehicleId.HasValue)
            query = query.Where(x => x.VehicleId == vehicleId.Value);

        if (driverId.HasValue)
            query = query.Where(x => x.DriverId == driverId.Value);

        var totalResults = await query.CountAsync();

        var trips = await query
            .OrderByDescending(x => x.ActualStartAt ?? x.ScheduledStartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = new List<ReportRowDto>(trips.Count);

        foreach (var trip in trips)
        {
            var durationMinutes =
                trip.ActualStartAt.HasValue && trip.ActualEndAt.HasValue
                    ? Math.Max(
                        0,
                        (int)(trip.ActualEndAt.Value - trip.ActualStartAt.Value)
                            .TotalMinutes)
                    : 0;

            var speed = 0.0;
            var distance = 0.0;

            if (trip.Vehicle.DeviceId.HasValue && trip.Vehicle.Device != null)
            {
                var positions = await _db.Positions
                    .AsNoTracking()
                    .Where(x =>
                        x.Valid &&
                        x.DeviceId == trip.Vehicle.Device.Imei &&
                        (!trip.ActualStartAt.HasValue ||
                         x.DeviceTime >= trip.ActualStartAt.Value) &&
                        (!trip.ActualEndAt.HasValue ||
                         x.DeviceTime <= trip.ActualEndAt.Value))
                    .OrderBy(x => x.DeviceTime)
                    .Select(x => new PositionSample(
                        x.Latitude,
                        x.Longitude,
                        x.Speed,
                        x.DeviceTime))
                    .ToListAsync();

                distance = CalculateDistanceKm(positions);
                speed = positions.Count == 0
                    ? 0
                    : positions.Max(x => x.Speed);
            }

            rows.Add(new ReportRowDto
            {
                Vehicle = string.IsNullOrWhiteSpace(trip.Vehicle.Name)
                    ? trip.Vehicle.PlateNumber ?? trip.Vehicle.Id.ToString()
                    : trip.Vehicle.Name,
                Date = trip.ActualStartAt ?? trip.ScheduledStartAt,
                DistanceKm = Math.Round(distance, 2),
                Trips = 1,
                SpeedKmh = Math.Round(speed, 2),
                Duration = FormatDuration(durationMinutes),
                Driver = trip.Driver.Name
            });
        }

        return Ok(new ReportDetailsResponseDto
        {
            Rows = rows,
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize
        });
    }

    // GET /api/reports/route?vehicleId=..&dateFrom=..&dateTo=..
    [HttpGet("route")]
    public async Task<ActionResult<List<RoutePointDto>>> GetRoute(
        [FromQuery] long vehicleId,
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo)
    {
        if (vehicleId <= 0)
            return BadRequest("vehicleId must be greater than zero.");

        if (dateTo < dateFrom)
            return BadRequest("dateTo must be greater than or equal to dateFrom.");

        var deviceImei = await _db.Vehicles
            .AsNoTracking()
            .Where(x => x.Id == vehicleId)
            .Select(x => x.Device != null ? x.Device.Imei : null)
            .FirstOrDefaultAsync();

        if (deviceImei == null)
            return NotFound("Vehicle or its tracking device was not found.");

        var points = await _db.Positions
            .AsNoTracking()
            .Where(x =>
                x.Valid &&
                x.DeviceId == deviceImei &&
                x.DeviceTime >= dateFrom &&
                x.DeviceTime <= dateTo)
            .OrderBy(x => x.DeviceTime)
            .Select(x => new RoutePointDto
            {
                Lat = x.Latitude,
                Lng = x.Longitude,
                Timestamp = x.DeviceTime
            })
            .ToListAsync();

        return Ok(points);
    }

    private static double CalculateDistanceKm<T>(IReadOnlyList<T> points)
        where T : IPositionSample
    {
        if (points.Count < 2)
            return 0;

        double total = 0;

        for (var i = 1; i < points.Count; i++)
        {
            total += HaversineKm(
                points[i - 1].Latitude,
                points[i - 1].Longitude,
                points[i].Latitude,
                points[i].Longitude);
        }

        return total;
    }

    private static double HaversineKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371.0088;

        var lat1 = DegreesToRadians(latitude1);
        var lat2 = DegreesToRadians(latitude2);
        var deltaLat = DegreesToRadians(latitude2 - latitude1);
        var deltaLon = DegreesToRadians(longitude2 - longitude1);

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Sin(deltaLon / 2) *
            Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180.0;

    private static string FormatDuration(int minutes)
    {
        if (minutes <= 0)
            return "0m";

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return hours > 0
            ? $"{hours}h {remainingMinutes}m"
            : $"{remainingMinutes}m";
    }

    private interface IPositionSample
    {
        double Latitude { get; }
        double Longitude { get; }
    }

    private sealed record PositionSample(
        double Latitude,
        double Longitude,
        double Speed,
        DateTime DeviceTime) : IPositionSample;
}
