using Tracking.PluginLoader.Abstractions;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Services;

public sealed class PluginLoader : IPluginLoader
{
    private readonly PluginScanner _scanner = new();
    private readonly ManifestReader _manifestReader = new();
    private readonly AssemblyPluginLoader _assemblyLoader = new();

    public async Task<IReadOnlyList<IProtocolPlugin>> LoadAsync(
        string pluginsPath,
        CancellationToken cancellationToken = default)
    {
        var plugins = new List<IProtocolPlugin>();

        foreach (var folder in _scanner.Scan(pluginsPath))
        {
            var package = await _manifestReader.ReadAsync(
                folder,
                cancellationToken);

            var plugin = _assemblyLoader.Load(package);

            plugins.Add(plugin);
        }

        return plugins;
    }
}