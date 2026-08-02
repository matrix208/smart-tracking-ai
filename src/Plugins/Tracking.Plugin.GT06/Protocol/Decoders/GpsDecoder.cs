using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class GpsDecoder
{
    public object Decode(BinaryReader reader)
    {
        // GT06 GPS Packet 0x12

        // Date time: YY MM DD HH MM SS
        var year = 2000 + reader.ReadByte();
        var month = reader.ReadByte();
        var day = reader.ReadByte();
        var hour = reader.ReadByte();
        var minute = reader.ReadByte();
        var second = reader.ReadByte();

        var timestamp = new DateTime(
            year,
            month,
            day,
            hour,
            minute,
            second,
            DateTimeKind.Utc);

        // GPS information byte
        var gpsInfo = reader.ReadByte();

        // Latitude (4 bytes)
        var latitudeRaw = reader.ReadUInt32BE();

        // Longitude (4 bytes)
        var longitudeRaw = reader.ReadUInt32BE();

        // Speed (1 byte)
        var speed = reader.ReadByte();

        // Course + status (2 bytes)
        var courseStatus = reader.ReadUInt16BE();

       // Status bits
            var gpsFix = (courseStatus & 0x0400) != 0;
            var isWest = (courseStatus & 0x0800) != 0;
            var isSouth = (courseStatus & 0x1000) != 0;

            var course = courseStatus & 0x03FF;

            // GT06 coordinates conversion
            var latitude = latitudeRaw / 1800000.0;
            var longitude = longitudeRaw / 1800000.0;

            // Apply hemisphere
            if (isSouth)
                latitude = -latitude;

            if (isWest)
                longitude = -longitude;

        return new GpsMessage
        {
            Latitude = latitude,
            Longitude = longitude,
            Speed = speed,
            Course = course,
            GpsFix = gpsFix,
            Timestamp = timestamp
        };
    }
}