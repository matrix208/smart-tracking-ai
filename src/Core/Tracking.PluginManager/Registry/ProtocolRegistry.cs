using Tracking.SDK.Interfaces;

namespace Tracking.PluginManager.Registry;

public sealed class ProtocolRegistry
{
    private readonly Dictionary<string, IProtocolPlugin> _plugins =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(IProtocolPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var id = plugin.Manifest.Id;

        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException(
                "Plugin manifest id is required.");

        _plugins[id] = plugin;
    }

    public bool Remove(string id)
    {
        return _plugins.Remove(id);
    }

    public bool Contains(string id)
    {
        return _plugins.ContainsKey(id);
    }

    public IProtocolPlugin? Get(string id)
    {
        return _plugins.TryGetValue(
            id,
            out var plugin)
            ? plugin
            : null;
    }

    public IReadOnlyCollection<IProtocolPlugin> GetAll()
    {
        return _plugins.Values.ToList();
    }
}
