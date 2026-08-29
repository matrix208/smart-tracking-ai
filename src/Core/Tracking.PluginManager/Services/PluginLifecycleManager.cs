using Tracking.Plugin.Abstractions.Interfaces;
using Tracking.PluginLoader.Models;
using Tracking.PluginLoader.Services;
using Tracking.SDK.Interfaces;

namespace Tracking.PluginManager.Services;

/// <summary>
/// Orchestrates the full plugin lifecycle:
///
///   Install -> Discover -> Validate -> Load -> Register -> Enable/Disable -> Uninstall
///
/// InstalledPluginStore stays the on-disk source of truth.
/// ProtocolPluginManager stays the in-memory runtime registry.
/// This class is the only thing that should coordinate both.
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

    // =========================================================
    // INSTALL
    //
    // Verifies signature + extracts + validates manifest +
    // writes to InstalledPluginStore (via IPluginInstaller),
    // then immediately runs Discover -> Validate -> Load ->
    // Register so the plugin is live without a restart.
    // =========================================================

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

    // =========================================================
    // DISCOVER
    //
    // Reads what InstalledPluginStore currently has on disk.
    // =========================================================

    public InstalledPluginRecord? Discover(string pluginId)
    {
        return _store.Get(pluginId);
    }

    public IReadOnlyList<InstalledPluginRecord> DiscoverAll()
    {
        return _store.GetAll();
    }

    // =========================================================
    // VALIDATE
    //
    // ManifestReader re-reads Manifest/manifest.json from the
    // installed folder and enforces Id/Assembly/EntryPoint are
    // present and that the assembly file actually exists on disk.
    // This is a second, independent validation pass distinct
    // from ManifestValidator used during Install.
    // =========================================================

    private async Task<PluginPackage> ValidateAsync(
        string pluginFolder,
        CancellationToken cancellationToken)
    {
        return await _manifestReader.ReadAsync(
            pluginFolder,
            cancellationToken);
    }

    // =========================================================
    // LOAD
    //
    // Loads the assembly into an isolated AssemblyLoadContext
    // and instantiates the plugin's entry point type.
    // =========================================================

    private IProtocolPlugin Load(PluginPackage package)
    {
        return _assemblyLoader.Load(package);
    }

    // =========================================================
    // REGISTER
    //
    // Combines Discover + Validate + Load + Register for a
    // single already-installed plugin id.
    // =========================================================

    public async Task<PluginLifecycleResult> LoadAndRegisterAsync(
        string pluginId,
        CancellationToken cancellationToken = default)
    {
        var record = Discover(pluginId)
            ?? throw new InvalidOperationException(
                $"Plugin '{pluginId}' is not installed.");

        var package = await ValidateAsync(
            record.Directory,
            cancellationToken);

        var plugin = Load(package);

        _pluginManager.Register(plugin, record.Enabled);

        return new PluginLifecycleResult
        {
            PluginId = plugin.Manifest.Id,
            Name = plugin.Manifest.Name,
            Version = plugin.Manifest.Version,
            Enabled = record.Enabled
        };
    }

    /// <summary>
    /// Runs Discover -> Validate -> Load -> Register for every
    /// enabled plugin currently in InstalledPluginStore.
    /// Intended for use at runtime startup.
    /// </summary>
    public async Task<IReadOnlyList<PluginLifecycleResult>> LoadAllEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<PluginLifecycleResult>();

        foreach (var record in DiscoverAll().Where(x => x.Enabled))
        {
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

    // =========================================================
    // ENABLE
    //
    // Persists Enabled=true in InstalledPluginStore. If the
    // plugin is not currently loaded in memory (e.g. server
    // just started with it disabled), loads and registers it
    // now. Otherwise flips the in-memory flag on the already
    // registered instance.
    // =========================================================

    public async Task<bool> EnableAsync(
        string pluginId,
        CancellationToken cancellationToken = default)
    {
        if (!_store.SetEnabled(pluginId, true))
            return false;

        if (_pluginManager.Get(pluginId) is null)
        {
            await LoadAndRegisterAsync(pluginId, cancellationToken);
        }
        else
        {
            _pluginManager.Enable(pluginId);
        }

        return true;
    }

    // =========================================================
    // DISABLE
    //
    // Persists Enabled=false and flips the in-memory flag.
    // The plugin's assembly stays loaded in memory (no unload
    // support yet) but ProtocolPluginManager.Find() will skip
    // it during protocol detection because it is disabled.
    // =========================================================

    public bool Disable(string pluginId)
    {
        var persisted = _store.SetEnabled(pluginId, false);

        _pluginManager.Disable(pluginId);

        return persisted;
    }

    // =========================================================
    // UNINSTALL
    //
    // Removes the plugin from the in-memory registry AND from
    // disk via IPluginInstaller.Uninstall (-> InstalledPluginStore.Remove).
    // =========================================================

    public bool Uninstall(string pluginId)
    {
        _pluginManager.Unregister(pluginId);

        return _installer.Uninstall(pluginId);
    }
}

public sealed class PluginLifecycleResult
{
    public required string PluginId { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public bool Enabled { get; init; }
}
