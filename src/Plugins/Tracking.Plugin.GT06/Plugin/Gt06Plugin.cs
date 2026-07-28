using Tracking.SDK.Interfaces;
using Tracking.SDK.Metadata;
using Tracking.SDK.Models;

namespace Tracking.Plugin.GT06.Plugin;

public sealed class Gt06Plugin : IProtocolPlugin
{
    public PluginManifest Manifest => new()
    {
        Id = "gt06",
        Name = "GT06 Protocol",
        Version = "1.0.0",
        Author = "Talal",
        Manufacturer = "Tracking Platform",
        EntryPoint = "Tracking.Plugin.GT06.Plugin.Gt06Plugin",
        Assembly = "Tracking.Plugin.GT06.dll",
        DefaultPort = 5001,
        SupportsTcp = true,
        SupportsUdp = false
    };

    public bool CanHandle(ReadOnlySpan<byte> packet)
    {
        throw new NotImplementedException();
    }

    public ValueTask<DeviceMessage?> DecodeAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ReadOnlyMemory<byte>> EncodeAsync(
        DeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}