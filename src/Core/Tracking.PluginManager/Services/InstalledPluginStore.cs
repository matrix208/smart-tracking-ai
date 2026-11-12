using System.Text.Json;

namespace Tracking.PluginManager.Services;

public sealed class InstalledPluginStore
{
    private readonly string _root;
    private readonly object _sync = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public InstalledPluginStore(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException(
                "Plugin storage root is required.",
                nameof(root));

        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public string Root => _root;

    public bool IsInstalled(string pluginId)
    {
        lock (_sync)
        {
            var directory = GetPluginDirectory(pluginId);
            return Directory.Exists(directory) &&
                   GetManifestPath(directory) is not null;
        }
    }

    public IReadOnlyList<InstalledPluginRecord> GetAll()
    {
        lock (_sync)
        {
            if (!Directory.Exists(_root))
                return [];

            var result = new List<InstalledPluginRecord>();

            foreach (var directory in Directory.GetDirectories(_root))
            {
                var manifestPath = GetManifestPath(directory);

                if (manifestPath is null)
                    continue;

                InstalledPluginManifest? manifest;

                try
                {
                    manifest =
                        JsonSerializer.Deserialize<InstalledPluginManifest>(
                            File.ReadAllText(manifestPath),
                            JsonOptions);
                }
                catch (JsonException)
                {
                    continue;
                }

                if (manifest is null ||
                    string.IsNullOrWhiteSpace(manifest.Id))
                {
                    continue;
                }

                var expectedDirectory =
                    GetPluginDirectory(manifest.Id);

                if (!string.Equals(
                    Path.GetFullPath(directory),
                    expectedDirectory,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(new InstalledPluginRecord
                {
                    Id = manifest.Id,
                    Name = manifest.Name,
                    Version = manifest.Version,
                    Directory = directory,
                    ManifestPath = manifestPath,
                    Enabled = ReadEnabled(
                        manifest.Id,
                        manifest.Enabled),
                    Assembly = manifest.Assembly,
                    EntryPoint = manifest.EntryPoint
                });
            }

            return result
                .OrderBy(x => x.Name)
                .ToList();
        }
    }

    public InstalledPluginRecord? Get(string pluginId)
    {
        lock (_sync)
        {
            var safeId = ValidatePluginId(pluginId);

            return GetAll().FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    safeId,
                    StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool SetEnabled(string pluginId, bool enabled)
    {
        lock (_sync)
        {
            var directory = GetPluginDirectory(pluginId);

            if (GetManifestPath(directory) is null)
                return false;

            WriteEnabled(pluginId, enabled);
            return true;
        }
    }

    public bool Remove(string pluginId)
    {
        lock (_sync)
        {
            var directory = GetPluginDirectory(pluginId);

            if (!Directory.Exists(directory))
                return false;

            Directory.Delete(directory, recursive: true);

            var statePath = GetStatePath(pluginId);

            if (File.Exists(statePath))
                File.Delete(statePath);

            return true;
        }
    }

    public void InstallFromDirectory(
        string sourceDirectory,
        string pluginId)
    {
        lock (_sync)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Plugin source directory does not exist: {sourceDirectory}");
            }

            var safeId = ValidatePluginId(pluginId);
            var targetDirectory = GetPluginDirectory(safeId);

            var previous = Get(safeId);
            bool? previousEnabled = previous?.Enabled;

            var stagingDirectory = Path.Combine(
                _root,
                $".{safeId}.staging-{Guid.NewGuid():N}");

            var backupDirectory = Path.Combine(
                _root,
                $".{safeId}.backup-{Guid.NewGuid():N}");

            var statePath = GetStatePath(safeId);
            var temporaryStatePath = statePath +
                "." + Guid.NewGuid().ToString("N") + ".tmp";

            bool previousMoved = false;
            bool newInstallationMoved = false;

            try
            {
                Directory.CreateDirectory(stagingDirectory);

                CopyDirectory(
                    sourceDirectory,
                    stagingDirectory);

                var manifestPath = GetManifestPath(stagingDirectory);

                if (manifestPath is null)
                {
                    throw new InvalidOperationException(
                        $"Plugin '{safeId}' does not contain a valid manifest.");
                }

                var manifest =
                    JsonSerializer.Deserialize<InstalledPluginManifest>(
                        File.ReadAllText(manifestPath),
                        JsonOptions)
                    ?? throw new InvalidDataException(
                        "Invalid plugin manifest.");

                if (string.IsNullOrWhiteSpace(manifest.Id) ||
                    !string.Equals(
                        manifest.Id,
                        safeId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "Package manifest id does not match the installation id.");
                }

                // Preserve the previous enabled state during upgrades.
                bool enabledState =
                    previousEnabled ?? manifest.Enabled;

                // Prepare the state file before replacing the installation.
                var stateDirectory = Path.GetDirectoryName(statePath)!;
                Directory.CreateDirectory(stateDirectory);

                File.WriteAllText(
                    temporaryStatePath,
                    JsonSerializer.Serialize(
                        new PluginState { Enabled = enabledState },
                        JsonOptions));

                // Move the existing installation to a backup.
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Move(
                        targetDirectory,
                        backupDirectory);

                    previousMoved = true;
                }

                // Activate the prepared installation.
                Directory.Move(
                    stagingDirectory,
                    targetDirectory);

                newInstallationMoved = true;

                // Commit the external state atomically.
                File.Move(
                    temporaryStatePath,
                    statePath,
                    overwrite: true);

                // Installation is committed. Backup cleanup is best-effort.
                if (previousMoved)
                {
                    try
                    {
                        Directory.Delete(
                            backupDirectory,
                            recursive: true);

                        previousMoved = false;
                    }
                    catch
                    {
                        // Keep the backup for manual recovery.
                    }
                }
            }
            catch
            {
                // Remove the new installation if the transaction failed.
                if (newInstallationMoved &&
                    Directory.Exists(targetDirectory))
                {
                    try
                    {
                        Directory.Delete(
                            targetDirectory,
                            recursive: true);

                        newInstallationMoved = false;
                    }
                    catch
                    {
                        // Preserve the backup if rollback cannot complete.
                    }
                }

                // Restore the previous installation.
                if (previousMoved &&
                    Directory.Exists(backupDirectory) &&
                    !Directory.Exists(targetDirectory))
                {
                    try
                    {
                        Directory.Move(
                            backupDirectory,
                            targetDirectory);

                        previousMoved = false;
                    }
                    catch
                    {
                        // Preserve the backup for manual recovery.
                    }
                }

                throw;
            }
            finally
            {
                if (Directory.Exists(stagingDirectory))
                {
                    try
                    {
                        Directory.Delete(
                            stagingDirectory,
                            recursive: true);
                    }
                    catch
                    {
                        // Do not hide the original result.
                    }
                }

                if (File.Exists(temporaryStatePath))
                {
                    try
                    {
                        File.Delete(temporaryStatePath);
                    }
                    catch
                    {
                        // Temporary state can be cleaned up later.
                    }
                }
            }
        }
    }

    private bool ReadEnabled(
        string pluginId,
        bool defaultValue)
    {
        var statePath = GetStatePath(pluginId);

        if (!File.Exists(statePath))
            return defaultValue;

        try
        {
            var state = JsonSerializer.Deserialize<PluginState>(
                File.ReadAllText(statePath),
                JsonOptions);

            return state?.Enabled ?? defaultValue;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Plugin state file is invalid: {pluginId}",
                ex);
        }
    }

    private void WriteEnabled(
        string pluginId,
        bool enabled)
    {
        var stateDirectory = Path.Combine(_root, ".state");
        Directory.CreateDirectory(stateDirectory);

        var statePath = GetStatePath(pluginId);

        var temporaryPath = statePath +
            "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(
                temporaryPath,
                JsonSerializer.Serialize(
                    new PluginState { Enabled = enabled },
                    JsonOptions));

            File.Move(
                temporaryPath,
                statePath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private string GetStatePath(string pluginId)
    {
        var safeId = ValidatePluginId(pluginId);

        return Path.Combine(
            _root,
            ".state",
            safeId.ToLowerInvariant() + ".json");
    }

    private string GetPluginDirectory(string pluginId)
    {
        var safeId = ValidatePluginId(pluginId);

        return Path.Combine(_root, safeId);
    }

    private static string ValidatePluginId(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            throw new ArgumentException(
                "Plugin id is required.",
                nameof(pluginId));
        }

        var safeId = pluginId.Trim();

        if (safeId.Length > 64 ||
            safeId == "." ||
            safeId == ".." ||
            safeId[0] == '.' ||
            safeId.Any(c =>
                !(char.IsAsciiLetterOrDigit(c) ||
                  c == '.' ||
                  c == '_' ||
                  c == '-')))
        {
            throw new InvalidOperationException(
                "Invalid plugin id.");
        }

        return safeId;
    }

    private static string? GetManifestPath(string pluginDirectory)
    {
        var canonical = Path.Combine(
            pluginDirectory,
            "Manifest",
            "manifest.json");

        if (File.Exists(canonical))
            return canonical;

        var legacy = Path.Combine(
            pluginDirectory,
            "manifest.json");

        if (File.Exists(legacy))
            return legacy;

        return null;
    }

    private static void CopyDirectory(
        string source,
        string destination)
    {
        foreach (var directory in Directory.GetDirectories(
            source,
            "*",
            SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(
                source,
                directory);

            Directory.CreateDirectory(
                Path.Combine(destination, relative));
        }

        foreach (var file in Directory.GetFiles(
            source,
            "*",
            SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(
                source,
                file);

            var target = Path.Combine(
                destination,
                relative);

            Directory.CreateDirectory(
                Path.GetDirectoryName(target)!);

            File.Copy(
                file,
                target,
                overwrite: false);
        }
    }

    private sealed class PluginState
    {
        public bool Enabled { get; set; } = true;
    }
}

public sealed class InstalledPluginManifest
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public string Author { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string EntryPoint { get; set; } = "";
    public string Assembly { get; set; } = "";
    public string SdkVersion { get; set; } = "";
    public int DefaultPort { get; set; }
    public bool SupportsTcp { get; set; }
    public bool SupportsUdp { get; set; }
    public List<string> Models { get; set; } = [];
    public List<string> Capabilities { get; set; } = [];
    public bool Enabled { get; set; } = true;
}

public sealed class InstalledPluginRecord
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Directory { get; init; } = "";
    public string ManifestPath { get; init; } = "";
    public bool Enabled { get; init; }
    public string Assembly { get; init; } = "";
    public string EntryPoint { get; init; } = "";
}
