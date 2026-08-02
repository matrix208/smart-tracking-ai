using Tracking.SDK.Commands;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Mappers;

internal static class Gt06CommandMapper
{
    public static string Map(DeviceCommand command)
    {
        return command.Name.ToLowerInvariant() switch
        {
            DeviceCommandNames.Position     => "DWXX#",
            DeviceCommandNames.Status       => "STATUS#",
            DeviceCommandNames.Reboot       => "RESET#",

            DeviceCommandNames.OilOn        => "RELAY,1#",
            DeviceCommandNames.OilOff       => "RELAY,0#",

            DeviceCommandNames.RelayOn      => "RELAY,1#",
            DeviceCommandNames.RelayOff     => "RELAY,0#",

            DeviceCommandNames.Monitor      => "MONITOR#",
            DeviceCommandNames.Tracker      => "TRACKER#",

            DeviceCommandNames.Upload       => "UPLOAD#",
            DeviceCommandNames.Check        => "CHECK#",

            DeviceCommandNames.Apn          => "APN#",
            DeviceCommandNames.Server       => "SERVER#",
            DeviceCommandNames.TimeZone     => "TIMEZONE#",
            DeviceCommandNames.Password     => "PASSWORD#",

            DeviceCommandNames.Factory      => "FACTORY#",
            DeviceCommandNames.RestartGps   => "GPSRESET#",
            DeviceCommandNames.Reset        => "RESET#",

            DeviceCommandNames.Sos          => "SOS#",
            DeviceCommandNames.Shock        => "SHOCK#",
            DeviceCommandNames.Move         => "MOVE#",
            DeviceCommandNames.Speed        => "SPEED#",
            DeviceCommandNames.Sleep        => "SLEEP#",
            DeviceCommandNames.Heartbeat    => "HEART#",

            DeviceCommandNames.Imei         => "IMEI#",
            DeviceCommandNames.Version      => "VERSION#",
            DeviceCommandNames.Signal       => "CSQ#",

            _ => throw new NotSupportedException(
                $"GT06 command '{command.Name}' is not supported.")
        };
    }
}