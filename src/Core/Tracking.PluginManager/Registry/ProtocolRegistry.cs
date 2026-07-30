using Tracking.SDK.Interfaces;

namespace Tracking.PluginManager.Registry;
public sealed class ProtocolRegistry
{
    private readonly Dictionary<string, IProtocolPlugin> _plugins = new();

    public IReadOnlyCollection<IProtocolPlugin> Plugins => _plugins.Values;

    public void Register(IProtocolPlugin plugin)
    {
        if (_plugins.ContainsKey(plugin.Manifest.Id))
            throw new InvalidOperationException(
                $"Plugin '{plugin.Manifest.Id}' is already registered.");

        _plugins.Add(plugin.Manifest.Id, plugin);
    }

    public IProtocolPlugin? Find(ReadOnlySpan<byte> packet)
    {
        foreach (var plugin in _plugins.Values)
        {
            if (plugin.CanHandle(packet))
                return plugin;
        }

        return null;
    }
    public IProtocolPlugin? Get(string protocolId)
{
    _plugins.TryGetValue(protocolId, out var plugin);
    return plugin;
}

public bool TryGet(
    string protocolId,
    out IProtocolPlugin? plugin)
{
    return _plugins.TryGetValue(
        protocolId,
        out plugin);
}
}