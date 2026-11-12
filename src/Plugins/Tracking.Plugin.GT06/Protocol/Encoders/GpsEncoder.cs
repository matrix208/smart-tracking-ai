using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class GpsEncoder
{
    public ReadOnlyMemory<byte> Encode(
        DateTime time,
        double latitude,
        double longitude,
        byte speed,
        ushort course,
        ushort serial)
    {
        Span<byte> payload = stackalloc byte[18];

        int index = 0;


        // Date Time (6 bytes)
        payload[index++] = ToBcd((byte)(time.Year - 2000));
        payload[index++] = ToBcd((byte)time.Month);
        payload[index++] = ToBcd((byte)time.Day);
        payload[index++] = ToBcd((byte)time.Hour);
        payload[index++] = ToBcd((byte)time.Minute);
        payload[index++] = ToBcd((byte)time.Second);


        // GPS information
        payload[index++] = 0xCC;


        // Latitude
        uint lat =
            (uint)(Math.Abs(latitude) * 1800000);

        BinaryPrimitives.WriteUInt32BigEndian(
            payload.Slice(index, 4),
            lat);

        index += 4;


        // Longitude
        uint lon =
            (uint)(Math.Abs(longitude) * 1800000);

        BinaryPrimitives.WriteUInt32BigEndian(
            payload.Slice(index, 4),
            lon);

        index += 4;


        // Speed
        payload[index++] = speed;


        // Course + Status
        ushort status = course;

        // GPS Fix = valid
        status &= 0x03FF;

        BinaryPrimitives.WriteUInt16BigEndian(
            payload.Slice(index, 2),
            status);


        return Gt06PacketBuilder.Build(
            Gt06MessageType.GPS,
            payload,
            serial);
    }


    private static byte ToBcd(byte value)
    {
        return (byte)(
            ((value / 10) << 4)
            |
            (value % 10));
    }
}