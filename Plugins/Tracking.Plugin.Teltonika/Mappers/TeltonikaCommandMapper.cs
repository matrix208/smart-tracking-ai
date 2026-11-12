using Tracking.SDK.Commands;
using Tracking.SDK.Models;

namespace Tracking.Plugin.Teltonika.Mappers;

internal static class TeltonikaCommandMapper
{
    public static string Map(DeviceCommand command)
    {
        return command.Name.ToLowerInvariant() switch
        {
            DeviceCommandNames.Position => "getgps",
            "requestposition" => "getgps",
            DeviceCommandNames.Status => "getinfo",
            DeviceCommandNames.Version => "getver",
            DeviceCommandNames.Reboot => "cpureset",
            DeviceCommandNames.Reset => "cpureset",
            DeviceCommandNames.RestartGps => "gpsreset",
            DeviceCommandNames.Iccid => "getgpsinfo",

            _ => throw new NotSupportedException(
                $"Teltonika command '{command.Name}' is not supported.")
        };
    }
}
