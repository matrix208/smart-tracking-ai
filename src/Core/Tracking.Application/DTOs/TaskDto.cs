namespace Tracking.Application.DTOs;

public sealed class TaskDto
{
    public long Id { get; set; }

    public long TripId { get; set; }

    public string TripNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = "Custom";

    public int Sequence { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Address { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = "Pending";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class TaskRequestDto
{
    public long TripId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = "Custom";

    public int Sequence { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? Address { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? Notes { get; set; }
}
