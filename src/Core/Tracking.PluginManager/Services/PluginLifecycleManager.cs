using Tracking.Plugin.Abstractions.Interfaces;
using Tracking.PluginLoader.Models;
using Tracking.PluginLoader.Services;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginManager.Services;

/// <summary>
/// Coordinates installation, discovery, validation, loading,
/// registration, enabling, disabling and uninstallation.
/// </summary>
public sealed class PluginLifecycleManager
{
    private readonly InstalledPluginStore _store;
    private readonly ProtocolPluginManager _pluginManager;
    private readonly IPluginInstaller _installer;
    private readonly ManifestReader _manifestReader = new();
    private readonly AssemblyPluginLoader _assemblyLoader = new();

    public PluginLifecycleManager(
        InstalledPluginStore store,
        ProtocolPluginManager pluginManager,
        IPluginInstaller installer)
    {
        _store = store;
        _pluginManager = pluginManager;
        _installer = installer;
    }

    public async Task<PluginLifecycleResult> InstallAsync(
        string packageFilePath,
        CancellationToken cancellationToken = default)
    {
        var installResult = await _installer.InstallAsync(
            packageFilePath,
            cancellationToken);

        return await LoadAndRegisterAsync(
            installResult.PluginId,
            cancellationToken);
    }

    public InstalledPluginRecord? Discover(string pluginId)
    {
        return _store.Get(pluginId);
    }

    public IReadOnlyList<InstalledPluginRecord> DiscoverAll()
    {
        return _store.GetAll();
    }

    private async Task<PluginPackage> ValidateAsync(
        string pluginFolder,
        CancellationToken cancellationToken)
    {
        return await _manifestReader.ReadAsync(
            pluginFolder,
            cancellationToken);
    }

    private IProtocolPlugin Load(PluginPackage package)
    {
        return _assemblyLoader.Load(package);
    }

    public async Task<PluginLifecycleResult> LoadAndRegisterAsync(
        string pluginId,
        CancellationToken cancellationToken = default)
    {
        var record = Discover(pluginId)
            ?? throw new InvalidOperationException(
                $"Plugin '{pluginId}' is not installed.");

        cancellationToken.ThrowIfCancellationRequested();

        // Verify the retained signed archive and compare every
        // installed payload file before reading or loading the plugin.
        _installer.VerifyInstalled(record.Directory);

        var package = await ValidateAsync(
            record.Directory,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var plugin = Load(package);

        _pluginManager.Register(
            plugin,
            record.Enabled);

        return new PluginLifecycleResult
        {
            PluginId = plugin.Manifest.Id,
            Name = plugin.Manifest.Name,
            Version = plugin.Manifest.Version,
            Enabled = record.Enabled
        };
    }

    public async Task<IReadOnlyList<PluginLifecycleResult>> LoadAllEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<PluginLifecycleResult>();

        foreach (var record in DiscoverAll().Where(x => x.Enabled))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                results.Add(
                    await LoadAndRegisterAsync(
                        record.Id,
                        cancellationToken));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Plugin '{record.Id}' failed to load during startup: {ex.Message}");
            }
        }

        return results;
    }

    public async Task<bool> EnableAsync(
        string pluginId,
        CancellationToken cancellationToken = default)
    {
        var record = Discover(pluginId);

        if (record is null)
            return false;

        if (_pluginManager.Get(pluginId) is null)
        {
            await LoadAndRegisterAsync(
                pluginId,
                cancellationToken);
        }

        if (!_store.SetEnabled(pluginId, true))
            return false;

        if (_pluginManager.Get(pluginId) is not null)
            _pluginManager.Enable(pluginId);

        return true;
    }

    public bool Disable(string pluginId)
    {
        var persisted = _store.SetEnabled(pluginId, false);

        if (!persisted)
            return false;

        _pluginManager.Disable(pluginId);
        return true;
    }

    public bool Uninstall(string pluginId)
    {
        var removed = _installer.Uninstall(pluginId);

        if (removed)
            _pluginManager.Unregister(pluginId);

        return removed;
    }
}

public sealed class PluginLifecycleResult
{
    public required string PluginId { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public bool Enabled { get; init; }
}
