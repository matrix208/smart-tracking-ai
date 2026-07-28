using Tracking.PluginLoader.Models;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Services;

public sealed class PluginManager
{
    private readonly PluginScanner _scanner;
    private readonly ManifestReader _manifestReader;
    private readonly AssemblyPluginLoader _loader;
    private readonly ProtocolRegistry _registry;

    public PluginManager(
        PluginScanner scanner,
        ManifestReader manifestReader,
        AssemblyPluginLoader loader,
        ProtocolRegistry registry)
    {
        _scanner = scanner;
        _manifestReader = manifestReader;
        _loader = loader;
        _registry = registry;
    }

    public IReadOnlyCollection<IProtocolPlugin> Plugins
        => _registry.Plugins;

    public async Task LoadAllAsync(
        string repositoryFolder,
        CancellationToken cancellationToken = default)
    {
        var folders = _scanner.Scan(repositoryFolder);

        foreach (var folder in folders)
        {
            PluginPackage package =
                await _manifestReader.ReadAsync(folder, cancellationToken);

            var plugin = _loader.Load(package);

            _registry.Register(plugin);

            Console.WriteLine(
                $"Loaded plugin: {plugin.Manifest.Name} ({plugin.Manifest.Id})");
        }
    }

    public IProtocolPlugin? Find(ReadOnlySpan<byte> packet)
    {
        return _registry.Find(packet);
    }
}