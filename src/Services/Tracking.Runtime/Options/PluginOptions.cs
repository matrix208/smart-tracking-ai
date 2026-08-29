namespace Tracking.Runtime.Options;

public sealed class PluginOptions
{
    public const string SectionName = "Plugins";

    public string RootPath { get; set; } = "plugins";
}
