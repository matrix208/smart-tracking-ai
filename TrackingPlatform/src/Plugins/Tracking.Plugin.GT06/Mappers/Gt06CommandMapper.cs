using Tracking.SDK.Commands;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Mappers;

internal static class Gt06CommandMapper
{
    public static string Map(DeviceCommand command)
    {
       return command.Name.ToLowerInvariant() switch
{
    DeviceCommandNames.Position   => "DWXX#",
    "requestposition"             => "DWXX#",
    DeviceCommandNames.Status     => "STATUS#",
            DeviceCommandNames.Check      => "CHECK#",
            DeviceCommandNames.Version    => "VERSION#",
            DeviceCommandNames.Imei       => "IMEI#",

            DeviceCommandNames.Reboot     => "RESET#",
            DeviceCommandNames.Reset      => "RESET#",
            DeviceCommandNames.Factory    => "FACTORY#",
            DeviceCommandNames.RestartGps => "GPSRESET#",

            DeviceCommandNames.OilOn      => "RELAY,1#",
            DeviceCommandNames.OilOff     => "RELAY,0#",
            DeviceCommandNames.RelayOn    => "RELAY,1#",
            DeviceCommandNames.RelayOff   => "RELAY,0#",

            DeviceCommandNames.Monitor    => "MONITOR#",
            DeviceCommandNames.Tracker    => "TRACKER#",

            DeviceCommandNames.Acc         => "ACC#",
            DeviceCommandNames.Vibration   => "VIBRATION#",
            DeviceCommandNames.GeoFence    => "FENCE#",
            DeviceCommandNames.Overspeed   => "OVERSPEED#",

            DeviceCommandNames.Listen      => "LISTEN#",
            DeviceCommandNames.Center      => "CENTER#",
            DeviceCommandNames.Admin       => "ADMIN#",

            DeviceCommandNames.Language    => "LANGUAGE#",
            DeviceCommandNames.Led         => "LED#",
            DeviceCommandNames.Buzzer      => "BUZZER#",

            DeviceCommandNames.Iccid       => "ICCID#",
            DeviceCommandNames.Parameter   => "PARAM#",
            DeviceCommandNames.Timer       => "TIMER#",

            DeviceCommandNames.Upload     => BuildUpload(command),
            DeviceCommandNames.Apn        => BuildApn(command),
            DeviceCommandNames.Server     => BuildServer(command),
            DeviceCommandNames.TimeZone   => BuildTimeZone(command),
            DeviceCommandNames.Password   => BuildPassword(command),

            DeviceCommandNames.Sos        => BuildSos(command),
            DeviceCommandNames.Shock      => BuildShock(command),
            DeviceCommandNames.Move       => BuildMove(command),
            DeviceCommandNames.Speed      => BuildSpeed(command),
            DeviceCommandNames.Sleep      => BuildSleep(command),
            DeviceCommandNames.Heartbeat  => BuildHeartbeat(command),

            DeviceCommandNames.Signal     => "CSQ#",
            

            _ => throw new NotSupportedException(
                $"GT06 command '{command.Name}' is not supported.")
        };
    }

    private static string BuildUpload(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("interval", out var value))
            return "UPLOAD#";

        return $"UPLOAD,{value}#";
    }

    private static string BuildApn(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("apn", out var apn))
            return "APN#";

        var result = $"APN,{apn}";

        if (command.Parameters.TryGetValue("user", out var user))
            result += $",{user}";

        if (command.Parameters.TryGetValue("password", out var password))
            result += $",{password}";

        return result + "#";
    }

    private static string BuildServer(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("host", out var host))
            return "SERVER#";

        command.Parameters.TryGetValue("port", out var port);

        return $"SERVER,1,{host},{port ?? 5001}#";
    }

    private static string BuildTimeZone(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("offset", out var offset))
            return "TIMEZONE#";

        return $"TIMEZONE,{offset}#";
    }

    private static string BuildPassword(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("password", out var password))
            return "PASSWORD#";

        return $"PASSWORD,{password}#";
    }

    private static string BuildSos(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("numbers", out var numbers))
            return "SOS#";

        return $"SOS,{numbers}#";
    }

    private static string BuildShock(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("level", out var level))
            return "SHOCK#";

        return $"SHOCK,{level}#";
    }

    private static string BuildMove(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("radius", out var radius))
            return "MOVE#";

        return $"MOVE,{radius}#";
    }

    private static string BuildSpeed(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("limit", out var limit))
            return "SPEED#";

        return $"SPEED,{limit}#";
    }

    private static string BuildSleep(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("mode", out var mode))
            return "SLEEP#";

        return $"SLEEP,{mode}#";
    }

    private static string BuildHeartbeat(DeviceCommand command)
    {
        if (!command.Parameters.TryGetValue("interval", out var interval))
            return "HEART#";

        return $"HEART,{interval}#";
    }
}