using Tracking.SDK.Interfaces;

namespace Tracking.Core.Services;

public sealed class ProtocolRegistry
{
    private readonly Dictionary<string, IProtocolPlugin> _plugins = new();

    public void Register(IProtocolPlugin plugin)
    {
        _plugins[plugin.Manifest.Id] = plugin;
    }

    public IProtocolPlugin? Find(string id)
    {
        return _plugins.TryGetValue(id, out var plugin)
            ? plugin
            : null;
    }

    public IReadOnlyCollection<IProtocolPlugin> GetAll()
    {
        return _plugins.Values;
    }
}