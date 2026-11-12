using Tracking.Plugin.GT06.Protocol.Messages;
using Tracking.Plugin.GT06.Protocol.Models;
using Tracking.SDK.Enums;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Mappers;

public static class Gt06MessageMapper
{
    public static DeviceMessage? Map(object message)
    {
        return message switch
        {
            LoginMessage login => Map(login),
            GpsMessage gps => Map(gps),
            HeartbeatMessage heartbeat => Map(heartbeat),
            AlarmMessage alarm => Map(alarm),
            CommandResponseMessage response => Map(response),
            _ => null
        };
    }

    private static DeviceMessage Map(LoginMessage message)
    {
        return new DeviceMessage
        {
            Type = MessageType.Login,
            DeviceId = message.Imei,
            Payload = message
        };
    }
private static DeviceMessage Map(GpsMessage message)
{
    return new DeviceMessage
    {
        Type = MessageType.Position,

        DeviceId = message.DeviceId,

        Position = new Position
        {
            DeviceId = message.DeviceId,

            Latitude = message.Latitude,
            Longitude = message.Longitude,

            Speed = message.Speed,
            Course = message.Course,

            DeviceTime = message.Timestamp,
            ServerTime = DateTime.UtcNow,

            Valid = message.GpsFix
        },

        Payload = message
    };
}
    private static DeviceMessage Map(HeartbeatMessage message)
    {
        return new DeviceMessage
        {
            Type = MessageType.Heartbeat,
            Payload = message
        };
    }
private static DeviceMessage Map(AlarmMessage message)
{
    return new DeviceMessage
    {
        Type = MessageType.Alarm,

        DeviceId = message.DeviceId,

        Alarm = new Tracking.SDK.Models.Alarm
        {
            DeviceId = message.DeviceId ?? string.Empty,
            AlarmCode = message.AlarmCode,
            DeviceTime = message.Timestamp,
            ServerTime = DateTime.UtcNow
        },

        Payload = message
    };
}
    private static DeviceMessage Map(CommandResponseMessage message)
{
            return new DeviceMessage
            {
                Type = MessageType.CommandResponse,

                Payload = new CommandResult
                {
                    ServerFlag = message.ServerFlag,
                    DeviceId = string.Empty,
                    Success = message.Success,
                    Response = message.Text
                }
            };
        }
}