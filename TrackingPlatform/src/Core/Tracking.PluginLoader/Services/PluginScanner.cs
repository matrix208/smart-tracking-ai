namespace Tracking.PluginLoader.Services;

public sealed class PluginScanner
{
    public IReadOnlyList<string> Scan(string pluginsFolder)
    {
        if (!Directory.Exists(pluginsFolder))
            return [];

        return Directory
            .GetDirectories(pluginsFolder)
            .OrderBy(x => x)
            .ToList();
    }
}