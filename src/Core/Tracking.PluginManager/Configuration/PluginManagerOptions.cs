namespace Tracking.PluginManager.Configuration;

public sealed class PluginManagerOptions
{
    public const string SectionName = "PluginManager";

    public string InstalledPluginsPath { get; set; } = "data/plugins";

    public string RepositoryPath { get; set; } = "repository";

    public bool LoadEnabledOnly { get; set; } = true;
}
