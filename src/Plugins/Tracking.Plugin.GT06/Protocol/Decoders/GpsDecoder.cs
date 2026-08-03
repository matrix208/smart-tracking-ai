using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class GpsDecoder
{
    public object Decode(BinaryReader reader)
    {
       var timestamp = reader.ReadDateTimeBcd();


        // GPS info
        var gpsInfo = reader.ReadByte();

        // Latitude
        var latitudeRaw = reader.ReadUInt32BE();

        // Longitude
        var longitudeRaw = reader.ReadUInt32BE();

        // Speed km/h
        var speed = reader.ReadByte();

        // Course + Status
        var status = reader.ReadCourseStatus();

            var gpsFix = status.GpsFix;
            var isWest = status.West;
            var isSouth = status.South;

            var course = status.Course;


        var latitude = latitudeRaw / 1800000.0;
        var longitude = longitudeRaw / 1800000.0;


        if (isSouth)
            latitude = -latitude;

        if (isWest)
            longitude = -longitude;

            Console.WriteLine(
                $"GPS: Lat={latitude}, Lon={longitude}, Speed={speed}, Course={course}, Fix={gpsFix}");
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