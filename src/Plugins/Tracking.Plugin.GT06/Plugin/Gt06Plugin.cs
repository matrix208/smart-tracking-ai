using Tracking.Plugin.GT06.Mappers;
using Tracking.Plugin.GT06.Protocol.Decoders;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Metadata;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Plugin;

public sealed class Gt06Plugin : IProtocolPlugin
{
    private readonly Gt06Decoder _decoder = new();

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

        ushort header = System.Buffers.Binary.BinaryPrimitives
            .ReadUInt16BigEndian(packet);

        return header == 0x7878 ||
               header == 0x7979;
    }

    public ValueTask<DeviceMessage?> DecodeAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        var decoded = _decoder.Decode(packet);

        var deviceMessage = Gt06MessageMapper.Map(decoded);

        if (deviceMessage != null)
        {
            Console.WriteLine($"GT06 -> {deviceMessage.Type}");
        }

        return ValueTask.FromResult(deviceMessage);
    }

    public ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}