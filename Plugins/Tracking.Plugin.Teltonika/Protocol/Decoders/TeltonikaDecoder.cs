using System.Text;
using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.Teltonika.Protocol.Frames;
using Tracking.Plugin.Teltonika.Protocol.Models;
using Tracking.Plugin.Teltonika.Protocol.Protocols;

namespace Tracking.Plugin.Teltonika.Protocol.Decoders;

public sealed class TeltonikaDecoder
{
    public object Decode(ReadOnlyMemory<byte> packet)
    {
        if (TeltonikaPacketInspector.IsImeiHandshake(packet.Span, out var imeiLength))
        {
            return DecodeImei(packet, imeiLength);
        }

        if (TeltonikaPacketInspector.IsAvlDataPacket(packet.Span, out var codecId))
        {
            return DecodeAvlData(packet, codecId);
        }

        throw new InvalidDataException("Unrecognized Teltonika packet.");
    }

    // =====================================================
    // IMEI handshake
    // =====================================================

    private static TeltonikaLoginMessage DecodeImei(
        ReadOnlyMemory<byte> packet,
        int imeiLength)
    {
        var reader = new BinaryReader(packet);

        reader.ReadUInt16BE(); // length prefix, already validated

        var imeiBytes = reader.ReadBytes(imeiLength);

        var imei = Encoding.ASCII.GetString(imeiBytes);

        return new TeltonikaLoginMessage
        {
            Imei = imei
        };
    }

    // =====================================================
    // AVL Data Packet
    // =====================================================

    private static TeltonikaAvlDataMessage DecodeAvlData(
        ReadOnlyMemory<byte> packet,
        byte codecId)
    {
        var reader = new BinaryReader(packet);

        reader.Skip(TeltonikaConstants.PreambleLength); // 0x00000000
        reader.ReadUInt32BE();                          // data field length
        reader.ReadByte();                              // codec id (already known)

        var numberOfData1 = reader.ReadByte();

        var records = new List<TeltonikaAvlRecord>(numberOfData1);

        for (var i = 0; i < numberOfData1; i++)
        {
            records.Add(
                codecId == TeltonikaConstants.Codec8Extended
                    ? DecodeExtendedRecord(reader)
                    : DecodeBasicRecord(reader));
        }

        // Number of Data 2 (should equal numberOfData1) + 4-byte CRC field.
        // CRC verification is intentionally not enforced here (informational
        // only), matching this platform's existing GT06 plugin behavior.
        reader.ReadByte();
        if (reader.Remaining >= 4)
        {
            reader.Skip(4);
        }

        return new TeltonikaAvlDataMessage
        {
            CodecId = codecId,
            Records = records
        };
    }

    // =====================================================
    // GPS element (identical layout in Codec8 and Codec8 Extended)
    // =====================================================

    private static (double Lon, double Lat, double Alt, double Angle, byte Sats, double Speed)
        ReadGpsElement(BinaryReader reader)
    {
        // Longitude/Latitude are signed two's-complement 32-bit
        // integers, degrees * 10,000,000 (per Teltonika wiki).
        var longitude = reader.ReadInt32BE() / 10_000_000.0;
        var latitude = reader.ReadInt32BE() / 10_000_000.0;

        var altitude = reader.ReadInt16BE();
        var angle = reader.ReadUInt16BE();
        var satellites = reader.ReadByte();
        var speed = reader.ReadUInt16BE();

        return (longitude, latitude, altitude, angle, satellites, speed);
    }

    // =====================================================
    // Codec8 record (1-byte IO field sizes)
    // =====================================================

    private static TeltonikaAvlRecord DecodeBasicRecord(BinaryReader reader)
    {
        var timestampMs = reader.ReadInt64BE();
        var priority = reader.ReadByte();

        var (lon, lat, alt, angle, sats, speed) = ReadGpsElement(reader);

        reader.ReadByte(); // Event IO ID (1 byte) — not currently surfaced
        reader.ReadByte(); // N (total IO count) — informational only

        var elements = new List<TeltonikaIoElement>();

        var n1 = reader.ReadByte();
        for (var i = 0; i < n1; i++)
        {
            var id = reader.ReadByte();
            var value = reader.ReadBytes(1);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n2 = reader.ReadByte();
        for (var i = 0; i < n2; i++)
        {
            var id = reader.ReadByte();
            var value = reader.ReadBytes(2);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n4 = reader.ReadByte();
        for (var i = 0; i < n4; i++)
        {
            var id = reader.ReadByte();
            var value = reader.ReadBytes(4);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n8 = reader.ReadByte();
        for (var i = 0; i < n8; i++)
        {
            var id = reader.ReadByte();
            var value = reader.ReadBytes(8);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        return new TeltonikaAvlRecord
        {
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime,
            Priority = priority,
            Longitude = lon,
            Latitude = lat,
            Altitude = alt,
            Angle = angle,
            Satellites = sats,
            Speed = speed,
            IoElements = elements
        };
    }

    // =====================================================
    // Codec8 Extended record (2-byte IO field sizes + NX support)
    // =====================================================

    private static TeltonikaAvlRecord DecodeExtendedRecord(BinaryReader reader)
    {
        var timestampMs = reader.ReadInt64BE();
        var priority = reader.ReadByte();

        var (lon, lat, alt, angle, sats, speed) = ReadGpsElement(reader);

        reader.ReadUInt16BE(); // Event IO ID (2 bytes)
        reader.ReadUInt16BE(); // N (total IO count)

        var elements = new List<TeltonikaIoElement>();

        var n1 = reader.ReadUInt16BE();
        for (var i = 0; i < n1; i++)
        {
            var id = reader.ReadUInt16BE();
            var value = reader.ReadBytes(1);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n2 = reader.ReadUInt16BE();
        for (var i = 0; i < n2; i++)
        {
            var id = reader.ReadUInt16BE();
            var value = reader.ReadBytes(2);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n4 = reader.ReadUInt16BE();
        for (var i = 0; i < n4; i++)
        {
            var id = reader.ReadUInt16BE();
            var value = reader.ReadBytes(4);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var n8 = reader.ReadUInt16BE();
        for (var i = 0; i < n8; i++)
        {
            var id = reader.ReadUInt16BE();
            var value = reader.ReadBytes(8);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        var nx = reader.ReadUInt16BE();
        for (var i = 0; i < nx; i++)
        {
            var id = reader.ReadUInt16BE();
            var length = reader.ReadUInt16BE();
            var value = reader.ReadBytes(length);
            elements.Add(new TeltonikaIoElement { Id = id, RawValue = value });
        }

        return new TeltonikaAvlRecord
        {
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).UtcDateTime,
            Priority = priority,
            Longitude = lon,
            Latitude = lat,
            Altitude = alt,
            Angle = angle,
            Satellites = sats,
            Speed = speed,
            IoElements = elements
        };
    }
}
