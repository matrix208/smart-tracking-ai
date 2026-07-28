using System.Text.Json;
using Tracking.PluginLoader.Models;
using Tracking.SDK.Metadata;

namespace Tracking.PluginLoader.Services;

public sealed class ManifestReader
{
    public async Task<PluginPackage> ReadAsync(
        string pluginFolder,
        CancellationToken cancellationToken = default)
    {
        pluginFolder = Path.GetFullPath(pluginFolder);

        var manifestFile = Path.Combine(pluginFolder, "manifest.json");

        if (!File.Exists(manifestFile))
            throw new FileNotFoundException(
                $"manifest.json not found in {pluginFolder}");

        await using var stream = File.OpenRead(manifestFile);

      var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

var manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(
    stream,
    options,
    cancellationToken);

        if (manifest is null)
Console.WriteLine($"Manifest type = {manifest?.GetType().FullName}");

        Console.WriteLine($"Manifest.Id         = '{manifest.Id}'");
        Console.WriteLine($"Manifest.Name       = '{manifest.Name}'");
        Console.WriteLine($"Manifest.EntryPoint = '{manifest.EntryPoint}'");
        Console.WriteLine($"Manifest.Assembly   = '{manifest.Assembly}'");

        return new PluginPackage
        {
            Folder = pluginFolder,
            Manifest = manifest,
            AssemblyPath = Path.GetFullPath(
                Path.Combine(pluginFolder, manifest.Assembly))
        };
    }
}