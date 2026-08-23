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
        Console.WriteLine(
            $"Pipeline received packet: {Convert.ToHexString(packet.Span)}");

        foreach (var plugin in _plugins)
        {
            Console.WriteLine(
                $"Checking plugin: {plugin.Manifest.Name}");

            var canHandle = plugin.CanHandle(packet.Span);

            Console.WriteLine(
                $"CanHandle: {canHandle}");

            if (!canHandle)
                continue;

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

        Console.WriteLine(
            "No plugin handled packet");

        return null;
    }
}