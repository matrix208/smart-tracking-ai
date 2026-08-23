namespace Tracking.Events;

public sealed record DeviceConnectedEvent(
    string DeviceId,
    DateTime Timestamp
) : ITrackingEvent;