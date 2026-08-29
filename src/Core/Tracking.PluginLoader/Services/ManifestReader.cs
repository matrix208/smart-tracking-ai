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

        var manifestFile = ResolveManifestPath(pluginFolder);

        if (manifestFile is null)
        {
            throw new FileNotFoundException(
                $"manifest.json not found in {pluginFolder}");
        }

        await using var stream = File.OpenRead(manifestFile);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var manifest =
            await JsonSerializer.DeserializeAsync<PluginManifest>(
                stream,
                options,
                cancellationToken);

        if (manifest is null)
        {
            throw new InvalidDataException(
                "Invalid plugin manifest.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            throw new InvalidDataException(
                "Plugin manifest Id is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Assembly))
        {
            throw new InvalidDataException(
                $"Plugin '{manifest.Id}' does not define an assembly.");
        }

        if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
        {
            throw new InvalidDataException(
                $"Plugin '{manifest.Id}' does not define an entry point.");
        }

        var assemblyPath = Path.GetFullPath(
            Path.Combine(
                pluginFolder,
                manifest.Assembly));

        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException(
                $"Plugin assembly '{manifest.Assembly}' was not found.",
                assemblyPath);
        }

        Console.WriteLine(
            $"Manifest.Id         = '{manifest.Id}'");

        Console.WriteLine(
            $"Manifest.Name       = '{manifest.Name}'");

        Console.WriteLine(
            $"Manifest.Version    = '{manifest.Version}'");

        Console.WriteLine(
            $"Manifest.EntryPoint = '{manifest.EntryPoint}'");

        Console.WriteLine(
            $"Manifest.Assembly   = '{manifest.Assembly}'");

        Console.WriteLine(
            $"ManifestPath        = '{manifestFile}'");

        Console.WriteLine(
            $"AssemblyPath        = '{assemblyPath}'");

        return new PluginPackage
        {
            Folder = pluginFolder,
            Manifest = manifest,
            AssemblyPath = assemblyPath
        };
    }

    private static string? ResolveManifestPath(string pluginFolder)
    {
        var canonical = Path.Combine(
            pluginFolder,
            "Manifest",
            "manifest.json");

        if (File.Exists(canonical))
            return canonical;

        var legacy = Path.Combine(
            pluginFolder,
            "manifest.json");

        if (File.Exists(legacy))
            return legacy;

        return null;
    }
}
