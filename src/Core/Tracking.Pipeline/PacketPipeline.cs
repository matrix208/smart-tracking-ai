using Tracking.PluginLoader.Services;
using Tracking.SDK.Interfaces;
using Tracking.SDK.Models;

namespace Tracking.Pipeline;

public sealed class PacketPipeline
{
    private readonly IReadOnlyList<IProtocolPlugin> _plugins;

    public PacketPipeline(IReadOnlyList<IProtocolPlugin> plugins)
    {
        _plugins = plugins;
    }

    public async ValueTask<DeviceMessage?> ProcessAsync(
        ReadOnlyMemory<byte> packet,
        IDeviceSession session,
        CancellationToken cancellationToken = default)
    {
        foreach (var plugin in _plugins)
        {
            if (!plugin.CanHandle(packet.Span))
                continue;

            return await plugin.DecodeAsync(
                packet,
                session,
                cancellationToken);
        }

        return null;
    }
}