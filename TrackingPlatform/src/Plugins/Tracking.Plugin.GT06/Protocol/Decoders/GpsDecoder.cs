
using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Models;

namespace Tracking.Plugin.GT06.Protocol.Decoders;

public sealed class GpsDecoder
{
    public GpsMessage Decode(BinaryReader reader)
    {
        // =====================================================
        // Timestamp
        // =====================================================

        var timestamp = reader.ReadDateTimeBcd();

        // =====================================================
        // GPS information
        // =====================================================

        var gpsInfo = reader.ReadByte();

        // =====================================================
        // Latitude
        // =====================================================

        var latitudeRaw = reader.ReadUInt32BE();

        // =====================================================
        // Longitude
        // =====================================================

        var longitudeRaw = reader.ReadUInt32BE();

        // =====================================================
        // Speed
        // =====================================================

        var speed = reader.ReadByte();

        // =====================================================
        // Course + Status
        // =====================================================

        var status = reader.ReadCourseStatus();

        var course = status.Course;
        var gpsFix = status.GpsFix;
        var isWest = status.West;
        var isSouth = status.South;

        // =====================================================
        // Convert coordinates
        // GT06: coordinate / 1,800,000
        // =====================================================

        var latitude = latitudeRaw / 1_800_000.0;
        var longitude = longitudeRaw / 1_800_000.0;

        // =====================================================
        // Direction
        // =====================================================

        if (isSouth)
        {
            latitude = -latitude;
        }

        if (isWest)
        {
            longitude = -longitude;
        }

        // =====================================================
        // Debug
        // =====================================================

        Console.WriteLine(
            $"GPS: " +
            $"Lat={latitude}, " +
            $"Lon={longitude}, " +
            $"Speed={speed}, " +
            $"Course={course}, " +
            $"Fix={gpsFix}, " +
            $"GPSInfo=0x{gpsInfo:X2}");

        // =====================================================
        // Message
        // =====================================================

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
