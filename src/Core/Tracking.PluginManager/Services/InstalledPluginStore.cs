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
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public string Root => _root;

    public bool IsInstalled(string pluginId)
    {
        var directory = GetPluginDirectory(pluginId);
        var manifestPath = GetManifestPath(directory);

        return Directory.Exists(directory) &&
               manifestPath is not null;
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

                try
                {
                    var json = File.ReadAllText(manifestPath);

                    var manifest =
                        JsonSerializer.Deserialize<InstalledPluginManifest>(
                            json,
                            JsonOptions);

                    if (manifest is null ||
                        string.IsNullOrWhiteSpace(manifest.Id))
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
                        Enabled = manifest.Enabled,
                        Assembly = manifest.Assembly,
                        EntryPoint = manifest.EntryPoint
                    });
                }
                catch
                {
                }
            }

            return result
                .OrderBy(x => x.Name)
                .ToList();
        }
    }

    public InstalledPluginRecord? Get(string pluginId)
    {
        return GetAll()
            .FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    pluginId,
                    StringComparison.OrdinalIgnoreCase));
    }

    public bool SetEnabled(string pluginId, bool enabled)
    {
        lock (_sync)
        {
            var directory = GetPluginDirectory(pluginId);
            var manifestPath = GetManifestPath(directory);

            if (manifestPath is null)
                return false;

            var json = File.ReadAllText(manifestPath);

            var manifest =
                JsonSerializer.Deserialize<InstalledPluginManifest>(
                    json,
                    JsonOptions);

            if (manifest is null)
                return false;

            manifest.Enabled = enabled;

            File.WriteAllText(
                manifestPath,
                JsonSerializer.Serialize(
                    manifest,
                    JsonOptions));

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

            Directory.Delete(
                directory,
                recursive: true);

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

            var targetDirectory =
                GetPluginDirectory(pluginId);

            var previous = Get(pluginId);

            var previousEnabled =
                previous?.Enabled ?? true;

            if (Directory.Exists(targetDirectory))
            {
                Directory.Delete(
                    targetDirectory,
                    recursive: true);
            }

            Directory.CreateDirectory(targetDirectory);

            CopyDirectory(
                sourceDirectory,
                targetDirectory);

            var manifestPath =
                GetManifestPath(targetDirectory);

            if (manifestPath is null)
            {
                var rootManifestPath =
                    Path.Combine(
                        targetDirectory,
                        "manifest.json");

                if (File.Exists(rootManifestPath))
                {
                    var manifestDirectory =
                        Path.Combine(
                            targetDirectory,
                            "Manifest");

                    Directory.CreateDirectory(
                        manifestDirectory);

                    manifestPath =
                        Path.Combine(
                            manifestDirectory,
                            "manifest.json");

                    File.Move(
                        rootManifestPath,
                        manifestPath);
                }
            }

            if (manifestPath is null ||
                !File.Exists(manifestPath))
            {
                throw new InvalidOperationException(
                    $"Installed plugin '{pluginId}' does not contain Manifest/manifest.json.");
            }

            var json = File.ReadAllText(manifestPath);

            var manifest =
                JsonSerializer.Deserialize<InstalledPluginManifest>(
                    json,
                    JsonOptions)
                ?? throw new InvalidDataException(
                    "Invalid plugin manifest.");

            manifest.Id = pluginId;
            manifest.Enabled = previousEnabled;

            File.WriteAllText(
                manifestPath,
                JsonSerializer.Serialize(
                    manifest,
                    JsonOptions));
        }
    }

    private static string? GetManifestPath(string pluginDirectory)
    {
        var canonical =
            Path.Combine(
                pluginDirectory,
                "Manifest",
                "manifest.json");

        if (File.Exists(canonical))
            return canonical;

        var legacy =
            Path.Combine(
                pluginDirectory,
                "manifest.json");

        if (File.Exists(legacy))
            return legacy;

        return null;
    }

    private string GetPluginDirectory(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException(
                "Plugin id is required.",
                nameof(pluginId));

        var safeId =
            pluginId.Trim();

        if (safeId.Contains(Path.DirectorySeparatorChar) ||
            safeId.Contains(Path.AltDirectorySeparatorChar) ||
            safeId.Contains(".."))
        {
            throw new InvalidOperationException(
                "Invalid plugin id.");
        }

        return Path.Combine(
            _root,
            safeId);
    }

    private static void CopyDirectory(
        string source,
        string destination)
    {
        foreach (var directory in
                 Directory.GetDirectories(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            var relative =
                Path.GetRelativePath(
                    source,
                    directory);

            Directory.CreateDirectory(
                Path.Combine(
                    destination,
                    relative));
        }

        foreach (var file in
                 Directory.GetFiles(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            var relative =
                Path.GetRelativePath(
                    source,
                    file);

            var target =
                Path.Combine(
                    destination,
                    relative);

            Directory.CreateDirectory(
                Path.GetDirectoryName(target)!);

            File.Copy(
                file,
                target,
                overwrite: true);
        }
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
