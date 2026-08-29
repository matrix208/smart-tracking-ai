using Tracking.Package.Archive;
using Tracking.Package.Manifest;
using Tracking.Package.Security;
using Tracking.Plugin.Abstractions.Interfaces;
using Tracking.PluginManager.Services;

namespace Tracking.Plugin.Runtime.Installer;

public sealed class PluginInstaller : IPluginInstaller
{
    private readonly InstalledPluginStore _store;
    private readonly string _publicKeyPemPath;
    private readonly bool _requireSignature;

    public PluginInstaller(
        InstalledPluginStore store,
        string publicKeyPemPath,
        bool requireSignature = true)
    {
        _store = store;
        _publicKeyPemPath = publicKeyPemPath;
        _requireSignature = requireSignature;
    }

    public async Task<PluginInstallResult> InstallAsync(
        string packageFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(packageFilePath))
            throw new FileNotFoundException(packageFilePath);

        if (_requireSignature)
        {
            bool verified = SignatureVerifier.VerifyFile(
                packageFilePath,
                _publicKeyPemPath);

            if (!verified)
            {
                throw new InvalidOperationException(
                    $"Package signature verification failed: {packageFilePath}");
            }
        }

        string tempDir = Path.Combine(
            Path.GetTempPath(),
            "plugin-install-" + Guid.NewGuid().ToString("N"));

        try
        {
            PackageArchiveReader.Extract(
                packageFilePath,
                tempDir);

            string manifestPath = ResolveManifestPath(tempDir);

            var manifest = ManifestSerializer.Load(manifestPath);

            ManifestValidator.Validate(manifest);

            cancellationToken.ThrowIfCancellationRequested();

            _store.InstallFromDirectory(
                tempDir,
                manifest.PackageId);

            return new PluginInstallResult
            {
                PluginId = manifest.PackageId,
                Name = manifest.DisplayName,
                Version = manifest.Version
            };
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, recursive: true);
                }
                catch
                {
                }
            }
        }
    }

    public bool Uninstall(string pluginId)
    {
        return _store.Remove(pluginId);
    }

    private static string ResolveManifestPath(string extractedDir)
    {
        var canonical = Path.Combine(
            extractedDir,
            "Manifest",
            "manifest.json");

        if (File.Exists(canonical))
            return canonical;

        var legacy = Path.Combine(
            extractedDir,
            "manifest.json");

        if (File.Exists(legacy))
            return legacy;

        throw new FileNotFoundException(
            "Package does not contain Manifest/manifest.json.",
            canonical);
    }
}
