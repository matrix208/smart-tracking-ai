using Tracking.Plugin.Teltonika.Mappers;
using Tracking.Plugin.Teltonika.Protocol.Decoders;
using Tracking.Plugin.Teltonika.Protocol.Encoders;
using Tracking.Plugin.Teltonika.Protocol.Frames;
using Tracking.Plugin.Teltonika.Protocol.Models;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Metadata;
using Tracking.SDK.Models;

namespace Tracking.Plugin.Teltonika.Plugin;

public sealed class TeltonikaPlugin : IProtocolPlugin
{
    private readonly TeltonikaDecoder _decoder = new();
    private readonly ImeiResponseEncoder _imeiEncoder = new();
    private readonly AvlAckEncoder _ackEncoder = new();
    private readonly Codec12CommandEncoder _commandEncoder = new();

    public PluginManifest Manifest => new()
    {
        Id = "teltonika",
        Name = "Teltonika Codec8 Extended",
        Description = "Teltonika Codec8 / Codec8 Extended protocol plugin (FMC130 and FMB/FMC/FMM families)",
        Version = "1.0.0",
        Author = "Telal",
        Manufacturer = "Tracking Platform",
        EntryPoint = "Tracking.Plugin.Teltonika.Plugin.TeltonikaPlugin",
        Assembly = "Tracking.Plugin.Teltonika.dll",
        DefaultPort = 5002,
        SupportsTcp = true,
        SupportsUdp = false,
        Models = ["FMC130", "FMB920", "FMB140", "FMC150", "FMB640"],
        Capabilities = ["gps", "io-elements", "codec8", "codec8-extended"]
    };

    public bool CanHandle(ReadOnlySpan<byte> packet)
    {
        return TeltonikaPacketInspector.IsImeiHandshake(packet, out _) ||
               TeltonikaPacketInspector.IsAvlDataPacket(packet, out _);
    }

    public async ValueTask<DeviceMessage?> DecodeAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        var decoded = _decoder.Decode(packet);

        if (decoded is TeltonikaLoginMessage login)
        {
            session.DeviceId = login.Imei;
            session.ProtocolId = "teltonika";

            var ack = _imeiEncoder.Encode(accept: true);
            await session.SendAsync(ack);

            Console.WriteLine($"Teltonika IMEI accepted: {login.Imei}");

            return TeltonikaMessageMapper.Map(login);
        }

        if (decoded is TeltonikaAvlDataMessage avl)
        {
            avl.Imei = session.DeviceId ?? string.Empty;

            var ack = _ackEncoder.Encode((uint)avl.Records.Count);
            await session.SendAsync(ack);

            Console.WriteLine(
                $"Teltonika AVL batch: {avl.Records.Count} record(s), IMEI={avl.Imei}");

            return TeltonikaMessageMapper.Map(avl);
        }

        return null;
    }

    public ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var ascii = TeltonikaCommandMapper.Map(command);
        return ValueTask.FromResult(_commandEncoder.Encode(ascii));
    }
}
