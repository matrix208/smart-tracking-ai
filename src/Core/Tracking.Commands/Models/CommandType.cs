namespace Tracking.Commands.Models;

public enum CommandType
{
    RequestPosition,

    RequestStatus,

    RelayControl,

    OutputControl,

    Reboot,

    TimeSync,

    Custom
}