using Tracking.Plugin.Teltonika.Protocol.Models;
using Tracking.SDK.Enums;
using Tracking.SDK.Models;

namespace Tracking.Plugin.Teltonika.Mappers;

public static class TeltonikaMessageMapper
{
    public static DeviceMessage? Map(object message)
    {
        return message switch
        {
            TeltonikaLoginMessage login => MapLogin(login),
            TeltonikaAvlDataMessage avl => MapAvl(avl),
            _ => null
        };
    }

    private static DeviceMessage MapLogin(TeltonikaLoginMessage message)
    {
        return new DeviceMessage
        {
            Type = MessageType.Login,
            DeviceId = message.Imei,
            Payload = message
        };
    }

    private static DeviceMessage MapAvl(TeltonikaAvlDataMessage message)
    {
        // NOTE: IProtocolPlugin.DecodeAsync can only return a single
        // DeviceMessage per call, but Teltonika can batch multiple AVL
        // records in one packet (common after a reconnect). The most
        // recent record becomes the live Position; the full batch is
        // preserved in Payload for a future pipeline enhancement that
        // could persist historical records instead of only the latest.
        var latest = message.Records[^1];

        return new DeviceMessage
        {
            Type = MessageType.Position,
            DeviceId = message.Imei,

            Position = new Position
            {
                DeviceId = message.Imei,
                Imei = message.Imei,

                Latitude = latest.Latitude,
                Longitude = latest.Longitude,
                Altitude = latest.Altitude,
                Speed = latest.Speed,
                Course = latest.Angle,
                Satellites = latest.Satellites,

                DeviceTime = latest.Timestamp,
                ServerTime = DateTime.UtcNow,

                Valid = latest.GpsFix
            },

            Payload = message
        };
    }
}
