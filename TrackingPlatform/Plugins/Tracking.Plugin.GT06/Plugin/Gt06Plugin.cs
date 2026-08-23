using Tracking.Plugin.GT06.Mappers;
using Tracking.Plugin.GT06.Protocol.Decoders;
using Tracking.Plugin.GT06.Protocol.Encoders;
using Tracking.Plugin.GT06.Protocol.Models;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Metadata;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Plugin;

public sealed class Gt06Plugin : IProtocolPlugin
{
    private readonly Gt06Decoder _decoder = new();

    private readonly LoginEncoder _loginEncoder = new();

    private readonly HeartbeatEncoder _heartbeatEncoder = new();

    private readonly CommandEncoder _commandEncoder = new();


    public PluginManifest Manifest => new()
    {
        Id = "gt06",
        Name = "GT06 Protocol",
        Version = "1.0.0",
        Author = "Telal",
        Manufacturer = "Tracking Platform",
        EntryPoint = "Tracking.Plugin.GT06.Plugin.Gt06Plugin",
        Assembly = "Tracking.Plugin.GT06.dll",
        DefaultPort = 5001,
        SupportsTcp = true,
        SupportsUdp = false
    };


    public bool CanHandle(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 2)
            return false;

        ushort header =
            System.Buffers.Binary.BinaryPrimitives
            .ReadUInt16BigEndian(packet);

        return header == 0x7878 ||
               header == 0x7979;
    }


    public async ValueTask<DeviceMessage?> DecodeAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        var decoded = _decoder.Decode(packet);

        Console.WriteLine(
            $"GT06 Decoded: {decoded.GetType().Name}");


        // ==========================
        // Login
        // ==========================
        if (decoded is LoginMessage login)
        {
            session.DeviceId = login.Imei;
            session.ProtocolId = "gt06";


            Console.WriteLine(
                $"PLUGIN Session={session.GetHashCode()} Protocol={session.ProtocolId}");


            var ack = _loginEncoder.Encode(
                login.Imei,
                login.Serial);


            await session.SendAsync(ack);


            Console.WriteLine(
                $"ACK Bytes: {Convert.ToHexString(ack.Span)}");


            Console.WriteLine(
                $"GT06 Login ACK sent: {login.Imei}");
        }



        // ==========================
        // Heartbeat
        // ==========================
        if (decoded is HeartbeatMessage heartbeat)
        {
            heartbeat.DeviceId = session.DeviceId;


            var ack = _heartbeatEncoder.Encode(
                heartbeat.Serial);


            await session.SendAsync(ack);


            Console.WriteLine(
                "GT06 Heartbeat ACK sent");
        }



        // ==========================
        // GPS Position
        // ==========================
        if (decoded is GpsMessage gps)
        {
            gps.DeviceId = session.DeviceId;


            Console.WriteLine(
                $"GPS DeviceId Attached: {gps.DeviceId}");
        }



        // ==========================
        // Alarm
        // ==========================
        if (decoded is AlarmMessage alarm)
        {
            alarm.DeviceId = session.DeviceId;
        }



        var deviceMessage =
            Gt06MessageMapper.Map(decoded);



        if (deviceMessage != null)
        {
            Console.WriteLine(
                $"GT06 -> {deviceMessage.Type}");

            Console.WriteLine("==============================");
            Console.WriteLine(
                $"Session.DeviceId = {session.DeviceId}");

            Console.WriteLine(
                $"Message.DeviceId = {deviceMessage.DeviceId}");

            Console.WriteLine("==============================");
        }


        return deviceMessage;
    }



    public ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(
            _commandEncoder.Encode(command));
    }
}