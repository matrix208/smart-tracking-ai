using System.Text.RegularExpressions;
using Tracking.Package.Models;

namespace Tracking.Package.Manifest;

public static class ManifestValidator
{
    private static readonly Regex PackageIdPattern = new(
        @"^[a-z0-9][a-z0-9._-]{0,63}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex VersionPattern = new(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex EntryPointPattern = new(
        @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static void Validate(PackageManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        if (string.IsNullOrWhiteSpace(manifest.PackageId) ||
            !PackageIdPattern.IsMatch(manifest.PackageId))
            throw new InvalidOperationException("PackageId is invalid.");

        if (string.IsNullOrWhiteSpace(manifest.DisplayName) ||
            manifest.DisplayName.Length > 128)
            throw new InvalidOperationException("DisplayName is invalid.");

        ValidateVersion(manifest.Version, "Version");

        if (!string.IsNullOrWhiteSpace(manifest.SdkVersion))
            ValidateVersion(manifest.SdkVersion, "SdkVersion");

        if (!string.IsNullOrWhiteSpace(manifest.MinServerVersion))
            ValidateVersion(manifest.MinServerVersion, "MinServerVersion");

        ValidateAssemblyPath(manifest.Assembly);

        if (string.IsNullOrWhiteSpace(manifest.EntryPoint) ||
            !EntryPointPattern.IsMatch(manifest.EntryPoint))
            throw new InvalidOperationException("EntryPoint is invalid.");

        if (!Enum.IsDefined(manifest.Type))
            throw new InvalidOperationException("Package Type is invalid.");

        if (manifest.DefaultPort is < 0 or > 65535)
            throw new InvalidOperationException("DefaultPort is invalid.");

        if (manifest.Permissions is null || manifest.Dependencies is null)
            throw new InvalidOperationException(
                "Permissions and Dependencies must not be null.");

        if (manifest.Permissions.Any(p => !Enum.IsDefined(p)))
            throw new InvalidOperationException(
                "Manifest contains an unknown permission.");

        foreach (var dependency in manifest.Dependencies)
        {
            if (dependency is null ||
                string.IsNullOrWhiteSpace(dependency.PackageId) ||
                !PackageIdPattern.IsMatch(dependency.PackageId))
                throw new InvalidOperationException(
                    "Manifest contains an invalid dependency PackageId.");

            ValidateVersion(dependency.Version, "Dependency Version");
        }
    }

    private static void ValidateVersion(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !VersionPattern.IsMatch(value))
            throw new InvalidOperationException(
                $"{field} must be a valid semantic version.");
    }

    private static void ValidateAssemblyPath(string? assembly)
    {
        if (string.IsNullOrWhiteSpace(assembly) ||
            Path.IsPathRooted(assembly) ||
            assembly.Contains('\\') ||
            assembly.Contains(':') ||
            assembly.Contains('\0'))
            throw new InvalidOperationException(
                "Assembly must be a safe relative DLL path.");

        var segments = assembly.Split('/');

        if (segments.Any(s =>
                string.IsNullOrWhiteSpace(s) || s is "." or ".."))
            throw new InvalidOperationException(
                "Assembly contains an unsafe path segment.");

        if (!assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Assembly must reference a DLL.");
    }
}
