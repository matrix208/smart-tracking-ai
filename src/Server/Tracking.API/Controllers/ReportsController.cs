using Microsoft.AspNetCore.Mvc;
using Tracking.API.Models;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ILogger<ReportsController> logger)
    {
        _logger = logger;
    }

    // GET /api/reports/summary?dateFrom=..&dateTo=..&vehicleId=..&driverId=..
    [HttpGet("summary")]
    public ActionResult<ReportSummaryDto> GetSummary(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? vehicleId,
        [FromQuery] string? driverId)
    {
        // TODO: استبدل هذا بالاستعلام الحقيقي من Tracking.Persistence
        // مثال متوقع:
        // var trips = await _dbContext.Trips
        //     .Where(t => t.StartedAt >= dateFrom && t.StartedAt <= dateTo)
        //     .Where(t => vehicleId == null || t.VehicleId == vehicleId)
        //     .Where(t => driverId == null || t.DriverId == driverId)
        //     .ToListAsync();

        var summary = new ReportSummaryDto
        {
            TotalDistanceKm = 2847,
            TotalTimeMinutes = 156 * 60 + 23,
            MaxSpeedKmh = 220,
            TotalTrips = 127
        };

        return Ok(summary);
    }

    // GET /api/reports/details?dateFrom=..&dateTo=..&vehicleId=..&driverId=..&page=1&pageSize=20
    [HttpGet("details")]
    public ActionResult<ReportDetailsResponseDto> GetDetails(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? vehicleId,
        [FromQuery] string? driverId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: استبدل بالاستعلام الحقيقي + Pagination من قاعدة البيانات
        var rows = new List<ReportRowDto>
        {
            new()
            {
                Vehicle = "Honda Accord - V002",
                Date = new DateTime(1984, 1, 15),
                DistanceKm = 256,
                Trips = 50,
                SpeedKmh = 220,
                Duration = "3h 45m",
                Driver = "khalid"
            },
            new()
            {
                Vehicle = "8908",
                Date = new DateTime(2025, 1, 15),
                DistanceKm = 284,
                Trips = 8,
                SpeedKmh = 140,
                Duration = "4h 12m",
                Driver = "khalid"
            }
        };

        var response = new ReportDetailsResponseDto
        {
            Rows = rows,
            TotalResults = 68,
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    // GET /api/reports/route?vehicleId=..&dateFrom=..&dateTo=..
    [HttpGet("route")]
    public ActionResult<List<RoutePointDto>> GetRoute(
        [FromQuery] string vehicleId,
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo)
    {
        // TODO: اسحب نقاط GPS الفعلية من جدول المواقع/الرحلات
        var points = new List<RoutePointDto>
        {
            new() { Lat = 18.2465, Lng = 42.5117, Timestamp = dateFrom },
            new() { Lat = 18.2501, Lng = 42.5200, Timestamp = dateFrom.AddMinutes(10) },
            new() { Lat = 18.2550, Lng = 42.5300, Timestamp = dateFrom.AddMinutes(20) }
        };

        return Ok(points);
    }
}
