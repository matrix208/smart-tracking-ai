using Tracking.Core.Models;

namespace Tracking.Core.Services;

public sealed class SessionPipeline
{
    private readonly ProtocolRegistry _registry;

    public SessionPipeline(ProtocolRegistry registry)
    {
        _registry = registry;
    }

    public async Task ProcessAsync(PacketContext context)
    {
        Console.WriteLine(
            $"Packet received ({context.Packet.Length} bytes)");

        // هنا لاحقاً سيتم اختيار الـ Plugin المناسب
        await Task.CompletedTask;
    }
}