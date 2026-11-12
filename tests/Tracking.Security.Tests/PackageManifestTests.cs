using Tracking.Package.Manifest;
using Tracking.Package.Models;
using Xunit;

namespace Tracking.Security.Tests;

public sealed class PackageManifestTests
{
    [Theory]
    [InlineData("GT06", "gt06", "GT06 Protocol", "Tracking.Plugin.GT06.Plugin.Gt06Plugin", "Tracking.Plugin.GT06.dll")]
    [InlineData("Teltonika", "teltonika", "Teltonika Codec8 Extended", "Tracking.Plugin.Teltonika.Plugin.TeltonikaPlugin", "Tracking.Plugin.Teltonika.dll")]
    public void Repository_manifest_is_loaded_and_validated(
        string folder,
        string expectedId,
        string expectedName,
        string expectedEntryPoint,
        string expectedAssembly)
    {
        var root = FindRepositoryRoot();
        var path = Path.Combine(
            root, "repository", folder, "package", "Manifest", "manifest.json");

        var manifest = ManifestSerializer.Load(path);

        Assert.Equal(expectedId, manifest.PackageId);
        Assert.Equal(expectedName, manifest.DisplayName);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal(expectedEntryPoint, manifest.EntryPoint);
        Assert.Equal(expectedAssembly, manifest.Assembly);

        ManifestValidator.Validate(manifest);
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("0.1.2")]
    [InlineData("2.4.6-beta.1")]
    [InlineData("1.2.3+build.7")]
    public void Valid_semantic_versions_are_accepted(string version)
    {
        var manifest = ValidManifest();
        manifest.Version = version;

        ManifestValidator.Validate(manifest);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1.0")]
    [InlineData("01.0.0")]
    [InlineData("1.0.0.0")]
    [InlineData("v1.0.0")]
    [InlineData("1.0.0-")]
    public void Invalid_semantic_versions_are_rejected(string version)
    {
        var manifest = ValidManifest();
        manifest.Version = version;

        Assert.Throws<InvalidOperationException>(
            () => ManifestValidator.Validate(manifest));
    }

    [Theory]
    [InlineData("../plugin.dll")]
    [InlineData("/tmp/plugin.dll")]
    [InlineData(@"folder\plugin.dll")]
    [InlineData("folder/../plugin.dll")]
    [InlineData("plugin.exe")]
    [InlineData("")]
    public void Unsafe_assembly_paths_are_rejected(string assembly)
    {
        var manifest = ValidManifest();
        manifest.Assembly = assembly;

        Assert.Throws<InvalidOperationException>(
            () => ManifestValidator.Validate(manifest));
    }

    [Theory]
    [InlineData("GT06")]
    [InlineData("Tracking..Plugin")]
    [InlineData("Tracking.Plugin.")]
    [InlineData("Tracking.Plugin;Injected")]
    public void Invalid_entry_points_are_rejected(string entryPoint)
    {
        var manifest = ValidManifest();
        manifest.EntryPoint = entryPoint;

        Assert.Throws<InvalidOperationException>(
            () => ManifestValidator.Validate(manifest));
    }

    [Theory]
    [InlineData("GT06")]
    [InlineData("UPPERCASE")]
    [InlineData("../plugin")]
    [InlineData("")]
    public void Invalid_package_ids_are_rejected(string packageId)
    {
        var manifest = ValidManifest();
        manifest.PackageId = packageId;

        Assert.Throws<InvalidOperationException>(
            () => ManifestValidator.Validate(manifest));
    }

    private static PackageManifest ValidManifest() => new()
    {
        PackageId = "test-plugin",
        DisplayName = "Test Plugin",
        Version = "1.0.0",
        Type = PackageType.Protocol,
        Assembly = "Test.Plugin.dll",
        EntryPoint = "Test.Plugin.EntryPoint"
    };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "repository", "GT06")) &&
                Directory.Exists(Path.Combine(directory.FullName, "repository", "Teltonika")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the TrackingPlatform repository root.");
    }
}
