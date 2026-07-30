using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Mappers;

internal static class Gt06CommandMapper
{
    public static string Map(
        DeviceCommand command)
    {
        return command.Name switch
        {
            "RequestStatus"   => "STATUS#",

            "RequestPosition" => "DWXX#",

            "Reboot"          => "RESET#",

            _ => throw new NotSupportedException(
                $"GT06 command '{command.Name}' is not supported.")
        };
    }
}
