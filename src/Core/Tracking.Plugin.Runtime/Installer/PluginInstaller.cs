using System.Security.Cryptography;
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

    private const string RetainedDirectory = ".package";
    private const string RetainedArchive = "package.zip";
    private const string RetainedSignature = "package.zip.sig";

    public PluginInstaller(
        InstalledPluginStore store,
        string publicKeyPemPath)
    {
        _store = store;

        if (string.IsNullOrWhiteSpace(publicKeyPemPath))
            throw new ArgumentException(
                "Public key path is required.",
                nameof(publicKeyPemPath));

        _publicKeyPemPath = Path.GetFullPath(publicKeyPemPath);
    }

    public async Task<PluginInstallResult> InstallAsync(
        string packageFilePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(packageFilePath))
        {
            throw new FileNotFoundException(
                "Plugin package was not found.",
                packageFilePath);
        }

        if (!File.Exists(_publicKeyPemPath))
        {
            throw new FileNotFoundException(
                "Plugin package verification public key was not found.",
                _publicKeyPemPath);
        }

        var sourceSignature = packageFilePath + ".sig";

        if (!File.Exists(sourceSignature))
        {
            throw new InvalidOperationException(
                "Plugin package signature is missing. Installation was rejected.");
        }

        var workspace = Path.Combine(
            Path.GetTempPath(),
            "plugin-install-" + Guid.NewGuid().ToString("N"));

        var payloadDirectory = Path.Combine(
            workspace,
            "payload");

        var retainedArchive = Path.Combine(
            workspace,
            RetainedArchive);

        var retainedSignature = retainedArchive + ".sig";

        try
        {
            Directory.CreateDirectory(workspace);

            // Copy both files first, then verify the exact copies we will use.
            File.Copy(
                packageFilePath,
                retainedArchive,
                overwrite: false);

            File.Copy(
                sourceSignature,
                retainedSignature,
                overwrite: false);

            cancellationToken.ThrowIfCancellationRequested();

            if (!SignatureVerifier.VerifyFile(
                retainedArchive,
                retainedSignature,
                _publicKeyPemPath))
            {
                throw new InvalidOperationException(
                    "Plugin package signature is invalid. Installation was rejected.");
            }

            // Extract only the verified archive.
            PackageArchiveReader.Extract(
                retainedArchive,
                payloadDirectory);

            cancellationToken.ThrowIfCancellationRequested();

            var manifestPath = ResolveManifestPath(payloadDirectory);
            var manifest = ManifestSerializer.Load(manifestPath);

            ManifestValidator.Validate(manifest);

            if (Directory.Exists(
                    Path.Combine(payloadDirectory, RetainedDirectory)) ||
                File.Exists(
                    Path.Combine(payloadDirectory, RetainedDirectory)))
            {
                throw new InvalidDataException(
                    "Package contains a reserved internal path.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Preserve the verified archive and signature outside the signed payload.
            var retainedDirectory = Path.Combine(
                payloadDirectory,
                RetainedDirectory);

            Directory.CreateDirectory(retainedDirectory);

            File.Copy(
                retainedArchive,
                Path.Combine(retainedDirectory, RetainedArchive),
                overwrite: false);

            File.Copy(
                retainedSignature,
                Path.Combine(retainedDirectory, RetainedSignature),
                overwrite: false);

            cancellationToken.ThrowIfCancellationRequested();

            _store.InstallFromDirectory(
                payloadDirectory,
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
            if (Directory.Exists(workspace))
            {
                try
                {
                    Directory.Delete(
                        workspace,
                        recursive: true);
                }
                catch
                {
                    // Do not hide the original installation result.
                }
            }
        }
    }

    public void VerifyInstalled(string installedDirectory)
    {
        if (string.IsNullOrWhiteSpace(installedDirectory) ||
            !Directory.Exists(installedDirectory))
        {
            throw new DirectoryNotFoundException(
                "Installed plugin directory was not found.");
        }

        if (!File.Exists(_publicKeyPemPath))
        {
            throw new FileNotFoundException(
                "Plugin package verification public key was not found.",
                _publicKeyPemPath);
        }

        var root = Path.GetFullPath(installedDirectory);

        var retainedDirectory = Path.Combine(
            root,
            RetainedDirectory);

        if (!Directory.Exists(retainedDirectory))
        {
            throw new InvalidDataException(
                "Installed plugin does not contain its signed package.");
        }

        RejectReparsePoint(retainedDirectory);

        var retainedEntries =
            Directory.GetFileSystemEntries(retainedDirectory);

        var expectedNames = new HashSet<string>(
            StringComparer.Ordinal)
        {
            RetainedArchive,
            RetainedSignature
        };

        if (retainedEntries.Length != 2 ||
            retainedEntries.Any(entry =>
                !expectedNames.Contains(Path.GetFileName(entry))))
        {
            throw new InvalidDataException(
                "Installed package metadata is invalid.");
        }

        var installedArchive = Path.Combine(
            retainedDirectory,
            RetainedArchive);

        var installedSignature = Path.Combine(
            retainedDirectory,
            RetainedSignature);

        RejectReparsePoint(installedArchive);
        RejectReparsePoint(installedSignature);

        var workspace = Path.Combine(
            Path.GetTempPath(),
            "plugin-verify-" + Guid.NewGuid().ToString("N"));

        var verifiedArchive = Path.Combine(
            workspace,
            RetainedArchive);

        var verifiedSignature = verifiedArchive + ".sig";

        var extractedDirectory = Path.Combine(
            workspace,
            "payload");

        try
        {
            Directory.CreateDirectory(workspace);

            // Verify private temporary copies to avoid using a changing source file.
            File.Copy(
                installedArchive,
                verifiedArchive,
                overwrite: false);

            File.Copy(
                installedSignature,
                verifiedSignature,
                overwrite: false);

            if (!SignatureVerifier.VerifyFile(
                verifiedArchive,
                verifiedSignature,
                _publicKeyPemPath))
            {
                throw new InvalidDataException(
                    "Installed plugin package signature verification failed.");
            }

            PackageArchiveReader.Extract(
                verifiedArchive,
                extractedDirectory);

            var installedManifest =
                ManifestSerializer.Load(
                    ResolveManifestPath(root));

            var packageManifest =
                ManifestSerializer.Load(
                    ResolveManifestPath(extractedDirectory));

            ManifestValidator.Validate(packageManifest);

            if (!string.Equals(
                installedManifest.PackageId,
                packageManifest.PackageId,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Installed plugin identity does not match its signed package.");
            }

            var installedFiles = GetFileHashes(
                root,
                excludeRetainedPackage: true);

            var packageFiles = GetFileHashes(
                extractedDirectory,
                excludeRetainedPackage: false);

            if (installedFiles.Count != packageFiles.Count)
            {
                throw new InvalidDataException(
                    "Installed plugin contains missing or unexpected files.");
            }

            foreach (var item in packageFiles)
            {
                if (!installedFiles.TryGetValue(
                    item.Key,
                    out var installedHash))
                {
                    throw new InvalidDataException(
                        $"Installed plugin file is missing: {item.Key}");
                }

                if (!string.Equals(
                    installedHash,
                    item.Value,
                    StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Installed plugin file integrity check failed: {item.Key}");
                }
            }
        }
        finally
        {
            if (Directory.Exists(workspace))
            {
                try
                {
                    Directory.Delete(
                        workspace,
                        recursive: true);
                }
                catch
                {
                    // Do not hide the verification result.
                }
            }
        }
    }

    public bool Uninstall(string pluginId)
    {
        return _store.Remove(pluginId);
    }

    private static Dictionary<string, string> GetFileHashes(
        string root,
        bool excludeRetainedPackage)
    {
        var result = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        var fullRoot = Path.GetFullPath(root);
        var pending = new Stack<string>();

        pending.Push(fullRoot);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            foreach (var entry in Directory.EnumerateFileSystemEntries(current))
            {
                var attributes = File.GetAttributes(entry);

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidDataException(
                        $"Symbolic links and reparse points are not allowed: {entry}");
                }

                bool isDirectory =
                    (attributes & FileAttributes.Directory) != 0;

                if (excludeRetainedPackage &&
                    string.Equals(
                        current,
                        fullRoot,
                        OperatingSystem.IsWindows()
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal) &&
                    string.Equals(
                        Path.GetFileName(entry),
                        RetainedDirectory,
                        StringComparison.Ordinal))
                {
                    if (!isDirectory)
                    {
                        throw new InvalidDataException(
                            "Invalid retained package directory.");
                    }

                    continue;
                }

                if (isDirectory)
                {
                    pending.Push(entry);
                    continue;
                }

                var relativePath = Path.GetRelativePath(
                    fullRoot,
                    entry).Replace('\\', '/');

                using var stream = File.OpenRead(entry);

                var hash = Convert.ToHexString(
                    SHA256.HashData(stream));

                if (!result.TryAdd(relativePath, hash))
                {
                    throw new InvalidDataException(
                        $"Duplicate installed file path: {relativePath}");
                }
            }
        }

        return result;
    }

    private static void RejectReparsePoint(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            throw new FileNotFoundException(
                "Required installed package file was not found.",
                path);
        }

        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException(
                $"Reparse points are not allowed: {path}");
        }
    }

    private static string ResolveManifestPath(string directory)
    {
        var canonical = Path.Combine(
            directory,
            "Manifest",
            "manifest.json");

        if (File.Exists(canonical))
            return canonical;

        var legacy = Path.Combine(
            directory,
            "manifest.json");

        if (File.Exists(legacy))
            return legacy;

        throw new FileNotFoundException(
            "Package does not contain Manifest/manifest.json.",
            canonical);
    }
}
