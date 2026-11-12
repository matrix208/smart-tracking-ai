using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tracking.PluginManager.Configuration;

namespace Tracking.API.Plugins;

public sealed class PluginRepositoryService
{
    private readonly string _repositoryRoot;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PluginRepositoryService(
        IOptions<PluginManagerOptions> options,
        IHostEnvironment environment)
    {
        _repositoryRoot =
            ResolvePath(
                options.Value.RepositoryPath,
                environment.ContentRootPath);
    }

    public IReadOnlyList<PluginRepositoryPackage> GetPackages()
    {
        if (!Directory.Exists(_repositoryRoot))
            return [];

        var packages =
            new List<PluginRepositoryPackage>();

        foreach (var directory in
                 Directory.GetDirectories(
                     _repositoryRoot))
        {
            var manifestPath =
                Path.Combine(
                    directory,
                    "manifest.json");

            if (!File.Exists(manifestPath))
                continue;

            try
            {
                var json =
                    File.ReadAllText(
                        manifestPath);

                var manifest =
                    JsonSerializer.Deserialize<PluginRepositoryManifest>(
                        json,
                        JsonOptions);

                if (manifest is null ||
                    string.IsNullOrWhiteSpace(
                        manifest.PackageId))
                {
                    continue;
                }

                packages.Add(
                    new PluginRepositoryPackage
                    {
                        PackageId = manifest.PackageId,
                        DisplayName = manifest.DisplayName,
                        Description = manifest.Description,
                        Version = manifest.Version,
                        SdkVersion = manifest.SdkVersion,
                        MinServerVersion = manifest.MinServerVersion,
                        Manufacturer = manifest.Manufacturer,
                        Company = manifest.Company,
                        Author = manifest.Author,
                        Type = manifest.Type,
                        Assembly = manifest.Assembly,
                        EntryPoint = manifest.EntryPoint,
                        Icon = manifest.Icon,
                        Readme = manifest.Readme,
                        License = manifest.License,
                        DefaultPort = manifest.DefaultPort,
                        SupportsTcp = manifest.SupportsTcp,
                        SupportsUdp = manifest.SupportsUdp,
                        Permissions =
                            manifest.Permissions ?? [],
                        Dependencies =
                            manifest.Dependencies ?? []
                    });
            }
            catch
            {
                // Ignore invalid repository packages.
            }
        }

        return packages
            .OrderBy(x => x.DisplayName)
            .ToList();
    }

    public string? GetPackageDirectory(
        string packageId)
    {
        var packageRoot =
            Path.Combine(
                _repositoryRoot,
                packageId);

        if (!Directory.Exists(packageRoot))
            return null;

        // A repository entry contains metadata at its root and
        // the actual installable plugin payload under /package.
        var packageDirectory =
            Path.Combine(
                packageRoot,
                "package");

        return Directory.Exists(packageDirectory)
            ? packageDirectory
            : packageRoot;
    }

    public string? GetPackagePayloadDirectory(
        string packageId)
    {
        var packageDirectory = Path.Combine(
            _repositoryRoot,
            packageId,
            "package");

        return Directory.Exists(packageDirectory)
            ? packageDirectory
            : null;
    }

    private static string ResolvePath(
        string path,
        string contentRootPath)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(
            Path.Combine(
                contentRootPath,
                path));
    }
}

public sealed class PluginRepositoryManifest
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; } = "";

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("sdkVersion")]
    public string SdkVersion { get; set; } = "";

    [JsonPropertyName("minServerVersion")]
    public string MinServerVersion { get; set; } = "";

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = "";

    [JsonPropertyName("company")]
    public string Company { get; set; } = "";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("assembly")]
    public string Assembly { get; set; } = "";

    [JsonPropertyName("entryPoint")]
    public string EntryPoint { get; set; } = "";

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "";

    [JsonPropertyName("readme")]
    public string Readme { get; set; } = "";

    [JsonPropertyName("license")]
    public string License { get; set; } = "";

    [JsonPropertyName("defaultPort")]
    public int DefaultPort { get; set; }

    [JsonPropertyName("supportsTcp")]
    public bool SupportsTcp { get; set; }

    [JsonPropertyName("supportsUdp")]
    public bool SupportsUdp { get; set; }

    [JsonPropertyName("permissions")]
    public List<string>? Permissions { get; set; }

    [JsonPropertyName("dependencies")]
    public List<string>? Dependencies { get; set; }
}

public sealed class PluginRepositoryPackage
{
    public string PackageId { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string Description { get; init; } = "";

    public string Version { get; init; } = "";

    public string SdkVersion { get; init; } = "";

    public string MinServerVersion { get; init; } = "";

    public string Manufacturer { get; init; } = "";

    public string Company { get; init; } = "";

    public string Author { get; init; } = "";

    public int Type { get; init; }

    public string Assembly { get; init; } = "";

    public string EntryPoint { get; init; } = "";

    public string Icon { get; init; } = "";

    public string Readme { get; init; } = "";

    public string License { get; init; } = "";

    public int DefaultPort { get; init; }

    public bool SupportsTcp { get; init; }

    public bool SupportsUdp { get; init; }

    public IReadOnlyList<string> Permissions { get; init; } = [];

    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
