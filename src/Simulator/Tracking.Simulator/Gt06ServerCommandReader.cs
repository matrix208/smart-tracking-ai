using System.Buffers.Binary;
using Tracking.Plugin.GT06.Protocol.Protocols;

internal static class Gt06ServerCommandReader
{
    public static Gt06ServerCommand? Read(byte[] packet)
    {
        if (packet.Length < 10)
            return null;

        if (packet[0] != 0x78 || packet[1] != 0x78)
            return null;

        int length = packet[2];

        var type = (Gt06MessageType)packet[3];

        int payloadLength = length - 5;

        var payload = packet
            .Skip(4)
            .Take(payloadLength)
            .ToArray();

        ushort serial =
            BinaryPrimitives.ReadUInt16BigEndian(
                packet.AsSpan(packet.Length - 4, 2));

        return new Gt06ServerCommand
        {
            Type = type,
            Payload = payload,
            Serial = serial
        };
    }
}