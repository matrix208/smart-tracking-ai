namespace Tracking.Package.Models;

public sealed class PackageManifest
{
    public string PackageId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string SdkVersion { get; set; } = string.Empty;

    public string MinServerVersion { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Company { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public PackageType Type { get; set; }

    public string Assembly { get; set; } = string.Empty;

    public string EntryPoint { get; set; } = string.Empty;

    public string Icon { get; set; } = "icon.png";

    public string Readme { get; set; } = "README.md";

    public string License { get; set; } = "LICENSE";

    public int DefaultPort { get; set; }

    public bool SupportsTcp { get; set; }

    public bool SupportsUdp { get; set; }

    public List<PackagePermission> Permissions { get; set; } = [];

    public List<PackageDependency> Dependencies { get; set; } = [];
}