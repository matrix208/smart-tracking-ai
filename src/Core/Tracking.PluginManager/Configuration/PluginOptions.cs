namespace Tracking.PluginManager.Configuration;

public sealed class PluginOptions
{
    public const string SectionName = "Plugins";

    public string RootPath { get; set; } = "plugins";

    public string RepositoryPath { get; set; } = "repository";
}
