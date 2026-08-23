using BinaryReader = Tracking.Protocol.Binary.BinaryReader;
using Tracking.Plugin.GT06.Protocol.Protocols;

namespace Tracking.Plugin.GT06.Protocol.Frames;

public sealed class Gt06FrameReader
{
    public Gt06Frame Read(ReadOnlyMemory<byte> packet)
    {
        var reader = new BinaryReader(packet);

        var header = reader.ReadUInt16BE();

        if (header != Gt06Constants.HeaderBasic &&
            header != Gt06Constants.HeaderExtended)
        {
            throw new InvalidDataException("Invalid GT06 header.");
        }

        byte length = reader.ReadByte();

        var protocol = (Gt06MessageType)reader.ReadByte();

        // Payload = كل البيانات ما عدا:
        // Header + Length + Protocol + Serial + CRC + Tail
        int payloadLength = length - 5;

        if (payloadLength < 0)
            throw new InvalidDataException("Invalid GT06 packet length.");

        var payload = reader.ReadBytes(payloadLength);

        ushort serial = reader.ReadUInt16BE();

        ushort crc = reader.ReadUInt16BE();

        ushort tail = reader.ReadUInt16BE();

        if (tail != Gt06Constants.Tail)
            throw new InvalidDataException("Invalid GT06 tail.");
return new Gt06Frame
{
    Header = header,
    Length = length,
    MessageType = protocol,
    Payload = payload,
    Serial = serial,
    Crc = crc
};

    }
}