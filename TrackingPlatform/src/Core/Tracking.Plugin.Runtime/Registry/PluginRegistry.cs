using System.Collections.Concurrent;
using Tracking.SDK.Interfaces;

namespace Tracking.Plugin.Runtime.Registry;

public sealed class PluginRegistry
{
    private readonly ConcurrentDictionary<string, IProtocolPlugin> _plugins = new();

    public bool Register(string id, IProtocolPlugin plugin)
    {
        return _plugins.TryAdd(id, plugin);
    }

    public bool Unregister(string id)
    {
        return _plugins.TryRemove(id, out _);
    }

    public bool TryGet(string id, out IProtocolPlugin? plugin)
    {
        return _plugins.TryGetValue(id, out plugin);
    }

    public IReadOnlyCollection<IProtocolPlugin> GetAll()
    {
        return _plugins.Values.ToList().AsReadOnly();
    }

    public bool Contains(string id)
    {
        return _plugins.ContainsKey(id);
    }

    public int Count => _plugins.Count;
}