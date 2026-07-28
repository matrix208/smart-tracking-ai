using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Services;
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
}