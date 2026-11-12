using Tracking.PluginManager.Registry;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginManager.Services;

public sealed class ProtocolPluginManager
{
    private readonly ProtocolRegistry _registry = new();

    private readonly Dictionary<string, bool> _enabled =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly object _sync = new();

    public IReadOnlyCollection<IProtocolPlugin> Plugins =>
        _registry.GetAll();

    // =========================================================
    // Register
    // =========================================================

    public void Register(
        IEnumerable<IProtocolPlugin> plugins)
    {
        lock (_sync)
        {
            foreach (var plugin in plugins)
            {
                RegisterInternal(
                    plugin,
                    enabled: true);
            }
        }
    }

    public bool Register(
        IProtocolPlugin plugin,
        bool enabled)
    {
        lock (_sync)
        {
            RegisterInternal(
                plugin,
                enabled);

            return true;
        }
    }

    private void RegisterInternal(
        IProtocolPlugin plugin,
        bool enabled)
    {
        _registry.Register(plugin);

        _enabled[plugin.Manifest.Id] = enabled;
    }

    // =========================================================
    // Unregister
    // =========================================================

    public bool Unregister(
        string pluginId)
    {
        lock (_sync)
        {
            var plugin = _registry
                .GetAll()
                .FirstOrDefault(x =>
                    string.Equals(
                        x.Manifest.Id,
                        pluginId,
                        StringComparison.OrdinalIgnoreCase));

            if (plugin is null)
                return false;

            _registry.Remove(plugin.Manifest.Id);
            _enabled.Remove(plugin.Manifest.Id);

            return true;
        }
    }

    // =========================================================
    // Enable
    // =========================================================

    public bool Enable(
        string pluginId)
    {
        lock (_sync)
        {
            if (!_registry.Contains(pluginId))
                return false;

            _enabled[pluginId] = true;

            return true;
        }
    }

    // =========================================================
    // Disable
    // =========================================================

    public bool Disable(
        string pluginId)
    {
        lock (_sync)
        {
            if (!_registry.Contains(pluginId))
                return false;

            _enabled[pluginId] = false;

            return true;
        }
    }

    // =========================================================
    // State
    // =========================================================

    public bool IsEnabled(
        string pluginId)
    {
        lock (_sync)
        {
            return _enabled.TryGetValue(
                       pluginId,
                       out var enabled) &&
                   enabled;
        }
    }

    // =========================================================
    // Protocol Detection
    // =========================================================

    /// <summary>
    /// Finds the first enabled plugin capable of handling
    /// the packet.
    ///
    /// Disabled plugins are never used for protocol detection.
    /// </summary>
    public IProtocolPlugin? Find(
        ReadOnlySpan<byte> packet)
    {
        lock (_sync)
        {
            foreach (var plugin in _registry.GetAll())
            {
                if (!_enabled.TryGetValue(
                        plugin.Manifest.Id,
                        out var enabled) ||
                    !enabled)
                {
                    continue;
                }

                try
                {
                    if (plugin.CanHandle(packet))
                        return plugin;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Plugin '{plugin.Manifest.Id}' failed during CanHandle: " +
                        ex.Message);
                }
            }

            return null;
        }
    }

    // =========================================================
    // Get
    // =========================================================

    public IProtocolPlugin? Get(
        string pluginId)
    {
        lock (_sync)
        {
            return _registry
                .GetAll()
                .FirstOrDefault(plugin =>
                    string.Equals(
                        plugin.Manifest.Id,
                        pluginId,
                        StringComparison.OrdinalIgnoreCase));
        }
    }

    // =========================================================
    // Enabled Plugins
    // =========================================================

    public IReadOnlyCollection<IProtocolPlugin>
        GetEnabledPlugins()
    {
        lock (_sync)
        {
            return _registry
                .GetAll()
                .Where(plugin =>
                    _enabled.TryGetValue(
                        plugin.Manifest.Id,
                        out var enabled) &&
                    enabled)
                .ToList();
        }
    }

    // =========================================================
    // Runtime State
    // =========================================================

    public PluginRuntimeState? GetState(
        string pluginId)
    {
        lock (_sync)
        {
            var plugin = _registry
                .GetAll()
                .FirstOrDefault(x =>
                    string.Equals(
                        x.Manifest.Id,
                        pluginId,
                        StringComparison.OrdinalIgnoreCase));

            if (plugin is null)
                return null;

            var enabled =
                _enabled.TryGetValue(
                    plugin.Manifest.Id,
                    out var value) &&
                value;

            return new PluginRuntimeState(
                plugin.Manifest.Id,
                plugin.Manifest.Name,
                plugin.Manifest.Version,
                enabled);
        }
    }

    public IReadOnlyCollection<PluginRuntimeState>
        GetStates()
    {
        lock (_sync)
        {
            return _registry
                .GetAll()
                .Select(plugin =>
                    new PluginRuntimeState(
                        plugin.Manifest.Id,
                        plugin.Manifest.Name,
                        plugin.Manifest.Version,
                        _enabled.TryGetValue(
                            plugin.Manifest.Id,
                            out var enabled) &&
                        enabled))
                .ToList();
        }
    }
}

public sealed record PluginRuntimeState(
    string Id,
    string Name,
    string Version,
    bool Enabled);
