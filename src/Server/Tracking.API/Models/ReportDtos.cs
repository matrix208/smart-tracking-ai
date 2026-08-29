namespace Tracking.API.Models;

public class ReportSummaryDto
{
    public double TotalDistanceKm { get; set; }
    public int TotalTimeMinutes { get; set; }
    public double MaxSpeedKmh { get; set; }
    public int TotalTrips { get; set; }
}

public class ReportRowDto
{
    public string Vehicle { get; set; } = "";
    public DateTime Date { get; set; }
    public double DistanceKm { get; set; }
    public int Trips { get; set; }
    public double SpeedKmh { get; set; }
    public string Duration { get; set; } = "";
    public string Driver { get; set; } = "";
}

public class ReportDetailsResponseDto
{
    public List<ReportRowDto> Rows { get; set; } = new();
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class RoutePointDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTime Timestamp { get; set; }
}
