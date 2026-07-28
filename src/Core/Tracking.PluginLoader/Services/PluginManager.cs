using Tracking.SDK.Interfaces;

namespace Tracking.PluginLoader.Services;

public sealed class PluginManager
{
    private readonly PluginScanner _scanner = new();
    private readonly ManifestReader _manifestReader = new();
    private readonly AssemblyPluginLoader _assemblyLoader = new();
    private readonly ProtocolRegistry _registry = new();

    public IReadOnlyCollection<IProtocolPlugin> Plugins
        => _registry.Plugins;

    public async Task LoadAsync(
        string pluginsFolder,
        CancellationToken cancellationToken = default)
    {
        var folders = _scanner.Scan(pluginsFolder);

        foreach (var folder in folders)
        {
            var package =
                await _manifestReader.ReadAsync(
                    folder,
                    cancellationToken);

            var plugin =
                _assemblyLoader.Load(package);

            _registry.Register(plugin);

            Console.WriteLine(
                $"Loaded Plugin : {plugin.Manifest.Name}");
        }
    }

    public IProtocolPlugin? Find(ReadOnlySpan<byte> packet)
    {
        return _registry.Find(packet);
    }
}