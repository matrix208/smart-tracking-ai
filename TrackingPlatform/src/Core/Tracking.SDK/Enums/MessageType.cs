namespace Tracking.SDK.Enums;

public enum MessageType
{
    Unknown = 0,

    Login,

    Heartbeat,

    Position,

    Alarm,

    Command,

    CommandResponse,

    Event,

    Photo,

    Video,

    Audio,

    Text
}