namespace Tracking.Events;

public sealed record DeviceDisconnectedEvent(
    string DeviceId,
    DateTime Timestamp
) : ITrackingEvent;