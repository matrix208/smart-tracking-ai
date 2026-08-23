using Tracking.Package.Models;

namespace Tracking.Package.Manifest;

public static class ManifestValidator
{
    public static void Validate(PackageManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.PackageId))
            throw new InvalidOperationException("PackageId is required.");

        if (string.IsNullOrWhiteSpace(manifest.DisplayName))
            throw new InvalidOperationException("DisplayName is required.");

        if (string.IsNullOrWhiteSpace(manifest.Version))
            throw new InvalidOperationException("Version is required.");

        if (string.IsNullOrWhiteSpace(manifest.Assembly))
            throw new InvalidOperationException("Assembly is required.");

        if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
            throw new InvalidOperationException("EntryPoint is required.");
    }
}