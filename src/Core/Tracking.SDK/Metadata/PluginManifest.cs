namespace Tracking.SDK.Metadata;

public sealed class PluginManifest
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public string Version { get; set; } = "";

    public string Author { get; set; } = "";

    public string Manufacturer { get; set; } = "";

    public string EntryPoint { get; set; } = "";

    // أضف هذا السطر
    public string Assembly { get; set; } = "";

    public string SdkVersion { get; set; } = "";

    public int DefaultPort { get; set; }

    public bool SupportsTcp { get; set; }

    public bool SupportsUdp { get; set; }

    public List<string> Models { get; set; } = [];

    public List<string> Capabilities { get; set; } = [];
}