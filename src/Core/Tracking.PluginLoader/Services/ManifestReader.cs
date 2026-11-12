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

        var assemblyPath = ResolveAssemblyPath(
            pluginFolder,
            manifest.Assembly);

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


    private static string ResolveAssemblyPath(
        string pluginFolder,
        string assembly)
    {
        if (string.IsNullOrWhiteSpace(assembly) ||
            Path.IsPathRooted(assembly) ||
            assembly.Contains('\\') ||
            assembly.Contains(':') ||
            assembly.Contains('\0'))
            throw new InvalidDataException(
                "Plugin assembly path is unsafe.");

        var segments = assembly.Split('/');

        if (segments.Any(s =>
                string.IsNullOrWhiteSpace(s) || s is "." or ".."))
            throw new InvalidDataException(
                "Plugin assembly path contains an unsafe segment.");

        if (!assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "Plugin assembly must be a DLL.");

        string root = Path.GetFullPath(pluginFolder);
        string prefix = root.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        string fullPath = Path.GetFullPath(
            Path.Combine(root, Path.Combine(segments)));

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!fullPath.StartsWith(prefix, comparison))
            throw new InvalidDataException(
                "Plugin assembly escapes its plugin directory.");

        string current = root;

        foreach (string segment in segments)
        {
            current = Path.Combine(current, segment);

            if (!File.Exists(current) && !Directory.Exists(current))
                break;

            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException(
                    "Plugin assembly path contains a symbolic link or reparse point.");
        }

        if (!File.Exists(fullPath))
            throw new FileNotFoundException(
                $"Plugin assembly '{assembly}' was not found.",
                fullPath);

        return fullPath;
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
