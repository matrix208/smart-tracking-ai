using Tracking.SDK.Interfaces;

namespace Tracking.Core.Services;

public sealed class PluginManager
{
    private readonly List<IProtocolPlugin> _plugins = [];

    public void Register(IProtocolPlugin plugin)
    {
        _plugins.Add(plugin);
    }

    public IReadOnlyList<IProtocolPlugin> All => _plugins;

    public IProtocolPlugin? Find(ReadOnlySpan<byte> packet)
    {
        foreach (var plugin in _plugins)
        {
            if (plugin.CanHandle(packet))
                return plugin;
        }

        return null;
    }
}