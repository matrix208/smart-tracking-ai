using Tracking.SDK.Interfaces;
using Tracking.PluginManager.Registry;

namespace Tracking.PluginManager.Services;

public sealed class ProtocolPluginManager
{
    private readonly ProtocolRegistry _registry = new();

    public IReadOnlyCollection<IProtocolPlugin> Plugins =>
        _registry.Plugins;

    public void Register(
        IEnumerable<IProtocolPlugin> plugins)
    {
        foreach (var plugin in plugins)
        {
            _registry.Register(plugin);

            Console.WriteLine(
                $"Loaded Plugin : {plugin.Manifest.Name}");
        }
    }
    public IProtocolPlugin? Get(
    string protocolId)
{
    return _registry.Get(protocolId);
}

public bool TryGet(
    string protocolId,
    out IProtocolPlugin? plugin)
{
    return _registry.TryGet(
        protocolId,
        out plugin);
}

    public IProtocolPlugin? Find(
        ReadOnlySpan<byte> packet)
    {
        return _registry.Find(packet);
    }
}