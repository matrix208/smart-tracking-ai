namespace Tracking.PluginLoader.Services;

public sealed class PluginScanner
{
    public IReadOnlyList<string> Scan(string pluginsFolder)
    {
        if (!Directory.Exists(pluginsFolder))
            return [];

        return Directory
            .GetDirectories(pluginsFolder)
            .Where(HasManifest)
            .OrderBy(x => x)
            .ToList();
    }

    private static bool HasManifest(string pluginFolder)
    {
        var canonicalManifest = Path.Combine(
            pluginFolder,
            "Manifest",
            "manifest.json");

        var legacyManifest = Path.Combine(
            pluginFolder,
            "manifest.json");

        return File.Exists(canonicalManifest) ||
               File.Exists(legacyManifest);
    }
}
