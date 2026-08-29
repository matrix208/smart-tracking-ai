namespace Tracking.Runtime.Options;

public sealed class PluginRuntimeOptions
{
    public const string SectionName = "PluginManager";

    public string InstalledPluginsPath { get; set; } = "data/plugins";

    public bool LoadEnabledOnly { get; set; } = true;
}
