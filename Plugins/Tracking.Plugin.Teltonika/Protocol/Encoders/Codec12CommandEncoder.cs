using System.Buffers.Binary;
using System.Text;
using Tracking.Plugin.Teltonika.Protocol.CRC;

namespace Tracking.Plugin.Teltonika.Protocol.Encoders;

/// <summary>
/// Builds a Codec12 GPRS command packet (server -> device),
/// used for text commands such as "getinfo" / "getio" / "cpureset".
/// </summary>
public sealed class Codec12CommandEncoder
{
    private const byte Codec12 = 0x0C;
    private const byte TypeCommand = 0x05;

    public ReadOnlyMemory<byte> Encode(string asciiCommand)
    {
        var commandBytes = Encoding.ASCII.GetBytes(asciiCommand);

        using var body = new MemoryStream();

        body.WriteByte(Codec12);
        body.WriteByte(0x01);           // Command Quantity 1
        body.WriteByte(TypeCommand);    // Type = command

        Span<byte> sizeBuf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(sizeBuf, (uint)commandBytes.Length);
        body.Write(sizeBuf);

        body.Write(commandBytes);
        body.WriteByte(0x01);           // Command Quantity 2

        var bodyBytes = body.ToArray();

        using var packet = new MemoryStream();

        // Preamble
        packet.Write(new byte[4]);

        // Data Size (Codec ID through Command Quantity 2)
        Span<byte> dataSizeBuf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(dataSizeBuf, (uint)bodyBytes.Length);
        packet.Write(dataSizeBuf);

        packet.Write(bodyBytes);

        // CRC-16/IBM over the body (Codec ID -> Command Quantity 2)
        var crc = Crc16Ibm.Compute(bodyBytes);

        Span<byte> crcBuf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBuf, crc);
        packet.Write(crcBuf);

        return packet.ToArray();
    }
}
