using Tracking.PluginLoader.Abstractions;
using Tracking.PluginLoader.Models;
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
        return await LoadAsync(
            pluginsPath,
            enabledPluginIds: null,
            cancellationToken);
    }

    public async Task<IReadOnlyList<IProtocolPlugin>> LoadAsync(
        string pluginsPath,
        IReadOnlySet<string>? enabledPluginIds,
        CancellationToken cancellationToken = default)
    {
        var plugins = new List<IProtocolPlugin>();

        if (!Directory.Exists(pluginsPath))
        {
            Console.WriteLine(
                $"Plugin directory does not exist: {pluginsPath}");

            return plugins;
        }

        foreach (var folder in _scanner.Scan(pluginsPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            PluginPackage package;

            try
            {
                package = await _manifestReader.ReadAsync(
                    folder,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Plugin skipped because manifest could not be loaded: {folder}");

                Console.WriteLine(
                    ex.Message);

                continue;
            }

            // =================================================
            // Enabled filter
            // =================================================
            //
            // When enabledPluginIds is supplied:
            //
            // ONLY plugins whose manifest Id exists in that
            // set are loaded.
            //
            // =================================================

            if (enabledPluginIds is not null &&
                !enabledPluginIds.Contains(package.Manifest.Id))
            {
                Console.WriteLine(
                    $"Plugin skipped because it is disabled: " +
                    $"{package.Manifest.Id}");

                continue;
            }

            try
            {
                var plugin = _assemblyLoader.Load(package);

                plugins.Add(plugin);

                Console.WriteLine(
                    $"Plugin loaded successfully: " +
                    $"{package.Manifest.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Plugin '{package.Manifest.Id}' failed to load: " +
                    $"{ex.Message}");

                throw;
            }
        }

        return plugins;
    }
}
