using System.Buffers.Binary;
using System.Text;
using Tracking.Plugin.GT06.Mappers;
using Tracking.Plugin.GT06.Protocol.Protocols;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Protocol.Encoders;

public sealed class CommandEncoder
{
    private const ushort Language = 0x0002;

    public ReadOnlyMemory<byte> Encode(
        DeviceCommand command)
    {
        return Encode(
            Gt06CommandMapper.Map(command),
            command.ServerFlag);
    }

    public ReadOnlyMemory<byte> Encode(
        string asciiCommand,
        uint serverFlag)
    {
        byte[] commandBytes =
            Encoding.ASCII.GetBytes(asciiCommand);

        using var payload = new MemoryStream();

        // Command Length
        payload.WriteByte(
            (byte)(4 + commandBytes.Length));

        // Server Flag
        Span<byte> flag = stackalloc byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(
            flag,
            serverFlag);

        payload.Write(flag);

        // ASCII Command
        payload.Write(commandBytes);

        // Language
        Span<byte> language = stackalloc byte[2];

        BinaryPrimitives.WriteUInt16BigEndian(
            language,
            Language);

        payload.Write(language);

        // Build GT06 Packet
        return Gt06PacketBuilder.Build(
            Gt06MessageType.Command,
            payload.ToArray(),
            1);
    }
}