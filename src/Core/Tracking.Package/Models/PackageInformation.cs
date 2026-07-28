namespace Tracking.Package.Models;

public sealed class PackageInformation
{
    public string PackageId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string InstallPath { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public PackageStatus Status { get; set; }

    public DateTime InstalledAt { get; set; }

    public DateTime LastUpdated { get; set; }
}