namespace Tracking.Events;

public sealed record PositionReceivedEvent(
    string DeviceId,
    double Latitude,
    double Longitude,
    double Speed,
    DateTime Timestamp
) : ITrackingEvent;