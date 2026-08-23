namespace Tracking.Package.Models;

public sealed class PackageDependency
{
    public string PackageId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}