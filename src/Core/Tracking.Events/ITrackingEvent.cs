namespace Tracking.Events;

public interface ITrackingEvent
{
    DateTime Timestamp { get; }
}