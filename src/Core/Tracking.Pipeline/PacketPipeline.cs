using Tracking.PluginManager.Services;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Pipeline;

public sealed class PacketPipeline
{
    private readonly ProtocolPluginManager _pluginManager;

    public PacketPipeline(ProtocolPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public async ValueTask<DeviceMessage?> ProcessAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Pipeline received packet: {Convert.ToHexString(packet.Span)}");

        var plugin = _pluginManager.Find(packet.Span);

        if (plugin is null)
        {
            Console.WriteLine(
                "No enabled plugin handled packet");

            return null;
        }

        Console.WriteLine(
            $"Using plugin: {plugin.Manifest.Name}");

        var message = await plugin.DecodeAsync(
            packet,
            session,
            cancellationToken);

        if (message != null)
        {
            Console.WriteLine(
                $"Decoded message: {message.Type}");
        }
        else
        {
            Console.WriteLine(
                "Plugin returned null message");
        }

        return message;
    }
}
